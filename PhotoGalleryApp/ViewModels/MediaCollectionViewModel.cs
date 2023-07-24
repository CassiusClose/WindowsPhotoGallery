using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel associated with the MediaCollection model and the view of the same name. The view displays
    /// the media in a scrollable list, with images clickable and selectable. This is intended to be used within
    /// other page views, such as the gallery page. To that end, the class provides several methods for interfacing
    /// with other viewmodels:
    /// - The list of Media can be filtered by adding a filter to the MediaView object.
    /// - There are events that will trigger any callback functions added to them
    ///     - MediaOpened, when the user clicks on a media item.
    ///     - MediaSelectedChanged, when the selection status of at least one media item changes.
    /// </summary>
    class MediaCollectionViewModel : ViewModelBase
    {
        /// <summary>
        /// Creates a new viewmodel associated with the given MediaCollection.
        /// </summary>
        /// <param name="collection">The MediaCollection model to be associated with</param>
        public MediaCollectionViewModel(NavigatorViewModel nav, MediaCollection collection) : this(nav, collection, null) { }

        /// <summary>
        /// Creates a new viewmodel associated with the given MediaCollection. The provided sorting
        /// rule is applied to the items View.
        /// </summary>
        /// <param name="collection">The MediaCollection model to be associated with</param>
        /// <param name="sorting">A SortDescription object describing how to sort the collection of media. Can be null</param>
        public MediaCollectionViewModel(NavigatorViewModel nav, MediaCollection collection, SortDescription? sorting, bool previewMode = false, TimeRange? maxViewLabel=TimeRange.Year)
        {
            _nav = nav;
            _previewMode = previewMode;

            // Init commands
            _selectMediaCommand = new RelayCommand(SelectMedia);
            _scrollChangedCommand = new RelayCommand(ScrollChanged);
            _openMediaCommand = new RelayCommand(OpenMedia);
            _removeTagFromFilterCommand = new RelayCommand(RemoveTagFromFilter);
            _changeThumbnailHeightCommand = new RelayCommand(ChangeThumbnailHeight);
            _createEventCommand = new RelayCommand(CreateNewEvent);


            // Init media lists
            MediaCollectionModel = collection;
            MediaCollectionModel.Tags.CollectionChanged += CollectionTagsChanged;
            MediaCollectionModel.MediaTagsChanged += MediaTagsChanged;
            MediaCollectionModel.CollectionChanged += MediaCollectionChanged;


            // Init the view, which does filtering & sorting
            _view = new MediaView(_nav, MediaCollectionModel, ThumbnailHeight, !_previewMode, maxViewLabel);
            _view.Filter += MediaFilter;
            _view.View.CollectionChanged += View_CollectionChanged;


            EventList_Reset();

            // Setup tag-related things
            FilterTags.CollectionChanged += FilterTagsChanged;

            // Init scroll timer
            _scrollChangedTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
            _scrollChangedTimer.Tick += ScrollChangedStopped;

            // Load all the media in the collection 
            LoadAllMedia();
        }




        #region Fields and Properties

        private NavigatorViewModel _nav;


        /// <summary>
        /// The associated MediaCollection model. This contains Media model classes.
        /// </summary>
        public MediaCollection MediaCollectionModel;

        private MediaView _view;

        public ObservableCollection<ICollectableViewModel> MediaView { 
            get { return _view.View; }
        }





        /// <summary>
        /// Stores the size of the collection's thumbnails. Right now, this is a hard coded value. Later, it will
        /// be an adjustable slider.
        /// </summary>
        private int _thumbnailHeight = 200;
        public int ThumbnailHeight
        {
            get { return _thumbnailHeight; }
            set
            {
                _thumbnailHeight = value;
                OnPropertyChanged();

                // Start another load task
                LoadVisibleMediaThenAll();
            }
        }



        private bool _previewMode = false;
        /// <summary>
        /// Whether the collection should be displayed in preview mode, where the media is not
        /// selectable and there are no options.
        /// </summary>
        public bool PreviewMode
        {
            get { return _previewMode; }
        }

        #endregion Fields and Properties



        #region MediaCollection/View Listeners

        /* When the view changes, deselect any items not longer in the view */
        private void View_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing items from MediaView, but OldItems is null");

                    foreach (ICollectableViewModel vm in e.OldItems)
                        vm.IsSelected = false;

                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing items from MediaView, but OldItems is null");

                    foreach (ICollectableViewModel vm in e.OldItems)
                        vm.IsSelected = false;

                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (ICollectableViewModel vm in _view.AllItems)
                    {
                        if (vm.IsSelected && !MediaView.Contains(vm))
                            vm.IsSelected = false;
                    }
                    break;
            }
        }

        /* When the media collection changes, refresh the load task and update the list of events */
        private void MediaCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            LoadVisibleMediaThenAll();

            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding items to MediaCollection, but NewItems is null");

                    EventList_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing items from MediaCollection, but OldItems is null");

                    EventList_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing items in MediaCollection, but OldItems or NewItems is null");

                    EventList_Add(e.NewItems);
                    EventList_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    EventList_Reset();
                    break;

            }
        }

        #endregion MediaCollection/View Listeners



        #region Tags/Filtering


        /// <summary>
        /// A collection of all tags in the associated gallery.
        /// </summary>
        public ObservableCollection<string> AllTags 
        {
            get {
                return MediaCollectionModel.Tags; 
            }
        }

        /**
         * When AllTags changes, remove any obselete tags from the filter & update the property
         */
        private void CollectionTagsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("AllTags");

            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing tags from MediaCollection, but OldItems is null");

                    CollectionTagsChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems == null)
                        throw new ArgumentException("Replacing tags from MediaCollection, but OldItems is null");

                    CollectionTagsChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    CollectionTagsChanged_Reset();
                    break;
            }
        }

        /**
         * If the filter contains any removed tags, remove them from the filter
         */
        private void CollectionTagsChanged_Remove(IList tags)
        {
            foreach (string tag in tags)
            {
                if (FilterTags.Contains(tag))
                    FilterTags.Remove(tag);
            }
        }

        /**
         * If the AllTags list doesn't contain any filter tags, remove them from the filter
         */
        private void CollectionTagsChanged_Reset()
        {
            foreach (string tag in FilterTags)
            {
                if (!AllTags.Contains(tag))
                    FilterTags.Remove(tag);
            }
        }






        private ObservableCollection<string> _filterTags = new ObservableCollection<string>();
        /// <summary>
        /// A collection of the tags currently selected to be displayed.
        /// </summary>
        public ObservableCollection<string> FilterTags
        {
            get { return _filterTags; }
        }


        /**
         * When FilterTags change, update the view
         */
        private void FilterTagsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding items to FilterTags, but NewItems is null");

                    _view.FilterMoreRestrictive();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing items from FilterTags, but OldItems is null");

                    if (FilterTags.Count == 0)
                        DeselectAllMedia();

                    _view.FilterLessRestrictive();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing items from FilterTags, but OldItems or NewItems is null");

                    _view.FilterMoreRestrictive();
                    _view.FilterLessRestrictive();
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _view.Refresh();
                    break;
            }
        }


        /**
         * If tags change for a media item in the collection, update it's status in the view
         * individually.
         */
        private void MediaTagsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // If the filter is active, no need to do anything.
            //
            // This also covers when the filter is removed due to none of the tags being
            // left in the media. In that case, the FilterTagsChanged handler will update
            // the view and this doesn't need to.
            if (FilterTags.Count == 0)
                return;

            Media media = (Media)sender;
            foreach(ICollectableViewModel vm in _view.AllItems)
            {
                if(vm.GetModel() == media)
                    _view.FilterStatusChanged(vm);
            }
        }



        /// <summary>
        /// Filters the gallery's collection of images based on the selected tags.
        /// If no tags are selected, all images are accepted.
        /// </summary>
        /// <param name="item">The MediaViewModel object to accept or reject.</param>
        /// <returns>bool, whether the media was accepted or not.</returns>
        public bool MediaFilter(ICollectableViewModel vm)
        {
            // If no tags picked, show all of the media
            if (FilterTags.Count == 0)
                return true;

            if (vm is MediaViewModel)
            {
                Media media = ((MediaViewModel)vm).Media;
                foreach (string tag in FilterTags)
                {
                    if (!media.Tags.Contains(tag))
                        return false;
                }
                return true;
            }
            //TODO How to filter events?
            else
                return false;
        }



        /// <summary>
        /// An event handler that adds the given tag to the list of selected tags, if it is not already added.
        /// </summary>
        /// <param name="sender">The element that this event was triggered on.</param>
        /// <param name="args">The event's arguments, of type PhotoGalleryApp.Views.ItemChosenEventArgs.</param>
        public void AddTagToFilter(object sender, EventArgs eArgs)
        {
            PhotoGalleryApp.Views.ItemChosenEventArgs args = (PhotoGalleryApp.Views.ItemChosenEventArgs)eArgs;
            if(args.Item != null && !FilterTags.Contains(args.Item))
                FilterTags.Add(args.Item);
        }


        #endregion Tags/Filtering



        #region Events

        private ObservableCollection<EventTileViewModel> _events = new ObservableCollection<EventTileViewModel>();
        public ObservableCollection<EventTileViewModel> Events { get { return _events; } }


        /**
         * When event is added to the collection, add to the event list
         */
        private void EventList_Add(IList newItems)
        {
            foreach(ICollectable c in newItems)
            {
                if(c is Event)
                    _events.Add(new EventTileViewModel((Event)c, _nav, ThumbnailHeight));
            }
            OnPropertyChanged("Events");
        }

        /**
         * When event is remove from the collection, remove from the event list
         */
        private void EventList_Remove(IList oldItems)
        {
            foreach(ICollectable c in oldItems)
            {
                if(c is Event)
                {
                    for(int i = 0; i < _events.Count; i++)
                    {
                        if (_events[i].GetModel() == c)
                        {
                            _events.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            OnPropertyChanged("Events");
        }

        /** Rebuild the event list */
        private void EventList_Reset()
        {
            _events.Clear();

            foreach(ICollectableViewModel vm in _view.AllItems)
            {
                if(vm is EventTileViewModel)
                {
                    _events.Add((EventTileViewModel)vm);
                }
            }
            OnPropertyChanged("Events");
        }



        private RelayCommand _createEventCommand;
        /// <summary>
        /// Creates an empty event in the media collection
        /// </summary>
        public ICommand CreateEventCommand => _createEventCommand;

        private void CreateNewEvent()
        {
            Event e = new Event("New Event");
            MediaCollectionModel.Add(e);
        }



        /// <summary>
        /// An event handler that adds the currently selected media to the given event.
        /// </summary>
        /// <param name="sender">The element that this event was triggered on.</param>
        /// <param name="eArgs">The event's arguments, of type PhotoGalleryApp.Views.ItemChosenEventArgs.</param>
        public void AddSelectedToEvent(object sender, EventArgs eArgs)
        {
            // Want only one change event to fire, even though we're changing several items in the MediaCollection.
            // So disable updates and then trigger one at the end. See MediaCollection.UpdateTags() for more info.

            PhotoGalleryApp.Views.ItemChosenEventArgs args = (PhotoGalleryApp.Views.ItemChosenEventArgs)eArgs;
            string eventName = args.Item;

            EventTileViewModel? eventvm = null;
            foreach(EventTileViewModel e in Events)
            {
                //TODO Either don't allow events with same name or do the id some other way
                if(e.Name == eventName)
                {
                    eventvm = e;
                    break;
                }
            }
            // If no event exists, create one and add it
            if(eventvm == null)
            {
                Event evnt = new Event(eventName);

                // Add selected items to event
                List<ICollectableViewModel> mvms = GetCurrentlySelectedItems();
                foreach(ICollectableViewModel cvm in mvms)
                {
                    evnt.Collection.Add(cvm.GetModel());
                }

                // Set thumbnail as the first image in the collection
                foreach(ICollectable model in evnt.Collection)
                {
                    if (model is Image)
                    {
                        evnt.Thumbnail = (Image)model;
                        break;
                    }
                }

                // Add the event to the media collection, and remove from this collection
                MediaCollectionModel.Add(evnt);
                foreach (ICollectableViewModel vm in mvms)
                    MediaCollectionModel.Remove(vm.GetModel());
            }

            // If event does exist, add items to it
            else
            {
                bool needsThumbnail = eventvm.Event.Collection.Count == 0;

                // Build lists for batch operations
                List<ICollectableViewModel> mediavms = GetCurrentlySelectedItems();
                //List<ICollectable> media = new List<ICollectable>();
                foreach(ICollectableViewModel cvm in mediavms)
                {
                    //media.Add(cvm.GetModel());
                    ((Event)eventvm.GetModel()).Collection.Add(cvm.GetModel());
                }

                if (needsThumbnail)
                {
                    // Set thumbnail as the first image in the collection
                    foreach(ICollectable model in eventvm.Event.Collection)
                    {
                        if (model is Image)
                        {
                            eventvm.Event.Thumbnail = (Image)model;
                            break;
                        }
                    }
                }

                // Add to event and remove from here
                foreach(ICollectableViewModel vm in mediavms)
                {
                    MediaCollectionModel.Remove(vm.GetModel());
                }
            }
        }

        #endregion Events



        #region Media Selection


        /*
         * Stores the MediaViewModel that was last selected in the collection, allowing for shift-click batch
         * selection. This is only when there is a current selection. If there is nothing currently selected,
         * this will be null.
         */
        private ICollectableViewModel? lastSelectedMedia = null;


        /*
         * Selects all media in the currently visible list.
         */
        public void SelectAllMedia()
        {
            foreach (ICollectableViewModel vm in MediaView)
            {
                if (!vm.IsSelected)
                    vm.IsSelected = true;
            }
            OnPropertyChanged("MediaSelected");
        }

        /// <summary>
        /// Deselects all media in the currently visible list.
        /// </summary>
        public void DeselectAllMedia()
        {
        /*
         * Media should be deselected when made not visible (doesn't pass the filter), so this method should
         * result in all media being deselected, even though it only processes the currently visible ones.
         */
            foreach (ICollectableViewModel vm in MediaView)
            {
                if (vm.IsSelected)
                    vm.IsSelected = false;
            }
            OnPropertyChanged("MediaSelected");
        }


        /// <summary>
        /// Returns a list of the currently selected MediaViewModels.
        /// </summary>
        /// <returns>A list of the currently selected MediaViewModels</returns>
        public List<ICollectableViewModel> GetCurrentlySelectedItems()
        {
            List<ICollectableViewModel> selectedItems = new List<ICollectableViewModel>();

            foreach(ICollectableViewModel vm in MediaView)
            {
                if (vm.IsSelected)
                    selectedItems.Add(vm);
            }

            return selectedItems;
        }


        /// <summary>
        ///  Returns whether there is media selected in the gallery. Acts as a bindable property for
        ///  the IsMediaSelected() method, for the buttons that don't use commands and must bind
        ///  IsEnabled directly.
        /// </summary>
        public bool MediaSelected
        {
            get { return IsMediaSelected(); }
        }

        /// <summary>
        /// Returns whether any media is currently selected.
        /// </summary>
        /// <returns>If any media is selected.</returns>
        public bool IsMediaSelected()
        {
            // Only look at currently visible media. Any media that isn't visible should have be deselected
            foreach (ICollectableViewModel vm in MediaView)
            {
                if (vm.IsSelected)
                    return true;
            }
            return false;
        }

        
        private RelayCommand _selectMediaCommand;
        /// <summary>
        /// A command which should be called when an image is selected.
        /// </summary>
        public ICommand SelectMediaCommand => _selectMediaCommand;

        /// <summary>
        /// Notifies any listeners that the selection of images has changed, and handles shift-click
        /// selection.
        /// </summary>
        public void SelectMedia(object parameter)
        {
            ICollectableViewModel vm = (ICollectableViewModel)parameter;
            // Was the most recent click a select or deselect action
            bool selected = vm.IsSelected;

            List<ICollectableViewModel> list = MediaView.Cast<ICollectableViewModel>().ToList();

            // Shift-clicking should select a range of media
            if(Keyboard.Modifiers == ModifierKeys.Shift)
            {
                // If nothing is selected, shift-click does nothing
                if (lastSelectedMedia == null)
                {
                    vm.IsSelected = false;
                }
                // Otherwise, change selection status for all between prev and currently selected.
                else
                {
                    int lastInd = list.IndexOf(lastSelectedMedia);
                    int currInd = list.IndexOf(vm);
                    int lowInd = lastInd;
                    int hiInd = currInd;
                    if (lastInd > currInd)
                    {
                        lowInd = currInd;
                        hiInd = lastInd;
                    }

                    for (int i = lowInd; i <= hiInd; i++)
                    {
                        if (list[i] is MediaViewModel)
                            list[i].IsSelected = selected;
                    }

                    // When you shift click on an image, that image becomes the basis for later
                    // shift-clicking. This is counter to how it works on Windows File Explorer
                    // and Linux Nautilus.
                    lastSelectedMedia = vm;
                }
            }
            // Normal click
            else
            {
                lastSelectedMedia = vm;
            }

            // If the action deselected all the remaining media, then clear the most recently selected media.
            if (!IsMediaSelected())
                lastSelectedMedia = null;

            OnPropertyChanged("MediaSelected");
        }

        #endregion Media Selection



        #region Commands/Buttons

        #region Tag Buttons

        /// <summary>
        /// An event handler that adds the given tag to each currently selected image.
        /// </summary>
        /// <param name="sender">The element that this event was triggered on.</param>
        /// <param name="eArgs">The event's arguments, of type PhotoGalleryApp.Views.ItemChosenEventArgs.</param>
        public void AddTagToSelected(object sender, EventArgs eArgs)
        {
            // Want only one change event to fire, even though we're changing several items in the MediaCollection.
            // So disable updates and then trigger one at the end. See MediaCollection.UpdateTags() for more info.

            PhotoGalleryApp.Views.ItemChosenEventArgs args = (PhotoGalleryApp.Views.ItemChosenEventArgs)eArgs;
            string tag = args.Item; 
            if (tag != null)
            {
                List<ICollectableViewModel> vms = GetCurrentlySelectedItems();
                foreach (ICollectableViewModel vm in vms)
                { 
                    if(vm is MediaViewModel)
                    { 
                        if (!((MediaViewModel)vm).Media.Tags.Contains(tag)) 
                        {
                            ((MediaViewModel)vm).Media.Tags.Add(tag);
                        }
                    }
                    else
                    {
                        Trace.WriteLine("Error: Events shouldn't be selectable");
                    }
                }
            }
        }


        /// <summary>
        /// An event handler that removes the given tag from each currently selected image.
        /// </summary>
        /// <param name="sender">The element that this event was triggered on.</param>
        /// <param name="eArgs">The event's arguments, of type PhotoGalleryApp.Views.ItemChosenEventArgs.</param>
        public void RemoveTagFromSelected(object sender, EventArgs eArgs)
        {
            // Want only one change event to fire, even though we're changing several items in the MediaCollection.
            // So disable updates and then trigger one at the end. See MediaCollection.UpdateTags() for more info.
            PhotoGalleryApp.Views.ItemChosenEventArgs args = (PhotoGalleryApp.Views.ItemChosenEventArgs)eArgs;
            string tag = args.Item;
            if(tag != null)
            {
                List<ICollectableViewModel> vms = GetCurrentlySelectedItems();
                foreach (ICollectableViewModel vm in vms)
                {
                    if (vm is MediaViewModel && ((MediaViewModel)vm).Media.Tags.Contains(tag))
                    {
                        ((MediaViewModel)vm).Media.Tags.Remove(tag);
                    }
                }
            }
        }



        private RelayCommand _removeTagFromFilterCommand;
        /// <summary>
        /// A command which removes a tag from the list of selected tags.
        /// </summary>
        public ICommand RemoveTagFromFilterCommand => _removeTagFromFilterCommand;

        /// <summary>
        /// Removes the given tag from the list of selected tags.
        /// </summary>
        /// <param name="parameter">The tag to remove, as a string.</param>
        public void RemoveTagFromFilter(object parameter)
        {
            FilterTags.Remove(parameter as string);
            DeselectAllMedia();
        }

        #endregion Tag Buttons

        #region ChangeThumbnailHeight

        private RelayCommand _changeThumbnailHeightCommand;
        public ICommand ChangeThumbnailHeightCommand => _changeThumbnailHeightCommand;

        public void ChangeThumbnailHeight()
        {
            if (ThumbnailHeight == 200)
                ThumbnailHeight = 100;
            else
                ThumbnailHeight = 200;
        }

        #endregion ChangeThumbnailHeight

        #region RemoveSelected

        /// <summary>
        /// Removes the given images from the gallery.
        /// </summary>
        /// <param name="parameter">The list of images to remove, of type System.Windows.Controls.SelectedItemCollection (from ListBox SelectedItems).</param>
        public void RemoveSelected(object sender, EventArgs eArgs)
        {
            List<ICollectableViewModel> vms = GetCurrentlySelectedItems();
            foreach(ICollectableViewModel vm in vms)
            {
                MediaCollectionModel.Remove(vm.GetModel());
            }

            OnPropertyChanged("MediaSelected");
        }
        #endregion RemoveSelected
        
        #region Open Media

        private RelayCommand _openMediaCommand;
        /// <summary>
        /// A command which opens the specified Photo in a new page. Must pass the specified Photo as an argument.
        /// </summary>
        public ICommand OpenMediaCommand => _openMediaCommand;

        /// <summary>
        /// Calls the MediaOpened event callbacks.
        /// </summary>
        /// <param name="parameter">The Media instance to open.</param>
        public void OpenMedia(object parameter)
        {
            ICollectableViewModel item = parameter as ICollectableViewModel;

            //TODO Can be more efficient
            ICollectable m = item.GetModel();
            if (m is Media)
            {
                // Get a list of all the currently filtered images
                List<ICollectableViewModel> list = MediaView.Cast<ICollectableViewModel>().ToList();
                List<Media> media = new List<Media>();
                foreach (ICollectableViewModel vm in list)
                {
                    if (vm is MediaViewModel)
                        media.Add((Media)vm.GetModel());
                }

                // Get the clicked on photo's index in the list of Photos
                int index = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (m == media[i])
                    {
                        index = i;
                        break;
                    }
                }

                // Create a new page to view the clicked image
                SlideshowViewModel imagePage = new SlideshowViewModel(media, index, MediaCollectionModel);
                _nav.NewPage(imagePage);
            }
            else
            {
                // For some reason, pushing a new page with the existing EventViewModel will
                // not display any media recently added to the event. Creating a new vm does
                // work. Which is okay, because soon I'm going to separate the event page vm
                // and the event tile with the MediaCollection view.
                _nav.NewPage(new EventViewModel((Event)item.GetModel(), _nav));
            }
        }

        #endregion Open Media

        #endregion Commands/Buttons



        #region Image Loading



        /**
         * Each image load function call (LoadAllImages() or LoadPriorityImagesThenAll()) has its own ID.
         * This marks the ID of the most recent image load call. These functions will check whether their ID
         * is the most recent, and cancel their loads if not. This way, calling any of the image load functions
         * will cancel any previous image load function calls.
         */
        private uint _imageLoadID = 0;

        /**
         * Loads the thumbnail of every image in the collection, including ones not included in the 
         * filter. If this function or LoadPriorityImagesThenAll() is called after this function is,
         * this function will stop loading its images.
         */
        private async void LoadAllMedia() 
        {
            if (PreviewMode)
                return;

            // Save this task's ID to a local variable
            uint taskID = ++_imageLoadID;

            // Load the images one at a time
            foreach (ICollectableViewModel item in _view.AllItems)
            {
                // If this task is outdated (there's a newer task ID out there), then cancel
                if (taskID != _imageLoadID)
                    break;

                if (item is MediaViewModel)
                    await Task.Run(() => { ((MediaViewModel)item).LoadMedia(); });
                else if (item is EventTileViewModel)
                    await Task.Run(() => { ((EventTileViewModel)item).LoadThumbnail(); });

                if (taskID != _imageLoadID)
                    break;
            }
        }

        /**
         * Loads the thumbnail of every image in the given list. This is used to load specifically the images that 
         * the user can see. Once those images in view have been loaded, this calls LoadAllImages() to resume loading the
         * entire collection in the background.
         */
        private async void LoadPriorityMediaThenAll(List<ICollectableViewModel> items)
        {
            // Save this task's ID to a local variable
            uint taskID = ++_imageLoadID;

            // Load the images one at a time
            foreach (ICollectableViewModel vm in items)
            {
                // If this task is outdated (there's a newer task ID out there), then cancel
                if (taskID != _imageLoadID)
                    break;

                if (vm is MediaViewModel)
                    await Task.Run(() => { ((MediaViewModel)vm).LoadMedia(); });
                else if (vm is EventTileViewModel) 
                    await Task.Run(() => { ((EventTileViewModel)vm).LoadThumbnail(); });

                if (taskID != _imageLoadID)
                    break;
            }

            // If the task isn't outdated, then resume loading the rest of the collection 
            if (taskID == _imageLoadID)
                LoadAllMedia();
        }

        #endregion Image Loading



        #region Scroll Events

        /*
         * This timer is used to decrease the number of ScrollChanged events that are triggered. The issue is that as
         * the user scrolls down, many ScrollChanged events are triggered, not just one when the user stops scrolling.
         * For the purposes of choosing which images to load next, it's not ideal to start many image load tasks that
         * will be cancelled when the next is called. So we use this timer, and only start loading the visible images
         * when the scroll hasn't been changed in a certain amount of time.
         */
        private DispatcherTimer _scrollChangedTimer = new DispatcherTimer();


        private RelayCommand _scrollChangedCommand;
        /// <summary>
        /// A command that should be called when the ScrollViewer that displays the images in this collection has its
        /// scroll changed (the user scrolls up or down, or the window size changes). This should be triggered on
        /// the 'ScrollChanged' event.
        /// </summary>
        public ICommand ScrollChangedCommand => _scrollChangedCommand;

        /**
         * Called every time the collection's ScrollViewer's 'ScrollChanged' event is triggered. Here, we restart the
         * timer, so that ScrollChangedStopped() is only called when the user has finished scrolling. 
         */
        private void ScrollChanged(object parameter)
        {
            // Start/restart the timer
            if (_scrollChangedTimer.IsEnabled)
                _scrollChangedTimer.Stop();
            _scrollChangedTimer.Start();
        }


        /**
         * Called when the user has stopped scrolling on the collections's ScrollViewer. This will figure out which
         * images are in view of the user and load them.
         */
        private void ScrollChangedStopped(object sender, EventArgs e)
        {
            // Must stop the timer from repeating infinitely
            _scrollChangedTimer.Stop();

            LoadVisibleMediaThenAll();
        }

        private void LoadVisibleMediaThenAll()
        {
            // Get a list of the current images in view
            List<ICollectableViewModel> items = _getVisibleItems();

            // Load the images in view, and once this is done, continue loading the rest of the images in the collection.
            LoadPriorityMediaThenAll(items);
        }

        /** Returns a list of items visible in the UI */
        private List<ICollectableViewModel> _getVisibleItems()
        {
            List<ICollectableViewModel> items = new List<ICollectableViewModel>();
            foreach (ICollectableViewModel vm in MediaView)
            {
                if (vm.IsInView)
                    items.Add(vm);
            }

            return items;
        }

        #endregion Scroll Events
    }
}
