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

        protected override UIElement GetMainMapItem()
        {
            return Pin;
        }


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
                Children.Add(_preview);
                SetPositionOffset(_preview, new Point(-_preview.Width / 2, -_preview.Height - 50));
                SetPosition(_preview, Pin.Location);
            }
        }

        /**
         * In edit mode, dragging the pin moves the location around
         */
        protected override void MainMapItem_Drag(object sender, MouseDragEventArgs e)
        {
            if(EditMode)
                Pin.Location = new Location(Pin.Location.Latitude + e.LatitudeDifference, Pin.Location.Longitude + e.LongitudeDifference);
        }


        /**
         * When the pin is clicked, tell the MapViewModel to open the preview
         */
        protected override void MainMapItem_Click(object sender, MouseEventArgs e)
        {
            Map? map = ViewAncestor.FindAncestor<Map>(this);
            if(map != null)
            {
                MapViewModel vm = (MapViewModel)map.DataContext;
                vm.MapItemClick(DataContext);
            }

            e.Handled = true;
        }
    }
}
