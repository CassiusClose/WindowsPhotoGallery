using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the page displaying a MapLocation.
    /// </summary>
    public class LocationPageViewModel : ViewModelBase
    {
        public LocationPageViewModel(NavigatorViewModel nav, MapLocation loc)
        {
            _openCollectionCommand = new RelayCommand(OpenCollection);

            _nav = nav;
            _mapLocation = loc;

            MediaCollection mainColl = MainWindow.GetCurrentSession().Gallery.Collection;

            // Only show the media associated with this location
            FilterSet filters = new FilterSet(mainColl);
            MapItemFilter filt = (MapItemFilter)filters.GetCriteriaByType(typeof(MapItemFilter));
            filt.MapItemCriteria = loc;

            MediaCollection = new MediaCollectionViewModel(nav, mainColl, true, null, filters);
        }

        public override void Cleanup() { }


        private NavigatorViewModel _nav;
        private MapLocation _mapLocation;


        /**
         * Preview of media associated with this path
         */
        public MediaCollectionViewModel MediaCollection { get; private set; }


        public string Name
        {
            get { return _mapLocation.Name; }
        }

        public string LocationString
        {
            get { return _mapLocation.Location.ToString(); }
        }



        private RelayCommand _openCollectionCommand;
        public ICommand OpenCollectionCommand => _openCollectionCommand;

        /**
         * Open the full gallery of associated Media
         */
        public void OpenCollection()
        {
            _nav.NewPage(new GalleryViewModel(_mapLocation.Name, _nav, MediaCollection.MediaCollectionModel, null));
        }
    }
}
