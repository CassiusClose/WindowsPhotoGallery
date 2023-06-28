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
        public SidebarViewModel(NavigatorViewModel nav, Gallery gallery)
        {
            _nav = nav;
            _gallery = gallery;

            _sidebarGalleryCommand = new RelayCommand(OpenGallery);
            _sidebarMapCommand = new RelayCommand(OpenMap);
            _sidebarEventsCommand = new RelayCommand(OpenEvents);
        }

        private NavigatorViewModel _nav;
        private Gallery _gallery;



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
                _nav.NewPage(new GalleryViewModel(_nav, _gallery));
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


        private RelayCommand _sidebarEventsCommand;
        public ICommand SidebarEventsCommand => _sidebarEventsCommand;

        /// <summary>
        /// Opens the events page
        /// </summary>
        /// <param name="parameter"></param>
        public void OpenEvents(object parameter)
        {
            if(_nav.CurrentPage.GetType() != typeof(EventsViewModel))
            {
                _nav.NewPage(new EventsViewModel(_nav, _gallery));
            }
        }

    }
}
