using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
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
            set { 
                _thumbnail = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// The timestamp of the earliest media in the event.
        /// </summary>
        //TODO What if nothing in the event? Right now, default is year 1.
        private DateTime? _startTimestamp = null;
        public DateTime? StartTimestamp
        {
            get { return _startTimestamp; }
            set
            {
                _startTimestamp = value;
                OnPropertyChanged();
            }
        }


        //TODO Implement ISerializable and make internal set
        private DateTime? _endTimestamp = null;
        public DateTime? EndTimestamp
        {
            get; set;
        }


        #endregion Fields and Properties


        /* 
         * When changes in the collection, recalculate the time-range of the event.
         */
        private void _mediaChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding items to Event MediaCollection, but NewItems is null");

                    _mediaChanged_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing items from Event MediaCollection, but OldItems is null");

                    _mediaChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing items to Event MediaCollection, but NewItems or OldItems is null");

                    _mediaChanged_Add(e.NewItems);
                    _mediaChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _mediaChanged_Reset();
                    break;
            }
        }

        private void _mediaChanged_Add(IList newItems)
        {
            foreach(ICollectable c in newItems)
            {
                c.PropertyChanged += Media_PropertyChanged;

                if(c is Media)
                {
                    Media m = (Media)c;
                    if (StartTimestamp == null || m.Timestamp < StartTimestamp)
                        StartTimestamp = m.Timestamp;
                    if (EndTimestamp == null || m.Timestamp > EndTimestamp)
                        EndTimestamp = m.Timestamp;
                }
                if(c is Event)
                {
                    Event e = (Event)c;
                    if(StartTimestamp == null || e.StartTimestamp < StartTimestamp)
                        StartTimestamp = e.StartTimestamp;
                    if(EndTimestamp == null || e.EndTimestamp > EndTimestamp)
                        EndTimestamp = e.EndTimestamp;
                }
            }
        }

        private void _mediaChanged_Remove(IList oldItems)
        {
            bool needsReset = false;

            foreach(ICollectable c in oldItems)
            {
                if(c is Media)
                {
                    Media m = (Media)c;
                    if (m.Timestamp.Equals(StartTimestamp) || m.Timestamp.Equals(EndTimestamp))
                        needsReset = true;
                }
                else
                {
                    Event e = (Event)c;
                    if (e.StartTimestamp.Equals(StartTimestamp) || e.EndTimestamp.Equals(EndTimestamp))
                        needsReset = true;
                }
            }

            if (needsReset)
                _mediaChanged_Reset();
        }

        private void _mediaChanged_Reset()
        {
            DateTime? earliest = null;
            DateTime? latest = null;
            foreach(ICollectable c in _collection)
            {
                c.PropertyChanged -= Media_PropertyChanged;
                c.PropertyChanged += Media_PropertyChanged;
                if(c is Media)
                {
                    DateTime? t = ((Media)c).Timestamp;
                    if (t != null && (t < earliest || earliest == null))
                        earliest = t;
                    if (t != null && (t > latest || latest == null))
                        latest = t;
                }
                else
                {
                    DateTime? t = ((Event)c).StartTimestamp;
                    if (t != null && (t < earliest || earliest == null))
                        earliest = t;
                    if (t != null && (t > latest || latest == null))
                        latest = t;
                }
            }

            if(earliest != null)
                StartTimestamp = (DateTime)earliest;
            if (latest != null)
                EndTimestamp = (DateTime)latest;
        }


        /* When a child changes its timestamp or timerange, update the timerange here */
        private void Media_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is Media)
            {
                Media m = (Media)sender;
                if (e.PropertyName == nameof(m.Timestamp))
                    _mediaChanged_Reset();
            }
            else
            {
                Event ev = (Event)sender;
                if(e.PropertyName == nameof(ev.StartTimestamp))
                    _mediaChanged_Reset();
            }
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
