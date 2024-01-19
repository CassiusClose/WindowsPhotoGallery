using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the popup that prompts the user to choose one MapItem.
    /// The Map is displayed, and when the user clicks on a MapItem, a Label
    /// outside the Map shows that the MapItem has been chosen.
    /// </summary>
    class PickMapItemPopupViewModel : PopupViewModel
    {
        public PickMapItemPopupViewModel(NavigatorViewModel nav)
        {
            _nav = nav;

            Map = new MapViewModel(_nav, MainWindow.GetCurrentSession().Map, true, true, false);
            Map.MapItemClickEvent += MapItemClick;
        }

        public override void Cleanup() 
        {
            Map.MapItemClickEvent -= MapItemClick;
            Map.Cleanup();
        }


        private NavigatorViewModel _nav;


        public MapViewModel Map
        {
            get; private set;
        }


        private MapItemViewModel? _chosenItem = null;
        /// <summary>
        /// The MapItem that the user has chosen. If null, the user has not
        /// picked any MapItem yet.
        /// </summary>
        public MapItemViewModel? ChosenItem
        {
            get { return _chosenItem; }
            set
            {
                _chosenItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChosenItemName));
            }
        }

        /// <summary>
        /// The name of the chosen MapItem, or null if none have been chosen.
        /// </summary>
        public string? ChosenItemName
        {
            get 
            {
                if (_chosenItem == null)
                    return null;
                return _chosenItem.Name; 
            }
        }


        /**
         * When the user clicks on an item, choose it
         */
        private void MapItemClick(MapItemViewModel vm)
        {
            ChosenItem = vm;
        }



        public override PopupReturnArgs GetPopupResults()
        {
            if(ChosenItem != null)
                return new PickMapItemPopupReturnArgs(ChosenItem.GetModel());

            return new PickMapItemPopupReturnArgs();
        }

        protected override bool ValidateData()
        {
            if (ChosenItem == null)
            {
                ValidationErrorText = "You must choose a map item";
                return false;
            } 
            return true;
        }
    }

    public class PickMapItemPopupReturnArgs : PopupReturnArgs
    {
        public PickMapItemPopupReturnArgs() { }
        public PickMapItemPopupReturnArgs(MapItem i) { ChosenMapItem = i; }

        public MapItem? ChosenMapItem = null;
    }
}
