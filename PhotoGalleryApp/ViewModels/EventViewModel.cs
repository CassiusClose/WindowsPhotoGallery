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
using PhotoGalleryApp.Utils;

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
            ((INotifyPropertyChanged)_event).PropertyChanged += Collection_PropertyChanged;
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
        public PrecisionDateTime StartTimestamp
        {
            get {
                return _event.Collection.StartTimestamp; 
            }
        }

        public PrecisionDateTime EndTimestamp
        {
            get { 
                return _event.Collection.EndTimestamp; 
            }
        }

        public string TimeRangeDisplay
        {
            get {
                string start = StartTimestamp.Month.ToString() + "/" + StartTimestamp.Day.ToString() + "/" + StartTimestamp.Year.ToString();

                if(StartTimestamp.Year == EndTimestamp.Year && StartTimestamp.Month == EndTimestamp.Month && StartTimestamp.Day == EndTimestamp.Day)
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
            _nav.NewPage(new GalleryViewModel(Event.Name, _nav, Event.Collection));
        }

        private void Collection_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            MediaCollection coll = (MediaCollection)sender;

            if(e.PropertyName == nameof(coll.StartTimestamp))
                OnPropertyChanged(nameof(StartTimestamp));

            if(e.PropertyName == nameof(coll.EndTimestamp))
                OnPropertyChanged(nameof(EndTimestamp));
        }
    }
}
