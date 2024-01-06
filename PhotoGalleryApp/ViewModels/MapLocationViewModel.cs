using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the MapLocation model. 
    /// </summary>
    public class MapLocationViewModel : MapItemViewModel
    {
        public MapLocationViewModel(MapLocation location)
        {
            _location = location;
        }
        public override void Cleanup() { }


        private MapLocation _location;
        

        public Location Coordinates
        {
            get { return _location.Coordinates; }
            set
            {
                _location.Coordinates = value;
                OnPropertyChanged();
            }
        }


        public string Name { get { return _location.Name; } }


        public override MapItem GetModel()
        {
            return _location;
        }
    }
}
