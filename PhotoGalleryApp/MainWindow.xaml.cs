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

            PhotoGallery gallery = new PhotoGallery("Gallery 1");
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/Lightship.jpg", new ObservableCollection<string> { "boat" }));
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/LuperonSunset.jpg", new ObservableCollection<string> { "boat", "harbor" }));
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/MtPeleeHike.jpg", new ObservableCollection<string> { "land" }));
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/Saba1.jpg", new ObservableCollection<string> { "land", "harbor" }));
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/Saba2.jpg", new ObservableCollection<string> { "land", "harbor" }));
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/Wicked.jpg", new ObservableCollection<string> { "boat" }));
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/Cover.png", new ObservableCollection<string> { "music" }));
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/Tire.jpg", new ObservableCollection<string> { "land" }));
            gallery.Add(new Photo("B:/Projects/WindowsPhotoGallery/images/Lightship2.jpg", new ObservableCollection<string> { "boat" }));

            nav.NewPage(new GalleryViewModel(nav, gallery));
            DataContext = nav;

            InitializeComponent();
        }
    }
}
