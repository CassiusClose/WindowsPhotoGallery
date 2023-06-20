using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    public class SidebarViewModel : ViewModelBase
    {
        public SidebarViewModel(NavigatorViewModel nav, MediaCollection mediaCollection)
        {
            _nav = nav;
            _mediaCollection = mediaCollection;

            _sidebarGalleryCommand = new RelayCommand(OpenGallery);
            _sidebarMapCommand = new RelayCommand(OpenMap);
        }

        private NavigatorViewModel _nav;
        private MediaCollection _mediaCollection;



        private RelayCommand _sidebarGalleryCommand;
        /// <summary>
        /// A command to open the gallery
        /// </summary>
        public ICommand SidebarGalleryCommand => _sidebarGalleryCommand;

        /// <summary>
        /// Opens the gallery page
        /// </summary>
        public void OpenGallery(object parameter)
        {
            //TODO Figure out how to switch between pages
            if (_nav.CurrentPage.GetType() != typeof(GalleryViewModel))
            {
                _nav.NewPage(new GalleryViewModel(_nav, _mediaCollection));
            }
        }
        
        private RelayCommand _sidebarMapCommand;
        /// <summary>
        /// A command to open the gallery
        /// </summary>
        public ICommand SidebarMapCommand => _sidebarMapCommand;

        /// <summary>
        /// Opens the map page.
        /// </summary>
        public void OpenMap(object parameter)
        {
            if (_nav.CurrentPage.GetType() != typeof(MapViewModel))
            {
                _nav.NewPage(new MapViewModel(_nav));
            }
        }



    }
}
