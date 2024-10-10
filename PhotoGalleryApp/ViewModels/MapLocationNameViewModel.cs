using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// Exposes only the name of a MapLocation
    /// </summary>
    public class MapLocationNameViewModel : ViewModelBase
    {
        public MapLocationNameViewModel(MapLocation loc)
        {
            Location = loc;
            Location.PropertyChanged += _location_PropertyChanged;
        }

        public override void Cleanup() 
        { 
            Location.PropertyChanged -= _location_PropertyChanged;
        }


        public MapLocation Location { get; internal set; }

        private void _location_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapLocation.Name))
                OnPropertyChanged(nameof(Name));
        }


        public string Name {
            get { return Location.Name; }
        }
    }
}