using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using PhotoGalleryApp.Filtering;
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
    public class MediaCollectionViewModel : ViewModelBase
    {
        /// <summary>
        /// Creates a new viewmodel associated with the given MediaCollection. 
        /// </summary>
        /// <param name="collection">The MediaCollection model to be associated with</param>
        public MediaCollectionViewModel(NavigatorViewModel nav, MediaCollection collection, bool previewMode = false, TimeRange? maxViewLabel=TimeRange.Year, FilterSet? viewFilters=null, bool expandEvents=false)
        {
            _nav = nav;
            _previewMode = previewMode;

            // Init commands
            _selectMediaCommand = new RelayCommand(SelectMedia);
            _scrollChangedCommand = new RelayCommand(ScrollChanged);
            _openMediaCommand = new RelayCommand(OpenMedia);
            _changeThumbnailHeightCommand = new RelayCommand(ChangeThumbnailHeight);
            _createEventCommand = new RelayCommand(CreateNewEvent);
            _addSelectedToEventCommand = new RelayCommand(AddSelectedToEvent);
            _addSelectedToMapCommand = new RelayCommand(AddSelectedToMap);


            // Init media lists
            MediaCollectionModel = collection;

            // Init the view, which does filtering & sorting
            _view = new MediaView(_nav, MediaCollectionModel, ThumbnailHeight, viewFilters, !_previewMode, expandEvents, maxViewLabel);
            _view.View.CollectionChanged += View_CollectionChanged;


            // Init scroll timer
            _scrollChangedTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
            _scrollChangedTimer.Tick += ScrollChangedStopped;

            // Load all the media in the collection 
            ReadyToLoadMedia = true;
            LoadVisibleMediaThenAll();
        }

        public override void Cleanup()
        {
            CancelLoadTasks();
            _scrollChangedTimer.Stop();

            _view.View.CollectionChanged -= View_CollectionChanged;
            _view.Cleanup();
        }




        #region Fields and Properties

        private NavigatorViewModel _nav;


        /// <summary>
        /// The associated MediaCollection model. This contains Media model classes.
        /// </summary>
        public MediaCollection MediaCollectionModel;

        private MediaView _view;

        public MediaView MediaViewClass { get { return _view; } }

        public ObservableCollection<ICollectableViewModel> MediaView { 
            get { return _view.View; }
        }



        /**
         * Delay loading media until everything is set up
         */
        private bool ReadyToLoadMedia = false;



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


        /// <summary>
        /// A collection of all tags in the current user session.
        /// </summary>
        public ObservableCollection<string> AllTags 
        {
            get {
                return MainWindow.GetCurrentSession().Gallery.Collection.Tags;
            }
        }


        public bool ShouldExpandEvents
        {
            get { return _view.ExpandEvents; }
            set {
                // Stop the View_CollectionChanged handler from reloading images, because there
                // will be many updates when expanding all events. Instead, just reload once
                // at the end.
                DisableImageLoad();
                _view.ExpandEvents = value;
                EnableImageLoad();
            }
        }

        #endregion Fields and Properties



        #region MediaCollection/View Listeners

        /**
         * Stops View_CollectionChanged from restarting image load tasks when
         * there's a change in the View. Use this if there's an operation
         * happening in this class where many changes will be made to the view,
         * and you only want one image load task to be done.
         */
        private bool _disableLoadMediaViewCC = false;

        public void DisableImageLoad()
        {
            _disableLoadMediaViewCC = true;
        }
        public void EnableImageLoad(bool reload = true)
        {
            _disableLoadMediaViewCC = false;
            if (reload)
                LoadVisibleMediaThenAll();
        }


        /* When the view changes, deselect any items not longer in the view */
        private void View_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if(!_disableLoadMediaViewCC)
                LoadVisibleMediaThenAll();

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
                    break;
            }
        }

        #endregion MediaCollection/View Listeners


        #region Events

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



        private RelayCommand _addSelectedToEventCommand;
        /// <summary>
        /// Prompts the user to choose the event to add the selected media to
        /// </summary>
        public ICommand AddSelectedToEventCommand => _addSelectedToEventCommand;

        /// <summary>
        ///  Prompt the user to choose an event, or create a new one, and add the selected media 
        ///  to it.
        /// </summary>
        public void AddSelectedToEvent()
        {
            // Open popup, retrieve results
            //TODO Better way to access the gallery?
            EventSelectionPopupViewModel vm = new EventSelectionPopupViewModel(_nav, MainWindow.GetCurrentSession().Gallery.Collection);
            EventSelectionPopupReturnArgs args = (EventSelectionPopupReturnArgs)_nav.OpenPopup(vm);

            // If user cancelled, do nothing
            if (!args.PopupAccepted)
                return;

            // If user chose an event, add media to it
            if(args.Action == EventSelectionPopupReturnArgs.EventType.EventChosen)
            {
                if (args.Event == null) 
                    return;
                Event e = args.Event;

                bool needsThumbnail = e.Collection.Count == 0;

                // Build lists for batch operations
                List<ICollectableViewModel> mediavms = GetCurrentlySelectedItems();
                //List<ICollectable> media = new List<ICollectable>();
                foreach(ICollectableViewModel cvm in mediavms)
                {
                    MediaCollectionModel.Remove(cvm.GetModel());
                    e.Collection.Add(cvm.GetModel());
                }

                if (needsThumbnail)
                {
                    // Set thumbnail as the first image in the collection
                    foreach(ICollectable model in e.Collection)
                    {
                        if (model is Image)
                        {
                            e.Thumbnail = (Image)model;
                            break;
                        }
                    }
                }
            }

            // If user created a new event, create the event, add media to it
            else
            {
                if (args.NewEventName == null)
                    return;

                Event evnt = new Event(args.NewEventName);

                // Add selected items to event
                List<ICollectableViewModel> mvms = GetCurrentlySelectedItems();
                foreach(ICollectableViewModel cvm in mvms)
                {
                    evnt.Collection.Add(cvm.GetModel());
                    MediaCollectionModel.Remove(cvm.GetModel());
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
            }
        }

        #endregion Events



        #region Map


        private RelayCommand _addSelectedToMapCommand;
        public ICommand AddSelectedToMapCommand => _addSelectedToMapCommand;

        /**
         * If the user chooses a MapItem, set all the selected items' MapItem
         * fields to the chosen one. Note that because a Media item can only be
         * associated with one place, this will override any previously
         * selected MapItems.
         */
        public void AddSelectedToMap()
        {
            PickMapItemPopupViewModel popup = new PickMapItemPopupViewModel(_nav);
            PickMapItemPopupReturnArgs args = (PickMapItemPopupReturnArgs)_nav.OpenPopup(popup);

            if(args.PopupAccepted && args.ChosenMapItem != null)
            {
                IList<ICollectableViewModel> list = GetCurrentlySelectedItems();
                foreach(ICollectableViewModel vm in list)
                {
                    if(vm.GetModel() is Media)
                    {
                        Media m = (Media)vm.GetModel();
                        m.MapItem = args.ChosenMapItem;
                    }
                    else if(vm.GetModel() is Event)
                    {
                        Event e = (Event)vm.GetModel();
                        e.AddMapItemToAll(args.ChosenMapItem);
                    }
                }
            }
        }


        #endregion Map



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
                            ((MediaViewModel)vm).Media.Tags.Add(tag);
                    }
                    else if(vm is EventTileViewModel)
                        ((EventTileViewModel)vm).Event.AddTagToAll(tag);
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
            PhotoGalleryApp.Views.ItemChosenEventArgs args = (PhotoGalleryApp.Views.ItemChosenEventArgs)eArgs;
            string tag = args.Item;
            if(tag != null)
            {
                List<ICollectableViewModel> vms = GetCurrentlySelectedItems();
                foreach (ICollectableViewModel vm in vms)
                {
                    if (vm is MediaViewModel && ((MediaViewModel)vm).Media.Tags.Contains(tag))
                        ((MediaViewModel)vm).Media.Tags.Remove(tag);
                    else if(vm is EventTileViewModel)
                        ((EventTileViewModel)vm).Event.RemoveTagFromAll(tag);
                }
            }
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
            if (!ReadyToLoadMedia)
                return;

            if (PreviewMode)
                return;

            // Save this task's ID to a local variable
            uint taskID = ++_imageLoadID;

            // Load the images one at a time
            foreach (ICollectableViewModel item in _view.View)
            {
                // If this task is outdated (there's a newer task ID out there), then cancel
                if (taskID != _imageLoadID)
                    break;

                //ICollectableViewModel item = _view.AllItems[i];
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
            if (!ReadyToLoadMedia)
                return;

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

        /**
         * Stops all current load tasks
         */
        private void CancelLoadTasks()
        {
            _imageLoadID++;
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
