using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the MapLocation model. 
    /// </summary>
    public class MapLocationViewModel : MapItemViewModel
    {
        public MapLocationViewModel(NavigatorViewModel nav, MapLocation location)
        {
            _openPageCommand = new RelayCommand(OpenPage);

            PreviewType = typeof(PhotoGalleryApp.Views.Maps.MapLocationPreview);

            _nav = nav;
            _location = location;
        }

        public override void Cleanup() { }


        private NavigatorViewModel _nav;
        private MapLocation _location;
        

        public Location Location
        {
            get { return _location.Location; }
            set
            {
                _location.Location = value;
                OnPropertyChanged();
            }
        }



        public override MapItem GetModel()
        {
            return _location;
        }


        private RelayCommand _openPageCommand;
        public ICommand OpenPageCommand => _openPageCommand;
        
        /**
         * Opens the page with the full info about the location
         */
        public void OpenPage()
        {
            _nav.NewPage(new LocationPageViewModel(_nav, _location));
        }
    }
}
