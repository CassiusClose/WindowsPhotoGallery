using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Xps.Packaging;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// The viewmodel for a specific Event object.
    /// </summary>
    class EventViewModel : ViewModelBase
    {
        public EventViewModel(Event evnt, NavigatorViewModel nav)
        {
            _nav = nav;
            _event = evnt;

            _openCollectionCommand = new RelayCommand(OpenCollection);

            _mediaCollection = new MediaCollectionViewModel(nav, _event.Collection, new SortDescription("Timestamp", ListSortDirection.Ascending), true);
            _mediaCollection.ThumbnailHeight = 150;
        }


        private NavigatorViewModel _nav;


        private Event _event;
        public Event Event { get { return _event; } }


        private MediaCollectionViewModel _mediaCollection;
        /// <summary>
        /// The media belonging to the event
        /// </summary>
        public MediaCollectionViewModel MediaCollection { get { return _mediaCollection; } }




        /// <summary>
        /// The event's display name
        /// </summary>
        public string Name
        {
            get { return _event.Name; }
            set { 
                _event.Name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The event's timestamp used for sorting, the earliest timestamp of its media
        /// </summary>
        public DateTime Timestamp
        {
            get { return _event.StartTimestamp; }
        }

        public DateTime EndTimestamp
        {
            get { return _event.EndTimestamp; }
        }

        public string TimeRangeDisplay
        {
            get {
                string start = Timestamp.Month.ToString() + "/" + Timestamp.Day.ToString() + "/" + Timestamp.Year.ToString();

                if(Timestamp.Year == EndTimestamp.Year && Timestamp.Month == EndTimestamp.Month && Timestamp.Day == EndTimestamp.Day)
                {
                    return start;
                }
                else
                {
                    string end = EndTimestamp.Month.ToString() + "/" + EndTimestamp.Day.ToString() + "/" + EndTimestamp.Year.ToString();
                    return start + " - " + end;
                }
            }
        }


        private MediaViewModel? _thumbnail = null;
        /// <summary>
        /// The viewmodel for the event's Thumbnail. If null, there is either no thumbnail 
        /// chosen for the event, or no class has required the thumbnail to be loaded yet.
        /// </summary>
        public MediaViewModel? Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _openCollectionCommand;
        public ICommand OpenCollectionCommand => _openCollectionCommand;

        public void OpenCollection()
        {
            _nav.NewPage(new GalleryViewModel(_nav, Event.Collection));
        }
    }
}
