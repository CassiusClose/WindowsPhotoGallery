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
using PhotoGalleryApp.ViewModels;
using PhotoGalleryApp.Models;
using System.Collections.ObjectModel;

namespace PhotoGalleryApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The main content of the app is this Navigator VM that controls how pages are
        // loaded on top of each other. Just display nav, and nav will display the rest.
        private NavigatorViewModel nav;

        public MainWindow()
        {
            nav = new NavigatorViewModel();

            PhotoGallery gallery = PhotoGallery.LoadGallery("gallery.xml");

            nav.NewPage(new GalleryViewModel(nav, gallery));
            DataContext = nav;

            InitializeComponent();
        }

        /**
         * When the user clicks on the "window", i.e. something that isn't focusable, like a
         * panel, grid, or label, then this will be called. Here, we set focus & keyboard focus
         * to the window's grid so that focus is lost elsewhere. This lets the user click off
         * of a textbox, for example, and have it lose focus.
         */
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowGrid.Focus();
            Keyboard.Focus(WindowGrid);
        }
         
        /**
         * KeyDown events will be send to the navigator to be passed on to the current page.
         */
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            nav.BroadcastKeyEvent(e.Key);
        }
    }
}
