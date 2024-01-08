using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    public class SidebarViewModel : ViewModelBase
    {
        public SidebarViewModel(NavigatorViewModel nav, MediaCollection coll)
        {
            _nav = nav;
            _collection = coll;

            _sidebarGalleryCommand = new RelayCommand(OpenGallery);
            _sidebarMapCommand = new RelayCommand(OpenMap);
            _sidebarEventsCommand = new RelayCommand(OpenEvents);
        }

        public override void Cleanup() { }


        private NavigatorViewModel _nav;

        private MediaCollection _collection;



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
                _nav.NewPage(new GalleryViewModel("All Items", _nav, _collection));
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
                PhotoGalleryApp.Models.Map map = new PhotoGalleryApp.Models.Map();
                map.Items.Add(new MapLocation("Miami", new Location(25.7617, -80.1918)));
                map.Items.Add(new MapLocation("Nassau", new Location(25.0443, -77.3504)));
                /*MapPath p = new MapPath("Path 1");
                p.Points.Add(new Location(30, 30));
                p.Points.Add(new Location(31, 30));
                p.Points.Add(new Location(34, 37));
                map.Items.Add(p);*/


                MapPath p = new MapPath("Rum to Mayaguana");
                //using (StreamReader reader = new StreamReader("Main.2014-08-15.Rum-Mayaguana.txt"))
                using (StreamReader reader = new StreamReader("Main.2014-11-23.Boqueron-CaboRojo.txt"))
                {
                    string? currentLine;
                    while ((currentLine = reader.ReadLine()) != null)
                    {
                        Location l = new Location();
                        string[] coords = currentLine.Split('\t', 5);
                        l.Latitude = double.Parse(coords[2]);
                        l.Longitude = double.Parse(coords[3]);
                        p.Points.Add(l);
                        //map.Items.Add(new MapLocation("Test", l));
                    }
                }

                map.Items.Add(p);

                _nav.NewPage(new MapViewModel(_nav, map));
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
                _nav.NewPage(new EventsViewModel(_nav, _collection));
            }
        }

    }
}
