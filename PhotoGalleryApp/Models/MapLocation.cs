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
        public MapLocation(string name, Location coordinates)
        {
            _name = name;
            _coordinates = coordinates;
        }


        private string _name;
        public string Name { 
            get { return _name; } 
            set { _name = value; }
        }


        private Location _coordinates;
        public Location Coordinates { 
            get { return _coordinates; }
            set { _coordinates = value; }
        }
    }
}
