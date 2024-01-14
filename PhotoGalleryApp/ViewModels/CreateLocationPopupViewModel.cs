using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /**
     * ViewModel for the Popup Window to create a new MapLocation.
     */
    public class CreateLocationPopupViewModel : PopupViewModel
    {
        public CreateLocationPopupViewModel() {
            _latitudeText = "0.0000";
            _longitudeText = "0.0000";
        }

        public CreateLocationPopupViewModel(Location l)
        {
            _latitudeText = l.Latitude.ToString();
            _longitudeText = l.Longitude.ToString();
        }

        public override void Cleanup() {}

        private string _name = "";
        /// <summary>
        /// The name of the location
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _latitudeText;
        /// <summary>
        /// The text in the latitude entry box. Not necessarily a valid number
        /// </summary>
        public string LatitudeText
        {
            get { return _latitudeText; }
            set
            {
                _latitudeText = value;
                OnPropertyChanged();
            }
        }


        private string _longitudeText;
        /// <summary>
        /// The text in the longitude entry box. Not necessarily a valid number
        /// </summary>
        public string LongitudeText 
        {
            get { return _longitudeText; }
            set
            {
                _longitudeText = value;
                OnPropertyChanged();
            }
        }


        public override PopupReturnArgs GetPopupResults()
        {
            // Convert lat/long text to Location
            double lat;
            bool latGood = double.TryParse(_latitudeText, out lat);

            double lon;
            bool lonGood = double.TryParse(_longitudeText, out lon);

            if(latGood && lonGood)
                return new CreateLocationPopupReturnArgs(Name, new Location(lat, lon));

            return new CreateLocationPopupReturnArgs();
        }


        protected override bool ValidateData()
        {
            if(string.IsNullOrWhiteSpace(Name))
            {
                ValidationErrorText = "Name field must not be empty";
                return false;
            }

            if(string.IsNullOrWhiteSpace(_latitudeText) || string.IsNullOrWhiteSpace(_longitudeText))
            {
                ValidationErrorText = "Latitude & Longtitude fields must not be empty";
                return false;
            }

            // Fail if lat/long text aren't valid doubles
            double lat;
            bool latGood = double.TryParse(_latitudeText, out lat);

            if(!latGood)
            {
                ValidationErrorText = "Latitude must be a valid number";
                return false;
            }

            double lon;
            bool lonGood = double.TryParse(_longitudeText, out lon);

            if (!lonGood)
            {
                ValidationErrorText = "Longitude must be a valid number";
                return false;
            }

            ValidationErrorText = "";
            return true;
        }
    }


    public class CreateLocationPopupReturnArgs : PopupReturnArgs
    {
        public CreateLocationPopupReturnArgs() {}

        public CreateLocationPopupReturnArgs(string name, Location l) { 
            Name = name;
            Location = l;
        }

        public string? Name;
        public Location? Location;
    }
}
