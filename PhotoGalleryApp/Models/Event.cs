using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            _collection = new MediaCollection(name);
        }

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
