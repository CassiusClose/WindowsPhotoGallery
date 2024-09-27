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
    /// <summary>
    /// A FolderView which takes a list of MapLocations and provides a
    /// FolderViewModel for each item in the list.
    /// </summary>
    public class MapLocationFolderView : FolderView<MapLocation>
    {
        public MapLocationFolderView(ObservableCollection<MapLocation> modelColl) : base(modelColl) {}

        protected override void AddCollectionChangedListener(MapLocation model, NotifyCollectionChangedEventHandler func)
        {
            model.Children.CollectionChanged += func;
        }

        protected override FolderViewModel? CreateViewModel(MapLocation item)
        {
            if (item is MapLocation) 
                return new MapLocationFolderViewModel(item);
            return null;
        }

        protected override IList GetCollection(MapLocation item)
        {
            return item.Children;
        }

        protected override MapLocation GetModel(FolderViewModel vm)
        {
            return ((MapLocationFolderViewModel)vm).GetModel();
        }

        protected override bool IsCollection(MapLocation item)
        {
            return true;
        }

        protected override void RemoveCollectionChangedListener(MapLocation model, NotifyCollectionChangedEventHandler func)
        {
            model.Children.CollectionChanged -= func;
        }
    }
}
