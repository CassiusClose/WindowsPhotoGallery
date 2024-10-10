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
    /// A View which converts a list of MapLocations to a list of view models
    /// to display their names.
    /// </summary>
    public class LocationNameView : ModelVMView<MapLocation, MapLocationNameViewModel>
    {
        public LocationNameView(ObservableCollection<MapLocation> coll) : base(coll) { }

        protected override void AddCollectionChangedListener(MapLocation model, NotifyCollectionChangedEventHandler func)
        {
            throw new NotImplementedException();
        }

        protected override MapLocationNameViewModel? CreateViewModel(MapLocation item)
        {
            return new MapLocationNameViewModel(item);
        }

        protected override IList GetCollection(MapLocation item)
        {
            throw new NotImplementedException();
        }

        protected override MapLocation GetModel(MapLocationNameViewModel vm)
        {
            return vm.Location;
        }

        protected override bool IsCollection(MapLocation item)
        {
            return false;
        }

        protected override void RemoveCollectionChangedListener(MapLocation model, NotifyCollectionChangedEventHandler func)
        {
            throw new NotImplementedException();
        }
    }
}
