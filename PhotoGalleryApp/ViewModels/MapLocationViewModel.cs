using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    public class MapLocationViewModel
    {
        public MapLocationViewModel(MapLocation location)
        {
            _location = location;
        }

        private MapLocation _location;

        public Location Coordinates { get { return _location.Coordinates; } }

        public string Name { get { return _location.Name; } }
    }
}
