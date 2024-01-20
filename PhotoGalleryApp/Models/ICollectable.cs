using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// An abstract item that can be stored in the MediaCollection class. Subclasses are Media, and Event.
    /// A MediaCollection can contain an event, which has its own MediaCollection. So you can have nested
    /// MediaCollections.
    /// </summary>
    [KnownType(typeof(Image))]
    [KnownType(typeof(Video))]
    [KnownType(typeof(Event))]
    [DataContract]
    public abstract class ICollectable : NotifyPropertyChanged
    {
        /// <summary>
        /// Returns whether the class holds an media item or a collection of items
        /// </summary>
        /// <returns>Whether the class holds an media item or a collection of items</returns>
        public abstract bool IsCollection();
    }
}
