using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Utils;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Information about a single location on the map
    /// </summary>
    [DataContract]
    public class MapLocation : MapItem
    {
        public MapLocation(string name, Location coordinates) : base(name)
        {
            _location = coordinates;
        }


        private Location _location;
        public Location Location { 
            get { return _location; }
            set { _location = value; }
        }


        // Store specific properties from Location so only those are serialized
        [DataMember(Name = nameof(Location))]
        private LocationSerializable _locationSerializable
        {
            get { return new LocationSerializable(Location); }
            set { this.Location = value.GetLocation(); }
        }
    }
}
