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
                foreach (ICollectable c in _collection)
                {
                    if (c is Event)
                    {
                        Event e = (Event)c;
                        if (folder.Timestamp == e.Collection.StartTimestamp)
                        {
                            _nav.NewPage(new GalleryViewModel(e.Name, _nav, e.Collection, null));
                            return;
                        }
                    }
                }
            }

            // If it's a time range (year or month), collect all the media for that time range and open it
            else 
            {
                List<ICollectable> list = new List<ICollectable>();
                foreach (ICollectable c in _collection)
                {
                    if (c is Event)
                    {
                        if(((Event)c).Collection.TimestampInRange(folder.Timestamp))
                            list.Add(c);
                    }
                    else
                    {
                        if (((Media)c).Timestamp.Matches(folder.Timestamp))
                            list.Add(c);
                    }
                }

                _nav.NewPage(new GalleryViewModel(folder.Timestamp.ToString(),  _nav, new MediaCollection(list), folder.Timestamp.Precision+1));
            }
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
