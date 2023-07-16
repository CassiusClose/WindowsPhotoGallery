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
using System.Xml.Serialization;

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

        private Gallery gallery;

        public MainWindow()
        {
            _nav = new NavigatorViewModel();

            gallery = Gallery.LoadGallery("gallery.xml");
            //Gallery gallery = new Gallery("Gallery", new MediaCollection());
            if(gallery == null)
            {
                Trace.WriteLine("ERROR IN SERIALIZATION");
                System.Windows.Application.Current.Shutdown();
                return;
            }

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

            _sidebar = new SidebarViewModel(_nav, gallery.Collection);

            _nav.NewPage(new GalleryViewModel(_nav, gallery.Collection));
            //_nav.NewPage(new MapViewModel(_nav));
            DataContext = _nav;

            InitializeComponent();

            this.MainSidebar.DataContext = _sidebar;
        }

        public void SaveGallery()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Gallery));
            TextWriter writer = new StreamWriter("gallery.xml");
            serializer.Serialize(writer, gallery);
            writer.Close();

        }
    }
}
