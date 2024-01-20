using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the page displaying a MapPath.
    /// </summary>
    public class PathPageViewModel : ViewModelBase
    {
        public PathPageViewModel(NavigatorViewModel nav, MapPath path)
        {
            _openCollectionCommand = new RelayCommand(OpenCollection);

            _nav = nav;
            _mapPath = path;

            MediaCollection mainColl = MainWindow.GetCurrentSession().Gallery.Collection;

            // Only show the media associated with this location
            FilterSet filters = new FilterSet(mainColl);
            MapItemFilter filt = (MapItemFilter)filters.GetCriteriaByType(typeof(MapItemFilter));
            filt.MapItemCriteria = path;

            MediaCollection = new MediaCollectionViewModel(nav, mainColl, true, null, filters);
        }

        public override void Cleanup() { }


        private NavigatorViewModel _nav;
        private MapPath _mapPath;


        /**
         * Preview of media associated with this path
         */
        public MediaCollectionViewModel MediaCollection { get; private set; }


        public string Name
        {
            get { return _mapPath.Name; }
        }



        private RelayCommand _openCollectionCommand;
        public ICommand OpenCollectionCommand => _openCollectionCommand;

        /**
         * Open the full gallery of associated Media
         */
        public void OpenCollection()
        {
            _nav.NewPage(new GalleryViewModel(_mapPath.Name, _nav, MediaCollection.MediaCollectionModel, null));
        }
    }
}
