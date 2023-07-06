using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// An abstract item that can be stored in the MediaCollection class. Subclasses are Media, and Event.
    /// A MediaCollection can contain an event, which has its own MediaCollection. So you can have nested
    /// MediaCollections.
    /// </summary>
    public abstract class ICollectable
    {
        /// <summary>
        /// Returns whether the class holds an media item or a collection of items
        /// </summary>
        /// <returns>Whether the class holds an media item or a collection of items</returns>
        public abstract bool IsCollection();
    }
}
