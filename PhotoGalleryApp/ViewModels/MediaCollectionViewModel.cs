using System;
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
        public MediaCollectionViewModel(NavigatorViewModel nav, MediaCollection collection, SortDescription? sorting, bool previewMode = false)
        {
            _nav = nav;
            _previewMode = previewMode;

            // Init commands
            _selectMediaCommand = new RelayCommand(SelectMedia);
            _scrollChangedCommand = new RelayCommand(ScrollChanged);
            _openMediaCommand = new RelayCommand(OpenMedia);


            // Init media lists
            MediaCollectionModel = collection;
            MediaCollectionModel.MediaTagsChanged += MediaTagsChanged;

            _mediaList = new ObservableCollection<ICollectableViewModel>();

            _mediaView = CollectionViewSource.GetDefaultView(_mediaList);
            if (sorting != null)
                _mediaView.SortDescriptions.Add((SortDescription)sorting);


            // Init scroll timer
            _scrollChangedTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
            _scrollChangedTimer.Tick += ScrollChangedStopped;

            // Load all the media in the collection 
            InitAndLoadAllMedia();
            // Don't need this handler for the initialization of the VMs, so hook it after
            MediaCollectionModel.CollectionChanged += MediaCollection_CollectionChanged;
        }



        private NavigatorViewModel _nav;

        /// <summary>
        /// The associated MediaCollection model. This contains Media model classes.
        /// </summary>
        public MediaCollection MediaCollectionModel;

        /* A list of all the MediaViewModels associated with each Media class in MediaCollection. */
        private ObservableCollection<ICollectableViewModel> _mediaList;

        private ICollectionView _mediaView;

        /// <summary>
        /// A list of the Collection's current items (with any filtering and sorting applied). (Full list is in MediaCollectionModel)
        /// </summary>
        public ICollectionView MediaView { 
            get { return _mediaView; }
        }



        /// <summary>
        /// Prevents the ICollectionView MediaView that provides the view with images from refreshing. There are some
        /// functions, such as removing media from the collection, where a refresh is not needed and only causes
        /// (harmless) binding errors.
        /// </summary>
        public bool DisableMediaViewRefresh = false;

        /*
         * When any media changes its tags, refresh the view
         */
        private void MediaTagsChanged()
        {
            if (!DisableMediaViewRefresh)
                MediaView.Refresh(); 
        }

        /*
         * When Media is removed from the collection, remove the MediaViewModel from the corresponding _mediaList.
         */
        private void MediaCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for(int i = _mediaList.Count()-1; i >= 0; i--)
            {
                ICollectable model = _mediaList[i].GetModel();
                if(!MediaCollectionModel.Contains(model))
                {
                    _mediaList.RemoveAt(i);
                }
            }

            //TODO Should load priority media if that's happening
            LoadAllMedia();
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



        /// <summary>
        /// Returns a list of all the MediaViewModels in the collection.
        /// </summary>
        /// <returns>A list of all the MediaViewModels in the collection</returns>
        public ObservableCollection<ICollectableViewModel> GetAllItems()
        {
            return _mediaList;
        }

        private void _addMediaItem(ICollectable media)
        {
            // Disable Refresh, because it'll be called when adding to the MediaView
            DisableMediaViewRefresh = true;

            ICollectableViewModel vm;
            if (media is Media)
                vm = MediaViewModel.CreateMediaViewModel((Media)media, true, 0, ThumbnailHeight);
            else
                vm = new EventTileViewModel((Event)media, _nav);

            // 1) Add to the MediaCollection first, so the tags are updated
            MediaCollectionModel.Add(media);

            DisableMediaViewRefresh = false;


            // 2) Then update the View, which will cause a Refresh
            _mediaList.Add(vm);
        }

        /// <summary>
        /// Adds a single Media item to the associated MediaCollection. If you have multiple items to
        /// add, call AddMediaItems(). Calling this several times may call Refresh() on the MediaView
        /// multiple times, which will cause binding errors.
        /// </summary>
        /// <param name="media">The Media object to add</param>
        public void AddMediaItem(ICollectable media)
        {
            _addMediaItem(media);

            // Start another load task
            LoadVisibleMediaThenAll();

            // Removing items will cause the view to remove them, but need to call Refresh() here
            // to resort them as well.
            MediaView.Refresh();
        }


        /// <summary>
        /// Adds multiple Media items to the associated MediaCollection
        /// </summary>
        /// <param name="media">The Media object to add</param>
        public void AddMediaItems(List<ICollectable> media)
        {
            // Stop Refresh() from getting called, because adding the item will cause that.
            DisableMediaViewRefresh = true;

            foreach(Media m in media)
            {
                _addMediaItem(m);
            }

            DisableMediaViewRefresh = false;

            // Start another load task
            LoadVisibleMediaThenAll();

            // Removing items will cause the view to remove them, but need to call Refresh() her/
            // to resort them as well.
            MediaView.Refresh();
        }
        

        private void _removeMediaItem(ICollectableViewModel vm)
        {
            // Disable Refresh, because it'll be called when adding to the MediaView
            DisableMediaViewRefresh = true;

            // 1) Add to the MediaCollection first, so the tags are updated
            MediaCollectionModel.Remove(vm.GetModel());

            DisableMediaViewRefresh = false;

            // 2) Then update the View, which will cause a Refresh
            _mediaList.Remove(vm);

        }

        public void RemoveMediaItem(ICollectableViewModel vm)
        {
            _removeMediaItem(vm);

            // Start another load task
            LoadVisibleMediaThenAll();

            // Removing items will cause the view to remove them, but need to call Refresh() here
            // to resort them as well.
            MediaView.Refresh();
        }

        public void RemoveMediaItems(List<ICollectableViewModel> vms)
        {
            DisableMediaViewRefresh = true;

            foreach(ICollectableViewModel vm in vms)
            {
                _removeMediaItem(vm);
            }

            DisableMediaViewRefresh = false;

            // Start another load task
            LoadVisibleMediaThenAll();

            // Removing items will cause the view to remove them, but need to call Refresh() here
            // to resort them as well.
            MediaView.Refresh();
        }


        #region Open Media

        public delegate void CallbackMediaOpened(ICollectableViewModel media);
        /// <summary>
        /// An event triggered when the user opens (clicks on) one of the Media items in the collection.
        /// </summary>
        public event CallbackMediaOpened MediaOpened;


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
            ICollectableViewModel p = parameter as ICollectableViewModel;
            if(MediaOpened != null)
                MediaOpened(p);
        }

        #endregion Open Media


        #region Media Selection

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
            MediaSelectedChanged();
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
            MediaSelectedChanged();
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


        /*
         * Stores the MediaViewModel that was last selected in the collection, allowing for shift-click batch
         * selection. This is only when there is a current selection. If there is nothing currently selected,
         * this will be null.
         */
        private ICollectableViewModel? lastSelectedMedia = null;


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



        public delegate void CallbackMediaSelectedChanged();
        /// <summary>
        /// An event triggered when the selection of Media items in the collection changes.
        /// </summary>
        public event CallbackMediaSelectedChanged MediaSelectedChanged;

        //TODO Or should this be done by subscribing to IsSelected properties and detecting change?
        /// <summary>
        /// Triggers the MediaSelectedChanged event. Used for when an outside class changes
        /// the selection status of at least one Media item.
        /// </summary>
        public void TriggerMediaSelectedChanged() { MediaSelectedChanged(); }


        
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

            // Update the selection-sensitive commands to enable or disable them.
            if(MediaSelectedChanged != null)
                MediaSelectedChanged();
        }

        #endregion Media Selection


        #region Image Loading

        /**
         * Creates the list of MediaViewModels of the media in this collection. Once the list is created, loads
         * their thumbnails asynchronously.
         */
        private void InitAndLoadAllMedia()
        {
            _mediaList.Clear();

            // Initialize all the image vms first without loading the images. This way,
            // the whole collection will be available and navigable to in the slideshow
            // view.
            for (int i = 0; i < MediaCollectionModel.Count; i++)
            {
                if (MediaCollectionModel[i] is Media)
                    _mediaList.Add(MediaViewModel.CreateMediaViewModel((Media)MediaCollectionModel[i], true, 0, ThumbnailHeight));
                else
                    _mediaList.Add(new EventTileViewModel((Event)MediaCollectionModel[i], _nav));
            }

            LoadAllMedia();
        }


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
            foreach (ICollectableViewModel item in _mediaList)
            {
                // If this task is outdated (there's a newer task ID out there), then cancel
                if (taskID != _imageLoadID)
                    break;

                if (item is MediaViewModel)
                    await Task.Run(() => { ((MediaViewModel)item).LoadMedia(); });
                else
                    await Task.Run(() => { ((EventTileViewModel)item).LoadThumbnail(ThumbnailHeight); });

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
                else
                    await Task.Run(() => { ((EventTileViewModel)vm).LoadThumbnail(ThumbnailHeight); });

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

        private List<ICollectableViewModel> _getVisibleItems()
        {
            List<ICollectableViewModel> items = new List<ICollectableViewModel>();
            foreach (ICollectableViewModel vm in _mediaView)
            {
                if (vm.IsInView)
                    items.Add(vm);
            }

            return items;
        }
        
        #endregion Scroll Events
    }
}
