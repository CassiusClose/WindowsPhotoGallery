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
            _removeMediaCommand = new RelayCommand(RemoveMedia, AreImagesSelected);
            _openMediaCommand = new RelayCommand(OpenMedia);
            _removeTagCommand = new RelayCommand(RemoveTag);
            _saveGalleryCommand = new RelayCommand(SaveGallery);
            _scrollChangedCommand = new RelayCommand(ScrollChanged);

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
                ImagesView.Refresh();
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

            OnPropertyChanged("FilterTags");
            ImagesView.Refresh();
        }

        private void AllTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RemoveObsoleteTagsFromFilter();
            OnPropertyChanged("AllTags");
        }

        private void MediaTagsChanged()
        {
            ImagesView.Refresh(); 
        }



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
                    return new VideoViewModel(media as Video, true, 0, ThumbnailHeight);

                case MediaFileType.Image:
                    return new ImageViewModel(media as Image, 0, ThumbnailHeight);

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
         * the selection has changed.
         */
        private void UpdateSelectedCommandsCanExecute()
        {
            _removeMediaCommand.InvokeCanExecuteChanged();
        }

        /*
         * A CanExecuteAction function for any command that deals with selected images.
         * The command will be allowed to execute if at least one image in the gallery
         * is selected.
         */
        private bool AreImagesSelected(object parameter)
        {
            System.Collections.IList list = parameter as System.Collections.IList;
            if (list.Count > 0)
                return true;
            return false;
        }



        private RelayCommand _selectMediaCommand;
        /// <summary>
        /// A command which should be called when an image is selected.
        /// </summary>
        public ICommand SelectMediaCommand => _selectMediaCommand;

        /// <summary>
        /// Notifies the gallery that the selection of images has changed.
        /// </summary>
        public void SelectMedia()
        {
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


        #endregion Commands
    }
}
