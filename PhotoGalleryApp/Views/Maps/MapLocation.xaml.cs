using Microsoft.Maps.MapControl.WPF;
using Microsoft.Xaml.Behaviors;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.ViewModels;
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

        protected override UIElement GetMainMapItem()
        {
            return Pin;
        }


        protected override void OpenPreview()
        {
            if(_preview == null)
            {
                _preview = new MapLocationPreview();
                _preview.DataContext = DataContext;
                Children.Add(_preview);
                SetPositionOffset(_preview, new Point(-_preview.Width / 2, -_preview.Height - 50));
                SetPosition(_preview, Pin.Location);
            }
        }



        protected override void MainMapItem_Click(object sender, MouseEventArgs e)
        {
            // When the pin is clicked, tell the MapViewModel to open the preview
            Map? map = ViewAncestor.FindAncestor<Map>(this);
            if(map != null)
            {
                MapViewModel vm = (MapViewModel)map.DataContext;
                vm.TogglePreview(DataContext);
            }

            e.Handled = true;
        }



        private void MapLayer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // If currently dragging the pin, update the pin's location
                if(_pinClick && EditMode)
                    Pin.Location = _parentMap.MapView.ViewportPointToLocation(Point.Add(e.GetPosition(this), _mousePos));

                e.Handled = true;
            }
        }


        #region Pin Drag

        Vector _mousePos;
        private bool _pinClick = false;


        private void Pin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Disable the background from capturing mouse clicks. Otherwise, the background will prevent mouse clicks
            // from reaching the polylines
            Background = null;
            _pinClick = false;
        }

        private void Pin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Pushpin)
                return;

            _mousePos = Point.Subtract(_parentMap.MapView.LocationToViewportPoint(Pin.Location), e.GetPosition(this));
            _pinClick = true;

            // Set background to transparent so it captures mouse clicks. This enables the layer's MouseMove event, which is used to
            // move a pin around when it's being dragged. Wouldn't need to use this if it used the Pushpin's MouseMove instead of
            // the MapLayer's MouseMove, but when using the pushpin, its location updates too slowly, so the mouse can move outside of the
            // pushpin and it will stop moving along with the mouse.
            Background = new SolidColorBrush(Colors.Transparent);
        }
        #endregion Pin Drag


    }
}
