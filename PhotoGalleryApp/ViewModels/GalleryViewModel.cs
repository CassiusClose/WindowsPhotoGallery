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
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for the Gallery view. Displays a MediaCollection to the user and lets users view, filter,
    /// and edit them. Holds a MediaCollectionViewModel, which contains the MediaCollection.
    /// </summary>
    class GalleryViewModel : ViewModelBase
    {
        #region Constructors
        public GalleryViewModel(NavigatorViewModel navigator, Gallery gallery)
        {
            _navigator = navigator;
            _gallery = gallery;

            // Init commands
            _addFilesCommand = new RelayCommand(AddFiles);
            _removeTagFromFilterCommand = new RelayCommand(RemoveTagFromFilter);
            _saveGalleryCommand = new RelayCommand(SaveGallery);
            _escapePressedCommand = new RelayCommand(EscapePressed);

            // Init the media collection
            gallery.MediaList.MediaTagsChanged += MediaTagsChanged;
            _mediaCollectionVM = new MediaCollectionViewModel(navigator, gallery.MediaList, new SortDescription("Timestamp", ListSortDirection.Ascending));
            _mediaCollectionVM.MediaSelectedChanged += MediaSelectedChanged;
            _mediaCollectionVM.MediaOpened += MediaOpened;
            gallery.MediaList.CollectionChanged += MediaCollectionChanged;

            // Collect all the events from the media collection for the AddSelectedToEvent
            // drop down
            UpdateEventList();

            // Setup tag-related things
            FilterTags = new ObservableCollection<string>();
            FilterTags.CollectionChanged += FilterTags_CollectionChanged;
            gallery.MediaList.Tags.CollectionChanged += AllTags_CollectionChanged;

            // Setup the filter, after FilterTags has been created
            _mediaCollectionVM.MediaView.Filter += MediaFilter;
        }

        #endregion Constructors



        #region Fields and Properties

        // A reference to the navigator so we can add pages to it
        private NavigatorViewModel _navigator;

        private Gallery _gallery;

        // A reference to the MediaCollectionViewModel so the gallery controls and the collection can interact
        private MediaCollectionViewModel _mediaCollectionVM;
        public MediaCollectionViewModel MediaCollectionVM { get { return _mediaCollectionVM; } }


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

        #endregion Fields and Properties



        #region Tags


        /// <summary>
        /// A collection of all tags in the associated gallery.
        /// </summary>
        public ObservableCollection<string> AllTags
        {
            get {
                return MediaCollectionVM.MediaCollectionModel.Tags; 
            }
        }

        /**
         * When the global list of existing tags changes, update the property. Drop-down lists will need
         * to update, and may need to remove tags from the current filter.
         */
        private void AllTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RemoveObsoleteTagsFromFilter();
            OnPropertyChanged("AllTags");
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
            }
        }

        /**
         * When the list of current filter tags changes, update the property.
         */
        private void FilterTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Any time the collection of tags change, the items show in the gallery will change,
            // but this will not automatically change which items are selected. So here, deselect
            // any selected items that don't match the selected tags.
            DeselectInvalidMedia();

            OnPropertyChanged("FilterTags");
            MediaCollectionVM.MediaView.Refresh();
        }



        /**
         * When one or multiple media's tags change (not necessarily changing the global tag list), then
         * deselect them as necessary according to the filter.
         */
        private void MediaTagsChanged()
        {
            DeselectInvalidMedia();
        }



        /// <summary>
        /// Filters the gallery's collection of images based on the selected tags.
        /// If no tags are selected, all images are accepted.
        /// </summary>
        /// <param name="item">The MediaViewModel object to accept or reject.</param>
        /// <returns>bool, whether the media was accepted or not.</returns>
        public bool MediaFilter(object item)
        {
            // If no tags picked, show all of the media
            if (FilterTags.Count == 0)
                return true;

            ICollectableViewModel vm = item as ICollectableViewModel;

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
        private bool MediaValidWithFilterTags(ICollectableViewModel vm)
        {
            if (vm is MediaViewModel)
            {
                Media media = ((MediaViewModel)vm).Media;
                foreach(string tag in FilterTags)
                {
                    if (!media.Tags.Contains(tag))
                        return false;
                }
                return true;
            }
            else
            {
                //TODO How to filter events?
                return true;
            }
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
            bool changed = false;
            foreach (ICollectableViewModel vm in MediaCollectionVM.GetAllItems())
            {
                if(vm.IsSelected)
                {
                    if(!MediaValidWithFilterTags(vm))
                    {
                        vm.IsSelected = false;
                        changed = true;
                    }
                }
            }
            if (changed)
                /*
                 * Trigger the event in MediaCollectionViewModel, which will then call the MediaSelectionChanged
                 * function here. Don't call that function directly, in case there are other subscribers to the
                 * event that need to be notified.
                 */
                MediaCollectionVM.TriggerMediaSelectedChanged();
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
            MediaCollectionVM.MediaCollectionModel.DisableTagUpdate = true;

            PhotoGalleryApp.Views.ItemChosenEventArgs args = (PhotoGalleryApp.Views.ItemChosenEventArgs)eArgs;
            string tag = args.Item; 
            if (tag != null)
            {
                List<ICollectableViewModel> vms = MediaCollectionVM.GetCurrentlySelectedItems();
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

            MediaCollectionVM.MediaCollectionModel.DisableTagUpdate = false;
            MediaCollectionVM.MediaCollectionModel.UpdateTags();
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
            MediaCollectionVM.MediaCollectionModel.DisableTagUpdate = true;

            PhotoGalleryApp.Views.ItemChosenEventArgs args = (PhotoGalleryApp.Views.ItemChosenEventArgs)eArgs;
            string tag = args.Item;
            if(tag != null)
            {
                List<ICollectableViewModel> vms = MediaCollectionVM.GetCurrentlySelectedItems();
                foreach (ICollectableViewModel vm in vms)
                {
                    if (vm is MediaViewModel)
                    {
                        if (((MediaViewModel)vm).Media.Tags.Contains(tag))
                            ((MediaViewModel)vm).Media.Tags.Remove(tag);
                    }
                }
            }
            
            MediaCollectionVM.MediaCollectionModel.DisableTagUpdate = false;
            MediaCollectionVM.MediaCollectionModel.UpdateTags();
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
        }

        #endregion Tag Buttons

        #endregion Tags


        #region Selection

        /// <summary>
        ///  Returns whether there is media selected in the gallery. Acts as a bindable property for
        ///  the IsMediaSelected() method, for the buttons that don't use commands and must bind
        ///  IsEnabled directly.
        /// </summary>
        public bool MediaSelected
        {
            get { return MediaCollectionVM.IsMediaSelected(); }
        }

        /*
         * Called when the collection's selection status changes, updates the MediaSelected property here.
         */
        private void MediaSelectedChanged()
        {
            OnPropertyChanged("MediaSelected");
        }

        #endregion Selection


        #region Events

        private ObservableCollection<EventTileViewModel> _events = new ObservableCollection<EventTileViewModel>();
        public ObservableCollection<EventTileViewModel> Events { get { return _events; } }

        /* When the media collection changes, rebuild the list of events */
        private void MediaCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateEventList();
        }

        /* Rebuild the list of events in the gallery's MediaCollection */
        private void UpdateEventList()
        {
            //TODO Make more efficient
            _events.Clear();

            foreach(Event e in MediaCollectionVM.MediaCollectionModel.GetEvents())
            {
                _events.Add(new EventTileViewModel(e, _navigator));
            }
            OnPropertyChanged("Events");
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
                }
            }
            if(eventvm == null)
            {
                //TODO Allow creating new events
                return;
                /*evnt = new Event(eventName);
                _gallery.Events.Add(evnt);*/
            }

            // Build lists for batch operations
            List<ICollectableViewModel> mediavms = MediaCollectionVM.GetCurrentlySelectedItems();
            //List<ICollectable> media = new List<ICollectable>();
            foreach(ICollectableViewModel cvm in mediavms)
            {
                //media.Add(cvm.GetModel());
                ((Event)eventvm.GetModel()).Collection.Add(cvm.GetModel());
            }

            // Add to event and remove from here
            //eventvm.MediaCollectionVM.AddMediaItems(media);
            MediaCollectionVM.RemoveMediaItems(mediavms);
        }


        #endregion Events



        #region Commands

        #region AddFiles

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
            List<ICollectable> media = new List<ICollectable>();

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

                    media.Add(m);
                }
            }

            MediaCollectionVM.AddMediaItems(media);
        }

        #endregion AddFiles


        #region OpenMedia

        /* Opens the given Media in a new Slideshow page. Is a callback for MediaCollectionViewModel. */
        private void MediaOpened(ICollectableViewModel item)
        {
            //TODO Can be more efficient
            ICollectable m = item.GetModel();
            if (m is Media)
            {
                // Get a list of all the currently visible images
                List<ICollectableViewModel> list = MediaCollectionVM.MediaView.Cast<ICollectableViewModel>().ToList();
                List<Media> media = new List<Media>(); //= MediaViewModel.GetMediaList(list);
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
                SlideshowViewModel imagePage = new SlideshowViewModel(media, index, MediaCollectionVM.MediaCollectionModel);
                _navigator.NewPage(imagePage);
            }
            else
            {
                // For some reason, pushing a new page with the existing EventViewModel will
                // not display any media recently added to the event. Creating a new vm does
                // work. Which is okay, because soon I'm going to separate the event page vm
                // and the event tile with the MediaCollection view.
                _navigator.NewPage(new EventViewModel((Event)item.GetModel(), _navigator));
            }
        }

        #endregion OpenMedia


        #region SaveGallery

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
            XmlSerializer serializer = new XmlSerializer(typeof(Gallery));
            TextWriter writer = new StreamWriter("gallery.xml");
            serializer.Serialize(writer, _gallery);
            writer.Close();
        }

        #endregion SaveGallery


        #region RemoveSelected

        /// <summary>
        /// Removes the given images from the gallery.
        /// </summary>
        /// <param name="parameter">The list of images to remove, of type System.Windows.Controls.SelectedItemCollection (from ListBox SelectedItems).</param>
        public void RemoveSelected(object sender, EventArgs eArgs)
        {
            MediaCollectionVM.DisableMediaViewRefresh = true;

            List<ICollectableViewModel> vms = MediaCollectionVM.GetCurrentlySelectedItems();
            foreach(ICollectableViewModel vm in vms)
            {
                MediaCollectionVM.MediaCollectionModel.Remove(vm.GetModel());
            }

            MediaCollectionVM.TriggerMediaSelectedChanged();
            MediaCollectionVM.DisableMediaViewRefresh = false;
        }
        #endregion RemoveSelected

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
            if (MediaSelected)
            {
                MediaCollectionVM.DeselectAllMedia();
            }
        }


        #endregion

        #endregion Commands
    }
}
