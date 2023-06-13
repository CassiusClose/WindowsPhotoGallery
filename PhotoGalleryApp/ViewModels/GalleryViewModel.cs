using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using PhotoGalleryApp.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.Views;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for the PhotoGallery model, displays the photos to the user and lets users interact with them.
    /// </summary>
    class GalleryViewModel : ViewModelBase
    {
        #region Constructors
        public GalleryViewModel(NavigatorViewModel navigator, MediaGallery gallery)
        {
            _navigator = navigator;

            // Init commands
            _addFilesCommand = new RelayCommand(AddFiles);
            _selectMediaCommand = new RelayCommand(SelectMedia);
            _removeMediaCommand = new RelayCommand(RemoveMedia, IsMediaSelected);
            _openMediaCommand = new RelayCommand(OpenMedia);
            _removeTagCommand = new RelayCommand(RemoveTag);
            _saveGalleryCommand = new RelayCommand(SaveGallery);
            _scrollChangedCommand = new RelayCommand(ScrollChanged);
            _escapePressedCommand = new RelayCommand(EscapePressed);

            // Init scroll timer
            _scrollChangedTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
            _scrollChangedTimer.Tick += ScrollChangedStopped;

            // Setup gallery & images
            _gallery = gallery;
            _gallery.MediaTagsChanged += MediaTagsChanged;
            _items = new ObservableCollection<MediaViewModel>();
            ImagesView = CollectionViewSource.GetDefaultView(_items);
           
            FilterTags = new ObservableCollection<string>();
            _gallery.Tags.CollectionChanged += AllTags_CollectionChanged;

            // Setup the filter, after FilterTags has been created
            ImagesView.Filter += MediaFilter;
            FilterTags.CollectionChanged += FilterTags_CollectionChanged;


            // Load all the images in the gallery
            InitAndLoadAllMedia();
        }

        #endregion Constructors



        #region Fields and Properties

        // A reference to the navigator so we can add pages to it
        private NavigatorViewModel _navigator;

        // The collection of photos in the gallery
        private MediaGallery _gallery;
        // The collection of images (image data) to display
        private ObservableCollection<MediaViewModel> _items;

        /// <summary>
        /// The gallery's current items (with any filtering & sorting applied)
        /// </summary>
        public ICollectionView ImagesView { get; }


        /// <summary>
        /// The Gallery's name.
        /// </summary>
        public string GalleryName
        {
            get { return _gallery.Name; }
            set
            {
                _gallery.Name = value;
                OnPropertyChanged();
            }
        }



        /// <summary>
        /// A collection of all tags in the associated gallery.
        /// </summary>
        public ObservableCollection<string> AllTags
        {
            get {
                return _gallery.Tags; 
            }
        }




        private ObservableCollection<string> _filterTags;
        /// <summary>
        /// A collection of the tags currently selected to be displayed.
        /// </summary>
        public ObservableCollection<string> FilterTags
        {
            get { return _filterTags; }
            set
            {
                _filterTags = value;
                OnPropertyChanged();
                Console.WriteLine("FilterTags Setter");
                //ImagesView.Refresh();
            }
        }

        /*
         * When the list of current tags changes, update the property.
         */
        private void FilterTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Any time the collection of tags change, the items show in the gallery will change,
            // but this will not automatically change which items are selected. So here, deselect
            // any selected items that don't match the selected tags.
            DeselectInvalidMedia();
            OnPropertyChanged("FilterTags");
            ImagesView.Refresh();
        }

        private void AllTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("AllTags_CollectionChanged");
            RemoveObsoleteTagsFromFilter();
            OnPropertyChanged("AllTags");
        }

        private void MediaTagsChanged()
        {
            Console.WriteLine("MediaTagsChanged");
            DeselectInvalidMedia();
            ImagesView.Refresh(); 
        }


        /*
         * Stores the MediaViewModel that was last selected in the gallery, allowing for shift-click batch
         * selection. This is only when there is a current selection. If there is nothing currently selected,
         * this will be null.
         */
        private MediaViewModel lastSelectedMedia = null;


        /// <summary>
        /// Stores the size of the gallery's thumbnails. Right now, this is a hard coded value. Later, it will
        /// be an adjustable slider.
        /// </summary>
        public int ThumbnailHeight
        {
            get { return 200; }
        }

        #endregion Fields and Properties



        #region Methods

        /**
         * Creates the list of ImageViewModels of the photos in this gallery. Once the list is created, loads
         * their thumbnails asynchronously.
         */
        private void InitAndLoadAllMedia()
        {
            _items.Clear();

            // Initialize all the image vms first without loading the images. This way,
            // the whole gallery will be available and navigatable to in the slideshow
            // view.
            for (int i = 0; i < _gallery.Count; i++)
            {
                _items.Add(CreateMediaViewModel(_gallery[i]));
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
         * Loads the thumbnail of every image in the gallery. If this function or LoadPriorityImagesThenAll()
         * is called after this function is, this function will stop loading its images.
         */
        private async void LoadAllMedia() 
        {
            // Save this task's ID to a local variable
            uint taskID = ++_imageLoadID;

            // Load the images one at a time
            foreach (MediaViewModel item in _items)
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
         * entire gallery in the background.
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

            // If the task isn't outdated, then resume loading the rest of the gallery
            if (taskID == _imageLoadID)
                LoadAllMedia();
        }




        /// <summary>
        /// Filters the gallery's collection of images based on the selected tags.
        /// If no tags are selected, all images are accepted.
        /// </summary>
        /// <param name="item">The Photo object to accept or reject.</param>
        /// <returns>bool, whether the photo was accepted or not.</returns>
        public bool MediaFilter(object item)
        {
            // If no tags picked, show all of the images 
            if (FilterTags.Count == 0)
                return true;

            MediaViewModel vm = item as MediaViewModel;

            // If the image does not contain any of the selected tags, reject it
            return MediaValidWithFilterTags(vm);
        }


        /*
         * Whether the Media object represented by the given MediaViewModel is valid with
         * the current list of selected tags. I.e., should this Media be displayed in the
         * gallery.
         * 
         * "Valid" means the Media contains all the tags in the currently selected tag list.
         */
        private bool MediaValidWithFilterTags(MediaViewModel vm)
        {
            Media media = vm.Media;
            foreach(string tag in FilterTags)
            {
                if (!media.Tags.Contains(tag))
                    return false;
            }
            return true;
        }


        private List<MediaViewModel> GetCurrentlySelectedItems()
        {
            List<MediaViewModel> selectedItems = new List<MediaViewModel>();

            foreach(MediaViewModel vm in _items)
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

        /*
         * Returns whether any media in the currently visible list are selected.
         */
        private bool IsMediaSelected()
        {
            List<MediaViewModel> list = ImagesView.Cast<MediaViewModel>().ToList();
            foreach (MediaViewModel vm in list)
            {
                if (vm.IsSelected)
                    return true;
            }
            return false;
        }

        /*
         * Selects all media in the currently visible list.
         */
        private void SelectAllMedia()
        {
            List<MediaViewModel> list = ImagesView.Cast<MediaViewModel>().ToList();
            foreach (MediaViewModel vm in list)
            {
                if (!vm.IsSelected)
                    vm.IsSelected = true;
            }
            UpdateSelectedCommandsCanExecute();
        }

        /*
         * Deselects all media in the currently visible list. Media should be deselected
         * when made not visible (doesn't pass the filter), so this method should result
         * in all media being deselected.
         */
        private void DeselectAllMedia()
        {
            List<MediaViewModel> list = ImagesView.Cast<MediaViewModel>().ToList();
            foreach (MediaViewModel vm in list)
            {
                if (vm.IsSelected)
                    vm.IsSelected = false;
            }
            UpdateSelectedCommandsCanExecute();
        }




        /*
         * Remove any tags from the filter that don't exist any longer.
         */
        private void RemoveObsoleteTagsFromFilter()
        {
            for(int i = 0; i < _filterTags.Count; i++)
            {
                if (!AllTags.Contains(_filterTags[i]))
                {
                    _filterTags.RemoveAt(i);
                    i--;
                }
            }

            OnPropertyChanged("FilterTags");
        }
        
        /*
         * Deselect any items that are currently selected, but aren't valid in the filter rules.
         */
        private void DeselectInvalidMedia()
        {
            foreach (MediaViewModel vm in _items)
            {
                if(vm.IsSelected)
                {
                    if(!MediaValidWithFilterTags(vm))
                    {
                        vm.IsSelected = false;
                    }
                }
            }
        }



        /// <summary>
        /// Creates and returns a MediaViewModel object, that holds the given Media object. Will be
        /// either an ImageViewModel or VideoViewModel instance, depending on the type of media the
        /// Media object contains.
        /// </summary>
        /// <param name="media">The Media object that the MediaViewModel will contain.</param>
        /// <returns>A MediaViewModel which contains the Media object, either an ImageViewModel
        /// or VideoViewModel.</returns>
        public MediaViewModel CreateMediaViewModel(Media media)
        {
            switch(media.MediaType)
            {
                case MediaFileType.Video:               
                    // Create the VideoVM in ThumbnailMode, which creates an ImageViewModel
                    // for the thumbnail within the VM.
                    VideoViewModel vvm = new VideoViewModel(media as Video, true, 0, ThumbnailHeight);
                    return vvm;

                case MediaFileType.Image:
                    ImageViewModel ivm = new ImageViewModel(media as Image, 0, ThumbnailHeight);
                    return ivm;

                default:
                    return null;
            }
        }


        #endregion Methods



        #region Commands


        private RelayCommand _addFilesCommand;
        /// <summary>
        /// A command that opens a dialog box and adds the selected image files to the gallery.
        /// </summary>
        public ICommand AddFilesCommand => _addFilesCommand;

        /// <summary>
        /// Opens a dialog box and adds the selected image files to the gallery.
        /// </summary>
        public void AddFiles()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Media files (*.png;*.jpeg;*.jpg,*.mp4)|*.png;*.jpeg;*.jpg;*.mp4";
            if(fileDialog.ShowDialog() == true)
            {
                foreach (string filename in fileDialog.FileNames)
                {
                    if (!Path.HasExtension(filename))
                        continue;

                    Media m;

                    string ext = Path.GetExtension(filename).ToLower();
                    if (ext == ".png" || ext == ".jpeg" || ext == ".jpg")
                        m = new Image(filename);
                    else
                        m = new Video(filename);

                    _gallery.Add(m);

                    MediaViewModel vm = CreateMediaViewModel(m);
                    _imageLoadID++;
                    _items.Add(vm);
                    ScrollChangedStopped(null, null);
                }
            }
        }


        private RelayCommand _openMediaCommand;
        /// <summary>
        /// A command which opens the specified Photo in a new page. Must pass the specified Photo as an argument.
        /// </summary>
        public ICommand OpenMediaCommand => _openMediaCommand;

        /// <summary>
        /// Opens the given Photo in a new page.
        /// </summary>
        /// <param name="parameter">The Photo instance to open.</param>
        public void OpenMedia(object parameter)
        {
            Media p = parameter as Media;
            
            // Get a list of all the currently visible images
            List<MediaViewModel> list = ImagesView.OfType<MediaViewModel>().ToList();
            List<Media> photos = MediaViewModel.GetMediaList(list);

            // Get the clicked on photo's index in the list of Photos
            int index = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (p == list[i].Media)
                    index = i;
            }

            // Create a new page to view the clicked image
            SlideshowViewModel imagePage = new SlideshowViewModel(photos, index, _gallery);
            _navigator.NewPage(imagePage);
        }



        private RelayCommand _saveGalleryCommand;
        /// <summary>
        /// A command which saves the gallery's state to disk.
        /// </summary>
        public ICommand SaveGalleryCommand => _saveGalleryCommand;

        /// <summary>
        /// Saves the gallery's state to disk.
        /// </summary>
        public void SaveGallery()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MediaGallery));
            TextWriter writer = new StreamWriter("gallery.xml");
            serializer.Serialize(writer, _gallery);
            writer.Close();
        }


        #region ScrollChanged Events

        // Stores a copy of the gallery display's ScrollViewer, used to determine what images are currently in view
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
        /// A command that should be called when the ScrollViewer that displays the images in this gallery has its
        /// scroll changed (the user scrolls up or down, or the window size changes). This should be triggered on
        /// the 'ScrollChanged' event.
        /// </summary>
        public ICommand ScrollChangedCommand => _scrollChangedCommand;
        
        /**
         * Called every time the gallery's ScrollViewer's 'ScrollChanged' event is triggered. Here, we restart the
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
         * Called when the user has stopped scrolling on the gallery's ScrollViewer. This will figure out which
         * images are in view of the user and load them.
         */
        private void ScrollChangedStopped(object sender, EventArgs e)
        {
            // Must stop the timer from repeating infinitely
            _scrollChangedTimer.Stop();

            // Get a list of the current images in view
            System.Windows.Controls.ListBox lb = _scrollViewer.Content as System.Windows.Controls.ListBox;
            List<MediaViewModel> list = DisplayUtils.GetVisibleItemsFromListBox(lb, Application.Current.MainWindow).Cast<MediaViewModel>().ToList();

            // Load the images in view, and once this is done, continue loading the rest of the images in the gallery.
            LoadPriorityMediaThenAll(list);
        }

        #endregion ScrollChanged Events



        #region Selected Media Commands

        /*
         * Notifies all commands that deal with selected images that 
         * the selection has changed. This should be called by this VM
         * whenever the selection is changed.
         */
        private void UpdateSelectedCommandsCanExecute()
        {
            _removeMediaCommand.InvokeCanExecuteChanged();
            OnPropertyChanged("MediaSelected");
        }


        private RelayCommand _selectMediaCommand;
        /// <summary>
        /// A command which should be called when an image is selected.
        /// </summary>
        public ICommand SelectMediaCommand => _selectMediaCommand;

        /// <summary>
        /// Notifies the gallery that the selection of images has changed, and handles shift-click
        /// selection.
        /// </summary>
        public void SelectMedia(object parameter)
        {
            MediaViewModel vm = parameter as MediaViewModel;
            // Was the most recent click a select or deselect action
            bool selected = vm.IsSelected;

            List<MediaViewModel> list = ImagesView.Cast<MediaViewModel>().ToList();

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
            UpdateSelectedCommandsCanExecute();
        }



        private RelayCommand _removeMediaCommand;
        /// <summary>
        /// A command which removes the given Photos from the gallery.
        /// </summary>
        public ICommand RemoveMediaCommand => _removeMediaCommand;

        /// <summary>
        /// Removes the given images from the gallery.
        /// </summary>
        /// <param name="parameter">The list of images to remove, of type System.Windows.Controls.SelectedItemCollection (from ListBox SelectedItems).</param>
        public void RemoveMedia(object parameter)
        {
            System.Collections.IList list = parameter as System.Collections.IList;
            List<Media> photos = list.Cast<Media>().ToList();
            foreach (Media p in photos)
                _gallery.Remove(p);
        }


        #endregion Selected Media Commands



        #region Tag Commands

        /// <summary>
        /// An event handler that adds the given tag to the list of selected tags, if it is not already added.
        /// </summary>
        /// <param name="sender">The element that this event was triggered on.</param>
        /// <param name="args">The event's arguments, of type PhotoGalleryApp.Views.ItemChosenEventArgs.</param>
        public void AddTagToFilter(object sender, EventArgs eArgs)
        {
            ItemChosenEventArgs args = (ItemChosenEventArgs)eArgs;
            if(args.Item != null && !FilterTags.Contains(args.Item))
                FilterTags.Add(args.Item);
        }



        /// <summary>
        /// An event handler that adds the given tag to each currently selected image.
        /// </summary>
        /// <param name="sender">The element that this event was triggered on.</param>
        /// <param name="eArgs">The event's arguments, of type PhotoGalleryApp.Views.ItemChosenEventArgs.</param>
        public void AddTagToSelected(object sender, EventArgs eArgs)
        {
            _gallery.DisableTagUpdate = true;
            ItemChosenEventArgs args = (ItemChosenEventArgs)eArgs;
            string tag = args.Item; 
            if (tag != null)
            {
                List<MediaViewModel> vms = GetCurrentlySelectedItems();
                foreach (MediaViewModel vm in vms)
                {
                    if (!vm.Media.Tags.Contains(tag)) 
                    {
                        vm.Media.Tags.Add(tag);
                    }
                }
            }
            _gallery.DisableTagUpdate = false;
            _gallery.UpdateTags();
        }


        public void RemoveTagFromSelected(object sender, EventArgs eArgs)
        {
            _gallery.DisableTagUpdate = true;
            ItemChosenEventArgs args = (ItemChosenEventArgs)eArgs;
            string tag = args.Item;
            if(tag != null)
            {
                List<MediaViewModel> vms = GetCurrentlySelectedItems();
                foreach (MediaViewModel vm in vms)
                {
                    if (vm.Media.Tags.Contains(tag))
                        vm.Media.Tags.Remove(tag);
                }
            }
            _gallery.DisableTagUpdate = false;
            _gallery.UpdateTags();
        }



        private RelayCommand _removeTagCommand;
        /// <summary>
        /// A command which removes a tag from the list of selected tags.
        /// </summary>
        public ICommand RemoveTagCommand => _removeTagCommand;

        /// <summary>
        /// Removes the given tag from the list of selected tags.
        /// </summary>
        /// <param name="parameter">The tag to remove, as a string.</param>
        public void RemoveTag(object parameter)
        {
            FilterTags.Remove(parameter as string);
        }

        #endregion Tag Commands

        #region KeyCommands

        private RelayCommand _escapePressedCommand;
        /// <summary>
        /// A command which handles the escape key being pressed.
        /// </summary>
        public ICommand EscapePressedCommand => _escapePressedCommand;

        /// <summary>
        /// Handle the escape key being pressed. This deselects any selected media.
        /// </summary>
        public void EscapePressed(object parameter)
        {
            if (IsMediaSelected())
            {
                DeselectAllMedia();
            }
        }


        #endregion

        #endregion Commands
    }
}
