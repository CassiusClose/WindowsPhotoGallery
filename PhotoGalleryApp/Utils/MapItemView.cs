using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

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
            _map.PropertyChanged += _map_PropertyChanged;

            _frozenTrees = new Dictionary<MapLocation, uint>();

            // pass false to the base class so it doesn't call refresh until the map is set.
            Refresh();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _map.PropertyChanged -= _map_PropertyChanged;
        }

        private MapViewModel _map;

        protected override MapItemViewModel CreateViewModel(MapItem item)
        {
            if (item is MapLocation)
            {
                MapLocation loc = (MapLocation)item;
                MapLocationViewModel vm = new MapLocationViewModel(MainWindow.GetNavigator(), loc, _map);
                loc.ChildrenLocationChanged += Child_LocationChanged;
                return vm;
            }
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

            MapLocation loc = (MapLocation)item;
            MapLocation topLevelParent = loc.GetRootNode();

            if (!_frozenTrees.ContainsKey(topLevelParent))
                return MapLocation.FarApart(loc, _map.ZoomLevel);


            uint locDepth = loc.GetTreeDepth();
            if (locDepth < _frozenTrees[topLevelParent] && loc.Children.Count > 0)
                return true;

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

            // There was a bug where the listener was getting added twice. Not
            // exactly sure why, but this prevents it.
            ((MapLocation)model).Children.CollectionChanged -= func;
            ((MapLocation)model).Children.CollectionChanged += func;
        }

        protected override void RemoveCollectionChangedListener(MapItem model, NotifyCollectionChangedEventHandler func)
        {
            if (model is not MapLocation)
                throw new ArgumentException();

            ((MapLocation)model).Children.CollectionChanged -= func;
        }

        protected override void PrepareForRemoval(MapItemViewModel vm)
        {
            if(vm is MapLocationViewModel)
            {
                ((MapLocation)((MapLocationViewModel)vm).GetModel()).ChildrenLocationChanged -= Child_LocationChanged;
            }
        }


        /**
         * Assumes the given MapLocation exists in the view. For the given
         * MapLocation, determine whether it should be displayed given the
         * current zoom level. If it should not be displayed, then remove it
         * and add the MapLocation that should be displayed at this zoom.
         */
        private void RecalcZoom(MapLocation loc)
        {
            if (_frozenTrees.ContainsKey(loc.GetRootNode()))
                return;

            if(loc.Parent != null)
            {
                // Are any of the parents are too close to each other?
                MapLocation? parent = loc.Parent;
                MapLocation? topTooClose = null;
                do
                {
                    if (!MapLocation.FarApart(parent, _map.ZoomLevel))
                        topTooClose = parent;

                    parent = parent.Parent;

                } while (parent != null);

                // If any of the parents are too close to each other, then show
                // only the MapLocation above it
                if(topTooClose != null)
                {
                    foreach (MapLocation child in loc.Parent.Children)
                        RemoveItem(child, false);

                    AddItem(topTooClose);
                    return;
                }
            }

            // No parent is too close, so look below
            // If this current one is now far enough apart, remove it & add the children
            if(MapLocation.FarApart(loc, _map.ZoomLevel))
            {
                RemoveItem(loc, false);

                foreach (MapLocation l in loc.Children)
                    AddItem(l);
            }
        }



        private void Child_LocationChanged(MapLocation loc)
        {
            // If the child is being displayed & is not part of the edit tree,
            // then refresh the MapLocation expansions
            foreach (MapItemViewModel item in View)
            {
                if (item is not MapLocationViewModel)
                    continue;

                MapLocationViewModel vm = (MapLocationViewModel)item;
                if(ReferenceEquals(vm.GetModel(), loc))
                {
                    RecalcZoom(loc);
                    return;
                }
            }
        }

        private void _map_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // If the map zooms, then recalculate which MapLocations should be expanded
            if(e.PropertyName == nameof(MapViewModel.ZoomLevel))
            {
                List<MapItemViewModel> oldView = new List<MapItemViewModel>(View);

                foreach(MapItemViewModel item in oldView)
                {
                    if (item is not MapLocationViewModel)
                        continue;

                    if (!View.Contains(item))
                        continue;

                    MapLocationViewModel vm = (MapLocationViewModel)item;
                    MapLocation loc = (MapLocation)vm.GetModel();
                    RecalcZoom(loc);

                }
            }
        }


        /**
         * Maintains a list of which MapLocation trees to be frozen, i.e.
         * displayed at a certain depth no matter the zoom level. Root nodes
         * should be stored here with a depth integer.
         */
        private Dictionary<MapLocation, uint> _frozenTrees;

        /// <summary>
        /// Unfreezes all MapLocation trees.
        /// </summary>
        public void ClearFrozen()
        {
            List<MapLocation> parents = _frozenTrees.Keys.ToList();
            foreach (MapLocation p in parents)
                RemoveAllItemsWithRootNode(p);

            _frozenTrees.Clear();

            foreach (MapLocation p in parents)
                AddItem(p);
        }

        /// <summary>
        /// Freezes the tree of the given MapLocation at the given
        /// MapLocation's depth
        /// </summary>
        public void FreezeMapLocationLevel(MapLocation loc)
        {
            MapLocation parent = loc.GetRootNode();
            RemoveAllItemsWithRootNode(parent);
            _frozenTrees[loc.GetRootNode()] = loc.GetTreeDepth();
            AddItem(parent);
        }

        /**
         * Removes any ViewModels from the View that have the given MapLocation
         * as their root node.
         */
        private void RemoveAllItemsWithRootNode(MapLocation parent) 
        {
            for(int i = 0; i < View.Count; i++) 
            {
                if (View[i] is not MapLocationViewModel)
                    continue;

                MapLocation l = (MapLocation)View[i].GetModel();
                if (ReferenceEquals(l.GetRootNode(), parent))
                {
                    RemoveItem(l);
                    i--;
                }
            }
        }
    }
}
