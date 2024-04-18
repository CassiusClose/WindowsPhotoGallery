using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public MapLocationViewModel(NavigatorViewModel nav, MapLocation location, MapViewModel map) : base(map)
        {
            _openPageCommand = new RelayCommand(OpenPage);

            PreviewType = typeof(PhotoGalleryApp.Views.Maps.MapLocationPreview);

            _nav = nav;

            _location = location;
            _location.PropertyChanged += _location_PropertyChanged;
        }

        public override void Cleanup() 
        {
            _location.PropertyChanged -= _location_PropertyChanged;
        }


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


        /// <summary>
        /// Whether or not this item is part of the family tree of the item
        /// that is currently being edited. This will be set by MapViewModel.
        /// </summary>
        public bool PartOfEditTree
        {
            get; set;
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
            _nav.NewPage(new LocationPageViewModel(_location));
        }

        private void _location_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(MapLocation.Location))
                OnPropertyChanged(nameof(Location));
        }
    }
}
