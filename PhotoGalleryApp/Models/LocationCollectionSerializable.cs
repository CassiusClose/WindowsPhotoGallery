using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Utility class to allow control over which properties of Location are
    /// serialized. When using, create a property in the same class as the real
    /// LocationCollection. When the LocationCollectionSerializable property is
    /// gotten (during serialization), create a new instance of the class from
    /// the real LocationCollection. When the LocationCollectionSerializable
    /// property is set (during deserialization), then the real
    /// LocationCollection should be set by GetLocationCollection() here.
    /// </summary>
    class LocationCollectionSerializable : List<LocationSerializable>
    {
        // Needed for deserialization
        private LocationCollectionSerializable() { }


        // Save the properties that should be serialized from the given LocationCollection
        public LocationCollectionSerializable(LocationCollection coll)
        {
            foreach(Location l in coll)
                Add(new LocationSerializable(l));
        }


        // Return a LocationCollection from the properties saved here
        public LocationCollection GetLocationCollection()
        {
            LocationCollection coll = new LocationCollection();
            foreach(LocationSerializable l in this)
                coll.Add(l.GetLocation());

            return coll;
        }
    }
}
