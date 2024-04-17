using PhotoGalleryApp.Models;
using PhotoGalleryApp.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /**
     * Maintains a list of ViewModels for each MapItem in a map.
     */
    class MapItemView : ModelVMView<MapItem, MapItemViewModel>
    {
        public MapItemView(ObservableCollection<MapItem> modelColl, MapViewModel map) : base(modelColl, true, false) 
        {
            _map = map;

            // pass false to the base class so it doesn't call refresh until the map is set.
            Refresh();
        }


        private MapViewModel _map;

        protected override MapItemViewModel CreateViewModel(MapItem item)
        {
            if (item is MapLocation)
                return new MapLocationViewModel(MainWindow.GetNavigator(), (MapLocation)item, _map);
            else
                return new MapPathViewModel(MainWindow.GetNavigator(), (MapPath)item, _map);
        }

        protected override MapItem GetModel(MapItemViewModel vm)
        {
            return vm.GetModel();
        }

        protected override bool IsCollection(MapItem item)
        {
            if (item is MapPath)
                return false;

            //do FIGURING;
            return false;
        }

        protected override IList GetCollection(MapItem item)
        {
            if (item is not MapLocation)
                throw new ArgumentException();

            return ((MapLocation)item).Children;
        }

        protected override void AddCollectionChangedListener(MapItem model, NotifyCollectionChangedEventHandler func)
        {
            if (model is not MapLocation)
                throw new ArgumentException();

            ((MapLocation)model).Children.CollectionChanged += func;
        }

        protected override void RemoveCollectionChangedListener(MapItem model, NotifyCollectionChangedEventHandler func)
        {
            if (model is not MapLocation)
                throw new ArgumentException();

            ((MapLocation)model).Children.CollectionChanged -= func;
        }
    }
}
