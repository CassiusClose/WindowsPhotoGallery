using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Xps;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel associated with the EventsView, which displays a folder list of all years, months, and events.
    /// </summary>
    class EventsViewModel : ViewModelBase
    {
        public EventsViewModel(NavigatorViewModel nav, MediaCollection coll)
        {
            _openGalleryCommand = new RelayCommand(OpenGallery);

            _nav = nav; 
            _collection = coll;

            _folders = new FolderView(coll);
            _folders.FolderOpened += FolderOpened;
        }


        private NavigatorViewModel _nav;

        private MediaCollection _collection;
        private FolderView _folders;

        /// <summary>
        /// A list of folders, one for each year that contains media
        /// </summary>
        public ObservableCollection<FolderLabelViewModel> Folders
        { get { return _folders.Folders; } }




        /**
         * When one of the folders is opened, they will call this
         */
        private void FolderOpened(FolderLabelViewModel folder)
        {
            // If it's an event, open the event's collection
            if (folder.Timestamp.Precision == TimeRange.Second)
            {
                Event? e = GetEventFromFolder(folder, _collection);
                if (e == null)
                    return;
                _nav.NewPage(new GalleryViewModel(e.Name, _nav, e.Collection, null));
            }

            // If it's a time range (year or month), open a new gallery with the collection filtered accordingly
            else 
            {
                _nav.NewPage(new GalleryViewModel(folder.Timestamp.ToString(), _nav, _collection, folder.Timestamp.Precision + 1, (ICollectableViewModel vm) =>
                {
                    if (vm.Timestamp.Matches(folder.Timestamp))
                        return true;
                    return false;
                }));
            }
        }

        //TODO Where does this belong?
        public static Event? GetEventFromFolder(FolderLabelViewModel folder, MediaCollection coll)
        {
            foreach(ICollectable c in coll)
            {
                if (c is Event) { 
                    Event e = (Event)c;
                    if (folder.Timestamp == e.Collection.StartTimestamp && folder.Label == e.Name)
                        return e;

                    Event? result = GetEventFromFolder(folder, e.Collection);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }




        private RelayCommand _openGalleryCommand;
        public ICommand OpenGalleryCommand => _openGalleryCommand;

        public void OpenGallery()
        {
            GalleryViewModel vm = new GalleryViewModel("All Items", _nav, ((MainWindow)System.Windows.Application.Current.MainWindow).Gallery.Collection);
            _nav.NewPage(vm);
        }
    }
}
