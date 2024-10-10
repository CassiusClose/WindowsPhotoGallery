using PhotoGalleryApp.Models;
using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A view which takes a MapLocation and creates a list of all of the
    /// parents (and self) of that MapLocation, in MapLocationNameViewModel
    /// form.
    /// </summary>
    public class MapLocationParentView
    {
        public MapLocationParentView(MapLocation location)
        {
            View = new ObservableCollection<MapLocationNameViewModel>();

            _location = location;
            _location.PropertyChanged += _location_PropertyChanged;
            Refresh();
        }

        public void Cleanup()
        {
            foreach(MapLocationNameViewModel vm in View)
            {
                vm.Location.PropertyChanged -= _location_PropertyChanged;
                vm.Cleanup();
            }
        }


        private MapLocation _location;

        private void _location_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Refresh();
        }

        public ObservableCollection<MapLocationNameViewModel> View
        {
            get; internal set;
        }


        private void Refresh()
        {
            foreach(MapLocationNameViewModel vm in View)
            {
                vm.Location.PropertyChanged -= _location_PropertyChanged;
                vm.Cleanup();
            }
            View.Clear();


            ObservableCollection<MapLocationNameViewModel> newList = new ObservableCollection<MapLocationNameViewModel>();

            MapLocation? loc = _location;
            while(loc != null)
            {
                _location.PropertyChanged += _location_PropertyChanged;
                View.Insert(0, new MapLocationNameViewModel(loc));
                loc = loc.Parent;
            }
        }
    }
}
