using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the page displaying a MapLocation.
    /// </summary>
    public class LocationPageViewModel : MapItemPageViewModel
    {
        public LocationPageViewModel(MapLocation item) : base(item) 
        {
            _openLocationCommand = new RelayCommand(OpenLocation);
            _setParentCommand = new RelayCommand(SetParent);

            _childrenView = new LocationNameView(item.Children);
            Children = _childrenView.View;

            _parentView = new MapLocationParentView(item);
            Parents = _parentView.View;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _childrenView.Cleanup();
            _parentView.Cleanup();
        }


        public MapLocation MapLocation
        {
            get { return (MapLocation)_mapItem; }
        }

        public string LocationString
        {
            get { return MapLocation.Location.ToString(); }
        }

        public string? LocationTreeString 
        {
            get { return MapLocation.DisplayString(); }
        }



        private LocationNameView _childrenView;
        public ObservableCollection<MapLocationNameViewModel> Children { get; internal set; }


        private MapLocationParentView _parentView;
        public ObservableCollection<MapLocationNameViewModel> Parents { get; internal set; }




        private RelayCommand _openLocationCommand;
        public ICommand OpenLocationCommand => _openLocationCommand;

        public void OpenLocation(object param)
        {
            if(param is MapLocationNameViewModel)
            {
                MapLocation map = ((MapLocationNameViewModel)param).Location;
                if (map != MapLocation)
                    MainWindow.GetNavigator().NewPage(new LocationPageViewModel(map));
            }
        }

        private RelayCommand _setParentCommand;
        public ICommand SetParentCommand => _setParentCommand;

        public void SetParent()
        {
            PickMapLocationPopupViewModel popup = new PickMapLocationPopupViewModel(MainWindow.GetNavigator());
            PickMapItemPopupReturnArgs args = (PickMapItemPopupReturnArgs)MainWindow.GetNavigator().OpenPopup(popup);

            if (args.PopupAccepted && args.ChosenMapItem is MapLocation)
            {
                MapLocation loc = (MapLocation)args.ChosenMapItem;
                if(loc != MapLocation.Parent)
                    MapLocation.SetParent(loc);
            }
        }


        protected override void _mapItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base._mapItem_PropertyChanged(sender, e);

            if (e.PropertyName == nameof(MapLocation.Location))
                OnPropertyChanged(nameof(LocationString));

            if(e.PropertyName == nameof(MapLocation.Parent))
                OnPropertyChanged(nameof(LocationTreeString));
        }
    }
}
