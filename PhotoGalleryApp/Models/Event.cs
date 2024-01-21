using PhotoGalleryApp.Utils;
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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Represents a singular event that happened and has media associated with it. Should have a range of time
    /// associated with it in the future.
    /// </summary>
    [KnownType(typeof(ICollectable))]
    [DataContract]
    public class Event : ICollectable
    {
        /// <summary>
        /// Creates a new Event object.
        /// </summary>
        /// <param name="name">The display name of the event</param>
        public Event(string name)
        {
            _name = name;

            _collection = new MediaCollection();
            ((INotifyPropertyChanged)_collection).PropertyChanged += Collection_PropertyChanged;
        }



        #region Fields and Properties

        private MediaCollection _collection;
        /// <summary>
        /// The media associated with the event
        /// </summary>
        [DataMember]
        public MediaCollection Collection { 
            get { return _collection; }
            set { _collection = value; }
        }


        private string _name;
        /// <summary>
        /// The display name of the event
        /// </summary>
        [DataMember]
        public string Name { 
            get { return _name; } 
            set { 
                _name = value;
                OnPropertyChanged();
            }
        }

        private Media? _thumbnail = null;


        /// <summary>
        /// The thumbnail to represent the event when viewed as a small tile. If null, there is
        /// no selected thumbnail.
        /// </summary>
        [DataMember]
        public Media? Thumbnail
        {
            get { return _thumbnail; }
            set { 
                _thumbnail = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// The Timestamp of the earliest Media in this event
        /// </summary>
        public PrecisionDateTime StartTimestamp => _collection.StartTimestamp;

        /// <summary>
        /// The Timestamp of the latest Media in this event
        /// </summary>
        public PrecisionDateTime EndTimestamp => _collection.EndTimestamp;



        #endregion Fields and Properties



        /**
         * Selects the given MapItem on all of the children of this event
         */
        public void AddMapItemToAll(MapItem i)
        {
            foreach(ICollectable item in Collection)
            {
                if(item is Media)
                    ((Media)item).MapItem = i;
                else if (item is Event)
                    ((Event)item).AddMapItemToAll(i);
            }
        }

        
        /**
         * Adds the given tag to all of the children of this event
         */
        public void AddTagToAll(string tag)
        {
            foreach(ICollectable item in Collection)
            {
                if (item is Media)
                {
                    Media m = (Media)item;
                    if (!m.Tags.Contains(tag))
                        m.Tags.Add(tag);
                }
                else if(item is Event)
                    ((Event)item).AddTagToAll(tag);
            }
        }


        /**
         * Removes the given tag from all of the children of this event
         */
        public void RemoveTagFromAll(string tag)
        {
            foreach(ICollectable item in Collection)
            {
                if (item is Media)
                    ((Media)item).Tags.Remove(tag);
                else if(item is Event)
                    ((Event)item).RemoveTagFromAll(tag);
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


        /**
         * When the media collection changes its Timestamps, notify any listeners for the Timestamp properties in this class.
         */
        private void Collection_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_collection.StartTimestamp))
                OnPropertyChanged(nameof(StartTimestamp)); 

            if(e.PropertyName == nameof(_collection.EndTimestamp))
                OnPropertyChanged(nameof(EndTimestamp)); 
        }
    }
}
