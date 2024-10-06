using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Filtering
{
    /// <summary>
    /// Allows the user to choose one MapItem from the global list of map items
    /// </summary>
    public class MapItemFilter : FilterCriteria
    {
        public MapItemFilter(MediaCollection mediaCollection) : base(mediaCollection) { }

        public MapItemFilter(MediaCollection mediaCollection, MapItem item) : base(mediaCollection)
        {
            MapItemCriteria = item;

            mediaCollection.MapItemsChanged += CollectionItem_MapItemsChanged;
        }


        /**
         * The MapItem that is the filter
         */
        private MapItem? _mapItem = null;
        public MapItem? MapItemCriteria 
        {
            get { return _mapItem; }
            set {
                _mapItem = value;
                if(FilterCriteriaChanged != null)
                    FilterCriteriaChanged();

                OnPropertyChanged();
            }
        }
        

        public override void ClearFilter()
        {
            MapItemCriteria = null;

            if(FilterCriteriaLoosened != null)
                FilterCriteriaLoosened();
        }

        /**
         * If a child's MapItem changes, then update its status in the filter
         */
        public override void CollectionItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is Media)
            {
                Media m = (Media)sender;
                if(e.PropertyName == nameof(m.MapItem))
                {
                    if (FilteredPropertyChanged != null)
                        FilteredPropertyChanged(m);
                }
            }
            else if (sender is Event)
            {
                Event ev = (Event)sender;
                if(e.PropertyName == nameof(ev.Collection.MapItems))
                {
                    if (FilteredPropertyChanged != null)
                        FilteredPropertyChanged(ev);
                }
            }
        }


        /**
         * If an item's MapItem collection changes, then update its status in the filter
         */
        private void CollectionItem_MapItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if(sender is ICollectable)
            {
                if (FilteredPropertyChanged != null)
                    FilteredPropertyChanged((ICollectable)sender);
            }
        }


        public override void CopyTo(FilterCriteria crit)
        {
            if(crit is not MapItemFilter)
                throw new ArgumentException("FilterCriteria CopyTo() was given a filter criteria of a different type");

            MapItemFilter f = (MapItemFilter)crit;
            if (!IsFilterActive())
                f.ClearFilter();
            else
                f.MapItemCriteria = MapItemCriteria;
        }


        public override bool Filter(ICollectable c)
        {
            if(c is Media)
            {
                Media m = (Media)c;

                if (m.MapItem == MapItemCriteria)
                    return true;

                if(m.MapItem is MapLocation && MapItemCriteria is MapLocation && ((MapLocation)m.MapItem).IsChildOf((MapLocation)MapItemCriteria))
                    return true;
            }
            else if (c is Event)
            {
                foreach(MapItem i in ((Event)c).Collection.MapItems)
                {
                    if (i == MapItemCriteria)
                        return true;

                    if(i is MapLocation && MapItemCriteria is MapLocation && ((MapLocation)i).IsChildOf((MapLocation)MapItemCriteria))
                            return true;

                }
            }

            return false;
        }


        public override bool IsFilterActive()
        {
            return _mapItem != null;
        }
    }
}
