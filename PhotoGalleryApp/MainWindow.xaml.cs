using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;

namespace PhotoGalleryApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The main content of the app is this Navigator VM that controls how pages are
        // loaded on top of each other. Just display nav, and nav will display the rest.
        private NavigatorViewModel _nav;

        private SidebarViewModel _sidebar;

        public UserSession Session;

        public MainWindow()
        {
            _nav = new NavigatorViewModel();

            Session = UserSession.LoadSession("session.xml");

            //TODO Check for missing media
            /*MediaCollection coll = gallery.MediaList;
            for(int i = coll.Count - 1; i >= 0; i--)
            {
                Media media = coll[i];

                if (!File.Exists(media.Filepath))
                {
                    Console.WriteLine("File not found, removing from gallery: " + media.Filepath);
                    coll.RemoveAt(i);
                    continue;
                }
            }*/

            _sidebar = new SidebarViewModel(_nav, Session);

            //_nav.NewPage(new GalleryViewModel("All Items", _nav, Gallery.Collection));
            _nav.NewPage(new EventsViewModel(_nav, Session.Gallery.Collection)); //_nav.NewPage(new MapViewModel(_nav))k
            DataContext = _nav;

            InitializeComponent();

            this.MainSidebar.DataContext = _sidebar;
        }

        public void SaveGallery()
        {
            Session.SaveSession();
        }


        public static UserSession GetCurrentSession()
        {
            return ((MainWindow)System.Windows.Application.Current.MainWindow).Session;
        }
    }
}
