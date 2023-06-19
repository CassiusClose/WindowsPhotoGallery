using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public MediaCollectionViewModel(MediaCollection collection)
        {
            // Init commands
            _selectMediaCommand = new RelayCommand(SelectMedia);
            _scrollChangedCommand = new RelayCommand(ScrollChanged);
            _openMediaCommand = new RelayCommand(OpenMedia);


            // Init media lists
            MediaCollectionModel = collection;
            _mediaList = new ObservableCollection<MediaViewModel>();
            MediaView = CollectionViewSource.GetDefaultView(_mediaList);

            // Init scroll timer
            _scrollChangedTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
            _scrollChangedTimer.Tick += ScrollChangedStopped;

            // Load all the media in the collection 
            InitAndLoadAllMedia();
        }


        /// <summary>
        /// The associated MediaCollection model. This contains Media model classes.
        /// </summary>
        public MediaCollection MediaCollectionModel;

        /* A list of all the MediaViewModels associated with each Media class in MediaCollection. */
        private ObservableCollection<MediaViewModel> _mediaList;

        /// <summary>
        /// A list of the Collection's current items (with any filtering and sorting applied). (Full list is in MediaCollectionModel)
        /// </summary>
        public ICollectionView MediaView { get; }




        /// <summary>
        /// Stores the size of the collection's thumbnails. Right now, this is a hard coded value. Later, it will
        /// be an adjustable slider.
        /// </summary>
        public int ThumbnailHeight
        {
            get { return 200; }
        }





        /// <summary>
        /// Returns a list of all the MediaViewModels in the collection.
        /// </summary>
        /// <returns>A list of all the MediaViewModels in the collection</returns>
        public ObservableCollection<MediaViewModel> GetAllItems()
        {
            return _mediaList;
        }


        //TODO This should update the MediaCollection as well.
        public void AddMediaItem(MediaViewModel vm)
        {
            _imageLoadID++;
            _mediaList.Add(vm);
            ScrollChangedStopped(null, null);
        }




        #region Open Media

        public delegate void CallbackMediaOpened(Media media);
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
            Media p = parameter as Media;
            MediaOpened(p);
        }

        #endregion Open Media


        #region Media Selection

        /*
         * Selects all media in the currently visible list.
         */
        public void SelectAllMedia()
        {
            foreach (MediaViewModel vm in MediaView)
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
            foreach (MediaViewModel vm in MediaView)
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
        public List<MediaViewModel> GetCurrentlySelectedItems()
        {
            List<MediaViewModel> selectedItems = new List<MediaViewModel>();

            foreach(MediaViewModel vm in MediaView)
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
        private MediaViewModel lastSelectedMedia = null;


        /// <summary>
        /// Returns whether any media is currently selected.
        /// </summary>
        /// <returns>If any media is selected.</returns>
        public bool IsMediaSelected()
        {
            // Only look at currently visible media. Any media that isn't visible should have be deselected
            foreach (MediaViewModel vm in MediaView)
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
            MediaViewModel vm = parameter as MediaViewModel;
            // Was the most recent click a select or deselect action
            bool selected = vm.IsSelected;

            List<MediaViewModel> list = MediaView.Cast<MediaViewModel>().ToList();

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
                _mediaList.Add(MediaViewModel.CreateMediaViewModel(MediaCollectionModel[i], true, 0, ThumbnailHeight));
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
         * Loads the thumbnail of every image in the collection. If this function or LoadPriorityImagesThenAll()
         * is called after this function is, this function will stop loading its images.
         */
        private async void LoadAllMedia() 
        {
            // Save this task's ID to a local variable
            uint taskID = ++_imageLoadID;

            // Load the images one at a time
            foreach (MediaViewModel item in _mediaList)
            {
                // If this task is outdated (there's a newer task ID out there), then cancel
                if (taskID != _imageLoadID)
                    break;

                await Task.Run(() => { item.LoadMedia(); });

                if (taskID != _imageLoadID)
                    break;
            }
        }

        /**
         * Loads the thumbnail of every image in the given list. This is used to load specifically the images that 
         * the user can see. Once those images in view have been loaded, this calls LoadAllImages() to resume loading the
         * entire collection in the background.
         */
        private async void LoadPriorityMediaThenAll(List<MediaViewModel> images)
        {
            // Save this task's ID to a local variable
            uint taskID = ++_imageLoadID;

            // Load the images one at a time
            foreach (MediaViewModel item in images)
            {
                if (taskID == _imageLoadID)
                    await Task.Run(() => { item.LoadMedia(); });
                // If this task is outdated (there's a newer task ID out there), then cancel
                else
                    break;
            }

            // If the task isn't outdated, then resume loading the rest of the collection 
            if (taskID == _imageLoadID)
                LoadAllMedia();
        }

        #endregion Image Loading


        #region Scroll Events

        // TODO Avoid this?
        // Stores a copy of the collection display's ScrollViewer, used to determine what images are currently in view
        private System.Windows.Controls.ScrollViewer _scrollViewer;

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
            // Save the ScrollViewer object locally so that ScrollChangedStopped can use it
            _scrollViewer = parameter as System.Windows.Controls.ScrollViewer;

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

            // Get a list of the current images in view
            System.Windows.Controls.ListBox lb = _scrollViewer.Content as System.Windows.Controls.ListBox;
            List<MediaViewModel> list = DisplayUtils.GetVisibleItemsFromListBox(lb, Application.Current.MainWindow).Cast<MediaViewModel>().ToList();

            // Load the images in view, and once this is done, continue loading the rest of the images in the collection.
            LoadPriorityMediaThenAll(list);
        }
        
        #endregion Scroll Events
    }
}
