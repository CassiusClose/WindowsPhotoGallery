using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public LocationPageViewModel(MapLocation item) : base(item) { }


        public MapLocation MapLocation
        {
            get { return (MapLocation)_mapItem; }
        }


        public string LocationString
        {
            get { return MapLocation.Location.ToString(); }
        }



        protected override void _mapItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base._mapItem_PropertyChanged(sender, e);

            if (e.PropertyName == nameof(MapLocation.Location))
                OnPropertyChanged(nameof(LocationString));
        }
    }
}
