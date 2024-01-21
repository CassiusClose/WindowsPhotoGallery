using Microsoft.Maps.MapControl.WPF;
using Microsoft.Xaml.Behaviors;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.ViewModels;
using PhotoGalleryApp.Views.Behavior;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoGalleryApp.Views.Maps
{
    /// <summary>
    /// Interaction logic for MapLocation.xaml
    /// </summary>
    public partial class MapLocation : MapItemView
    {
        public MapLocation()
        {
            InitializeComponent();
        }


        /**
         * Removes all components from any MapLayers
         */
        public override void RemoveAll()
        {
            if (_preview != null)
                ClosePreview();
            PinLayer.Children.Remove(Pin);
        }


        #region Location Pin

        private Pushpin Pin;

        protected override void Init_MainMapItem()
        {
            Pin = new Pushpin();
            PinLayer.Children.Add(Pin);

            SetBinding(LocationProperty, new Binding("Location"));
        }

        protected override UIElement GetMainMapItem()
        {
            return Pin;
        }

        #endregion Location Pin


        #region Locations Property

        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location", typeof(Location), typeof(MapLocation),
            new PropertyMetadata(LocationPropertyChanged));
 

        /// <summary>
        /// An ordered collection of Locations that make up the path
        /// </summary>
        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set {
                SetValue(LocationProperty, value);
                Pin.Location = value;
            }
        }

        private static void LocationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapLocation control = (MapLocation)sender;
            control.Pin.Location = control.Location;
        }

        #endregion Locations Property


        protected override void OpenPreview()
        {
            if(_preview == null)
            {
                MapItemViewModel vm = (MapItemViewModel)DataContext;

                // Get the specified type for the Preview box, and instanciate it
                Type? PreviewType = vm.PreviewType;
                if (PreviewType == null)
                    return;

                _preview = (UserControl?)Activator.CreateInstance(PreviewType);
                if (_preview == null)
                    return;

                _preview.DataContext = DataContext;
                PreviewLayer.Children.Add(_preview);
                MapLayer.SetPositionOffset(_preview, new Point(-_preview.Width / 2, -_preview.Height - 50));
                MapLayer.SetPosition(_preview, Pin.Location);
            }
        }

        protected override void EditModeChanged() 
        {
            // Show the Location pin above all other pins
            if (EditMode)
            {
                PinLayer.Children.Remove(Pin);
                PinLayer.Children.Add(Pin);
            }
        }


        /**
         * In edit mode, dragging the pin moves the location around
         */
        protected override void MainMapItem_Drag(object sender, MouseDragEventArgs e)
        {
            if(EditMode)
                Location = new Location(Pin.Location.Latitude + e.LatitudeDifference, Pin.Location.Longitude + e.LongitudeDifference);
        }


        /**
         * When the pin is clicked, tell the MapViewModel to open the preview
         */
        protected override void MainMapItem_Click(object sender, MouseEventArgs e)
        {
            MapViewModel vm = (MapViewModel)_map.DataContext;
            vm.MapItemClick(DataContext);

            e.Handled = true;
        }
    }
}
