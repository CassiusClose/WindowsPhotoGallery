using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for EventPage.xaml
    /// </summary>
    public partial class EventPage : UserControl
    {
        public EventPage()
        {
            InitializeComponent();
        }

        // When the user clicks anywhere on the control, keep focus on the top-level
        // so key events are captured.
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            e.Handled = true;
        }

        // Seize focus when the control is loaded (when the navigator opens this page).
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.Focus();
            }
        }
    }
}
