using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    class MapViewModel : ViewModelBase
    {
        public MapViewModel(NavigatorViewModel nav)
        {
            _nav = nav;

            _addLocationCommand = new RelayCommand(AddLocation);

            _locations = new ObservableCollection<MapLocationViewModel>
            {
                new MapLocationViewModel(new MapLocation("Miami", new Location(25.7617, -80.1918))),
                new MapLocationViewModel(new MapLocation("Nassau", new Location(25.0443, -77.3504)))
            };
            _locations.CollectionChanged += _locations_CollectionChanged;

            Locations = CollectionViewSource.GetDefaultView(_locations);
            OnPropertyChanged("Locations");
        }

        private void _locations_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Trace.WriteLine("Collection Changed");
        }

        private NavigatorViewModel _nav;

        private ObservableCollection<MapLocationViewModel> _locations;
        public ICollectionView Locations { get; }



        private RelayCommand _addLocationCommand;
        public ICommand AddLocationCommand { get { return _addLocationCommand; } }

        private void AddLocation(object parameter)
        {
            Trace.WriteLine("Hey");
            _locations.Add(new MapLocationViewModel(new MapLocation("Test", new Location(20, 20))));
        }
    }
}
