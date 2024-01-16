using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the page displaying a MapLocation.
    /// </summary>
    public class LocationPageViewModel : ViewModelBase
    {
        public LocationPageViewModel(NavigatorViewModel nav, MapLocation loc)
        {
            _nav = nav;
            _mapLocation = loc;
        }

        public override void Cleanup() { }


        private NavigatorViewModel _nav;
        private MapLocation _mapLocation;


        public string Name
        {
            get { return _mapLocation.Name; }
        }

        public string LocationString
        {
            get { return _mapLocation.Coordinates.ToString(); }
        }
    }
}
