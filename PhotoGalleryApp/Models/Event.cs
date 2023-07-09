using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Represents a singular event that happened and has media associated with it. Should have a range of time
    /// associated with it in the future.
    /// </summary>
    public class Event : ICollectable
    {
        /* Need this for the XML Deserialization */
        public Event() : this("Event") { }

        /// <summary>
        /// Creates a new Event object.
        /// </summary>
        /// <param name="name">The display name of the event</param>
        public Event(string name)
        {
            _name = name;
            _collection = new MediaCollection();
            _collection.CollectionChanged += _mediaChanged;
        }



        #region Fields and Properties

        private MediaCollection _collection;
        /// <summary>
        /// The media associated with the event
        /// </summary>
        public MediaCollection Collection { 
            get { return _collection; }
            set { _collection = value; }
        }


        private string _name;
        /// <summary>
        /// The display name of the event
        /// </summary>
        public string Name { 
            get { return _name; } 
            set { _name = value; }
        }

        private Media? _thumbnail = null;
        /// <summary>
        /// The thumbnail to represent the event when viewed as a small tile. If null, there is
        /// no selected thumbnail.
        /// </summary>
        public Media? Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value; }
        }


        private DateTime _startTimestamp;
        /// <summary>
        /// The timestamp of the earliest media in the event.
        /// </summary>
        //TODO What if nothing in the event? Right now, default is year 1.
        public DateTime StartTimestamp
        {
            get { return _startTimestamp; }
        }


        private DateTime _endTimestamp;
        public DateTime EndTimestamp
        {
            get { return _endTimestamp; }
        }


        #endregion Fields and Properties


        /* 
         * When changes in the collection, recalculate the time-range of the event.
         */
        private void _mediaChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //TODO For efficiency, detect what has changed and only use that to update timestamp
            DateTime? earliest = null;
            DateTime? latest = null;
            foreach(ICollectable c in _collection)
            {
                if(c is Media)
                {
                    DateTime t = ((Media)c).Timestamp;
                    if (t < earliest || earliest == null)
                        earliest = t;
                    if (t > latest || latest == null)
                        latest = t;
                }
                else
                {
                    DateTime t = ((Event)c).StartTimestamp;
                    if (t < earliest || earliest == null)
                        earliest = t;
                    if (t > latest || latest == null)
                        latest = t;
                }
            }

            //TODO In a case like this, does anything update the viewmodels? Presumably the change action
            // will come from a viewmodel, but what if there's another viewmodel that needs an update?
            if(earliest != null)
                _startTimestamp = (DateTime)earliest;
            if (latest != null)
                _endTimestamp = (DateTime)latest;
        }


        /// <summary>
        /// Returns whether the class holds an media item or a collection of items. This returns true.
        /// </summary>
        /// <returns>Whether the class holds an media item or a collection of items</returns>
        public override bool IsCollection()
        {
            return true;
        }

    }
}
