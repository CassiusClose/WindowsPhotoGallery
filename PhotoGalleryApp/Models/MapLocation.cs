using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;

namespace PhotoGalleryApp.Models
{
    public class MapLocation
    {
        public MapLocation(string name, Location coordinates)
        {
            _name = name;
            _coordinates = coordinates;
        }


        private string _name;
        public string Name { get { return _name; } }


        private Location _coordinates;
        public Location Coordinates { get { return _coordinates; } }
    }
}
