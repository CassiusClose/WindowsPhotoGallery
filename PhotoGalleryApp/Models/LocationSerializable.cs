using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Utility class to allow control over which properties of Location are
    /// serialized. When using, create a property in the same class as the real
    /// LocationCollection. When the LocationSerializable property is gotten
    /// (during serialization), create a new instance of the class from the
    /// real Location. When the LocationSerializable property is set (during
    /// deserialization), then the real Location should be set by GetLocation()
    /// here.
    /// </summary>
    [DataContract(Name = nameof(Location))]
    class LocationSerializable
    {
        // Save the properties that should be serialized from the given Location
        public LocationSerializable(Location l)
        {
            Latitude = l.Latitude;
            Longitude = l.Longitude;
        }

        [DataMember]
        private double Latitude;
        
        [DataMember]
        private double Longitude;


        // Return a Location from the properties saved here
        public Location GetLocation()
        {
            Location l = new Location();
            l.Latitude = Latitude;
            l.Longitude = Longitude;
            return l;
        }
    }
}
