using PhotoGalleryApp.Filtering;
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
using System.Reflection.Metadata.Ecma335;
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

            _folders = new YearMonthFolderTree(coll);
            _folders.FolderClicked += FolderOpened;
        }

        public override void Cleanup()
        {
            _folders.FolderClicked -= FolderOpened;
            _folders.Cleanup();
        }


        private NavigatorViewModel _nav;

        private MediaCollection _collection;
        private YearMonthFolderTree _folders;

        /// <summary>
        /// A list of folders, one for each year that contains media
        /// </summary>
        public ObservableCollection<FolderViewModel> Folders
        { get { return _folders.Folders; } }




        /**
         * When one of the folders is opened, they will call this
         */
        private void FolderOpened(FolderViewModel folderVM)
        {
            if (folderVM is not EventFolderViewModel)
                return;

            EventFolderViewModel folder = (EventFolderViewModel)folderVM;


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
                FilterSet filters = new FilterSet(_collection);
                TimeRangeFilter filt = (TimeRangeFilter)filters.GetCriteriaByType(typeof(TimeRangeFilter));
                filt.SetTimeRange(folder.Timestamp, folder.Timestamp);

                _nav.NewPage(new GalleryViewModel(folder.Timestamp.ToString(), _nav, _collection, folder.Timestamp.Precision + 1, filters));
            }
        }

        //TODO Where does this belong?
        public static Event? GetEventFromFolder(EventFolderViewModel folder, MediaCollection coll)
        {
            foreach(ICollectable c in coll)
            {
                if (c is Event) { 
                    Event e = (Event)c;
                    if (folder.Timestamp == e.StartTimestamp && folder.Label == e.Name)
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
            //TODO Better way to access the gallery?
            GalleryViewModel vm = new GalleryViewModel("All Items", _nav, ((MainWindow)System.Windows.Application.Current.MainWindow).Session.Gallery.Collection);
            _nav.NewPage(vm);
        }
    }
}
