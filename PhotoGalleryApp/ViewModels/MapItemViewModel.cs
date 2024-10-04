using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// Abstract class representing items that can be displayed on a map. Currently, these items are 
    /// Locations and Paths. These items should be able to be edited, and should have some sort of
    /// preview box that pops up when clicked on. 
    /// </summary>
    public abstract class MapItemViewModel : ViewModelBase
    {
        public MapItemViewModel(MapViewModel map)
        {
            _map = map;
            _map.PropertyChanged += Map_PropertyChanged;
            _mapZoomLevel = map.ZoomLevel;
        }

        public override void Cleanup()
        {
            _map.PropertyChanged -= Map_PropertyChanged;
        }


        protected MapViewModel _map;

        /** Call function when map level changes */
        protected void Map_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not MapViewModel)
                throw new ArgumentException();

            if(e.PropertyName == nameof(MapViewModel.ZoomLevel))
            {
                _mapZoomLevel = ((MapViewModel)sender).ZoomLevel;
                ZoomLevelChanged();
            }
        }


        protected double _mapZoomLevel;

        protected virtual void ZoomLevelChanged() { }


        /// <summary>
        /// Returns the model that the ViewModel is associated with
        /// </summary>
        /// <returns></returns>
        public abstract MapItem GetModel();


        /// <summary>
        /// Returns the Type of the UserControl that is the Preview for this
        /// item. Subclasses should set this to not-null if previews are
        /// enabled.
        /// </summary>
        public Type? PreviewType { get; protected set; }


        protected bool _editMode;
        /// <summary>
        /// Whether the MapItem is able to be edited
        /// </summary>
        public virtual bool EditMode
        {
            get { return _editMode; }
            set
            {
                _editMode = value;
                EditModeChanged();
                // Keep this after EditModeChanged() so that MapPath can switch
                // paths around before the edit mode pins are created
                OnPropertyChanged();
            }
        }

        protected virtual void EditModeChanged() {}


        protected bool _previewOpen;
        /// <summary>
        /// Whether the MapItem's preview box should be open
        /// </summary>
        public virtual bool PreviewOpen
        {
            get { return _previewOpen; }
            set
            {
                _previewOpen = value; 
                OnPropertyChanged();
            }
        }


        public string Name { 
            get { return GetModel().Name; } 
            set
            {
                GetModel().Name = value;
                OnPropertyChanged();
            }
        }


        private bool _fadedColor = false;
        /// <summary>
        /// Whether the item's color should be faded or not. This will be set
        /// by MapViewModel when items are put into/removed from edit mode.
        /// </summary>
        public bool FadedColor
        {
            get { return _fadedColor; }
            set {
                _fadedColor = value; 
                OnPropertyChanged(); 
            }
        }




        /**
         * Generator to create MapPathViewModels and MapLocationViewModels
         */
        public static MapItemViewModel CreateMapItemViewModel(MapItem item, MapViewModel map)
        {
            if (item is MapPath)
                return new MapPathViewModel(MainWindow.GetNavigator(), (MapPath)item, map);

            else
                return new MapLocationViewModel(MainWindow.GetNavigator(), (MapLocation)item, map);
        }

    }
}
