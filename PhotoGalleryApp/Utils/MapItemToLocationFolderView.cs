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
using System.Windows;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A FolderView which takes a list of MapItems and provides a
    /// FolderViewModel for each MapLocation in the list.
    /// </summary>
    public class MapItemToLocationFolderView : FolderView<MapItem>
    {
        public MapItemToLocationFolderView(ObservableCollection<MapItem> modelColl) : base(modelColl) {}

        protected override void AddCollectionChangedListener(MapItem model, NotifyCollectionChangedEventHandler func)
        {
            if (model is MapLocation)
                ((MapLocation)model).Children.CollectionChanged += func;
        }


        protected override FolderViewModel? CreateViewModel(MapItem item)
        {
            if(item is MapLocation) 
                return new MapLocationFolderViewModel((MapLocation)item);
            return null;
        }

        protected override IList GetCollection(MapItem item)
        {
            return ((MapLocation)item).Children;
        }

        protected override MapItem GetModel(FolderViewModel vm)
        {
            return ((MapLocationFolderViewModel)vm).GetModel();
        }

        protected override bool IsCollection(MapItem item)
        {
            return item is MapLocation;
        }

        protected override void RemoveCollectionChangedListener(MapItem model, NotifyCollectionChangedEventHandler func)
        {
            if (model is MapLocation)
                ((MapLocation)model).Children.CollectionChanged -= func;
        }
    }
}
