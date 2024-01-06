using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PhotoGalleryApp.Views.Maps
{
    /// <summary>
    /// Abstract code-behind for a map item preview box.
    /// </summary>
    public abstract partial class MapItemPreviewView : UserControl
    {
        public MapItemPreviewView()
        {
            MouseDown += Container_MouseDown;
        }

        private void Container_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Prevent the map from taking the click. The preview is over the map, so should absorb the click
            e.Handled = true;
        }
    }
}
