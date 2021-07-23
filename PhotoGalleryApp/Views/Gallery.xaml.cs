using System;
using System.Collections.Generic;
using System.Linq;
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

namespace PhotoGalleryApp.Views
{
    /// <summary>
    /// Interaction logic for Gallery.xaml
    /// </summary>
    public partial class Gallery : UserControl
    {
        public Gallery()
        {
            InitializeComponent();
        }

        /**
         * If the user scrolls the mouse wheel when the mouse is anywhere within the ScrollViewer,
         * scroll the ScrollViewer.
         */
        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int offset = e.Delta;
            ImagesScrollViewer.ScrollToVerticalOffset(ImagesScrollViewer.VerticalOffset - offset);
            e.Handled = true;
        }
    }
}
