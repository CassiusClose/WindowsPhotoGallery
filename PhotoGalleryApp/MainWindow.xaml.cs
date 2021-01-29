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
        public MainWindow()
        {
            NavigatorViewModel nav = new NavigatorViewModel();

            PhotoGallery gallery = PhotoGallery.LoadGallery("gallery.xml");

            nav.NewPage(new GalleryViewModel(nav, gallery));
            DataContext = nav;

            InitializeComponent();
        }
    }
}
