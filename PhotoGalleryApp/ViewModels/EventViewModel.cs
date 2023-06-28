using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// The viewmodel for a specific Event object.
    /// </summary>
    class EventViewModel : ViewModelBase
    {
        public EventViewModel(Event evnt)
        {
            _event = evnt;

            _mediaCollectionVM = new MediaCollectionViewModel(_event.Collection);
        }

        private Event _event;
        public Event Event { get { return _event; } }


        private MediaCollectionViewModel _mediaCollectionVM;
        /// <summary>
        /// The media belonging to the event
        /// </summary>
        public MediaCollectionViewModel MediaCollectionVM { get { return _mediaCollectionVM; } }

        /// <summary>
        /// The event's display name
        /// </summary>
        public string Name
        {
            get { return _event.Name; }
        }



    }
}
