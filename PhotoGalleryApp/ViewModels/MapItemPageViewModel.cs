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
    /// Abstract ViewModel for a page displaying a MapItem's details
    /// </summary>
    public abstract class MapItemPageViewModel : ViewModelBase
    {
        public MapItemPageViewModel(MapItem item) 
        {
            _openCollectionCommand = new RelayCommand(OpenCollection);

            _mapItem = item;
            _mapItem.PropertyChanged += _mapItem_PropertyChanged;

            MediaCollection mainColl = MainWindow.GetCurrentSession().Gallery.Collection;

            // Only show the media associated with this location
            FilterSet filters = new FilterSet(mainColl);
            MapItemFilter filt = (MapItemFilter)filters.GetCriteriaByType(typeof(MapItemFilter));
            filt.MapItemCriteria = _mapItem;

            MediaCollection = new MediaCollectionViewModel(MainWindow.GetNavigator(), mainColl, true, null, filters);
        }

        public override void Cleanup()
        {
            _mapItem.PropertyChanged -= _mapItem_PropertyChanged;
        }


        protected MapItem _mapItem;


        /**
         * Preview of media associated with this path
         */
        public MediaCollectionViewModel MediaCollection { get; private set; }


        public string Name
        {
            get { return _mapItem.Name; }
        }



        private RelayCommand _openCollectionCommand;
        public ICommand OpenCollectionCommand => _openCollectionCommand;

        /**
         * Open the full gallery of associated Media
         */
        public void OpenCollection()
        {
            NavigatorViewModel nav = MainWindow.GetNavigator();
            nav.NewPage(new GalleryViewModel(_mapItem.Name, nav, MediaCollection.MediaCollectionModel, null));
        }


        protected virtual void _mapItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapItem.Name))
                OnPropertyChanged(nameof(Name));
        }





        /**
         * Generator to create PathPageViewModels and LocationPageViewModels. 
         */
        public static MapItemPageViewModel CreateMapItemPageViewModel(MapItem item)
        {
            if (item is MapPath)
                return new PathPageViewModel((MapPath)item);

            else
                return new LocationPageViewModel((MapLocation)item);
        }
    }
}
