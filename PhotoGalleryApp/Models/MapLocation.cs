using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Utils;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Information about a single location on the map
    /// </summary>
    public class MapLocation : MapItem
    {
        // Parameterless constructor for XML deserialization
        private MapLocation() : base("") { }

        public MapLocation(string name, Location coordinates) : base(name)
        {
            _coordinates = coordinates;
        }


        private Location _coordinates;
        public Location Coordinates { 
            get { return _coordinates; }
            set { _coordinates = value; }
        }
    }
}
