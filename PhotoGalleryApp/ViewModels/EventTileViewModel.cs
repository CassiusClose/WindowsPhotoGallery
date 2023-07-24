using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps.Packaging;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// The viewmodel for a tile display of a specific Event object, such as when
    /// displaying a MediaCollection.
    /// </summary>
    class EventTileViewModel : ICollectableViewModel
    {
        public EventTileViewModel(Event evnt, NavigatorViewModel nav, int thumbnailHeight)
        {
            _nav = nav;
            _event = evnt;
            _thumbnailHeight = thumbnailHeight;

            _event.PropertyChanged += Event_PropertyChanged;
            ((INotifyPropertyChanged)evnt.Collection).PropertyChanged += Collection_PropertyChanged;

            // If thumbnail already exists, create a VM for it
            InitThumbnail();
        }

        /// <summary>
        /// Returns the ICollectable model associated with this viewmodel
        /// </summary>
        /// <returns>The ICollectable model associated with this viewmodel</returns>
        public override ICollectable GetModel()
        {
            return Event;
        }


        private NavigatorViewModel _nav;


        private Event _event;
        public Event Event { get { return _event; } }


        /// <summary>
        /// The event's display name
        /// </summary>
        public string Name
        {
            get { return _event.Name; }
        }

        /// <summary>
        /// The event's timestamp used for sorting, the earliest timestamp of its media
        /// </summary>
        protected override PrecisionDateTime _getTimestamp()
        {
            return _event.Collection.StartTimestamp; 

        }


        private int _thumbnailHeight;

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

        /// <summary>
        /// If the event's Thumbnail property is set, create a VM for it
        /// </summary>
        public void InitThumbnail()
        {
            if(_event.Thumbnail != null)
            {
                if(_thumbnail == null || _thumbnail.Media != _event.Thumbnail) 
                    Thumbnail = MediaViewModel.CreateMediaViewModel(_event.Thumbnail, true, 0, _thumbnailHeight);
            }
        }


        /// <summary>
        /// Load the image into memory if it hasn't been.
        /// </summary>
        /// <param name="thumbnailHeight">The height at which to load the thumbnail</param>
        public void LoadThumbnail()
        {
            if (Thumbnail != null)
                Thumbnail.LoadMedia();
        }

        private void Event_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_event.Thumbnail))
                InitThumbnail();
        }

        private void Collection_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_event.Collection.StartTimestamp))
                OnPropertyChanged(nameof(Timestamp));
        }
    }
}
