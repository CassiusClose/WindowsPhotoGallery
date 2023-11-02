using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media.Media3D;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A class used to hold, sort, and filter a list of ICollectableViewModel items.
    /// </summary>
    public class MediaView
    {
        /// <summary>
        /// </summary>
        /// <param name="maxLabel">The highest level of time to show labels for. If this is null, will determine this based on whether there are multiple
        /// of that time range in the collection. So if all the media is from the same year, it will not show the year label.</param>
        public MediaView(NavigatorViewModel nav, MediaCollection collection, int thumbnailHeight, FilterSet? filters=null, bool useLabels=true, bool expandEvents=false, TimeRange? maxLabel=TimeRange.Year)
        {
            _nav = nav;
            _collection = collection;
            _thumbnailHeight = thumbnailHeight;
            _useLabels = useLabels;
            _maxLabel = maxLabel;
            _expandEvents = expandEvents;

            if(filters != null)
            {
                if(filters.MediaCollection != collection)
                    throw new ArgumentException("The MediaCollection in the FilterSet passed to MediaView is not the same MediaCollection passed to MediaView.");
            }
            if (filters == null)
                filters = new FilterSet(collection);

            _filters = filters;
            filters.FilterCriteriaLoosened += FilterLoosened;
            filters.FilterCriteriaTightened += FilterTightened;
            filters.FilteredPropertyChanged += FilterStatusChanged;


            _collection.CollectionChanged += MediaCollectionChanged;
            _collection.ItemPropertyChanged += CollectionItem_PropertyChanged;

            _viewList = new ObservableCollection<ICollectableViewModel>();

            // Build the initial list
            MediaCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Cleanup()
        {
            _filters.FilterCriteriaLoosened -= FilterLoosened;
            _filters.FilterCriteriaTightened -= FilterTightened;
            _filters.FilteredPropertyChanged -= FilterStatusChanged;

            _collection.CollectionChanged -= MediaCollectionChanged;
            _collection.ItemPropertyChanged -= CollectionItem_PropertyChanged;

            foreach(ICollectableViewModel item in _viewList)
                item.Cleanup();

            _filters.Cleanup();
        }


        private NavigatorViewModel _nav;
        private MediaCollection _collection;


        // _fullList has all the items. _viewList contains the list filtered and sorted, with labels added
        private ObservableCollection<ICollectableViewModel> _viewList;

        public ObservableCollection<ICollectableViewModel> View
        {
            get { return _viewList; }
        }



        private bool _expandEvents;
        /// <summary>
        /// Whether to represent an Event as a single EventTileViewModel or to replace it with
        /// all of its child Media.
        /// </summary>
        public bool ExpandEvents
        {
            get { return _expandEvents; }
            set
            {
                if (_expandEvents != value) 
                {
                    _expandEvents = value;
                    CollectionChanged_Reset();
                }
            }
        }

        private int _thumbnailHeight;
        private bool _useLabels;

        /**
         * The max level of time label to display. If this is null, then go into a dynamic mode, where it will only display labels for which 
         * there are multiple for a given time range. For example, if all of the media is from the same year, it will not display a year label.
         */
        private TimeRange? _maxLabel;


        private FilterSet _filters;
        public FilterSet ViewFilters { get { return _filters; } }



        /**
         * Apply all filters to the given item and return the results
         */
        private bool FilterResults(ICollectable c)
        {
            return _filters.Filter(c);
        }



        /// <summary>
        /// Completely rebuild the view. Used when a more specific action cannot be determined. Otherwise, use one
        /// of the other public methods here.
        /// </summary>
        public void Refresh()
        {
            CollectionChanged_Reset();
        }


        /// <summary>
        /// When the filter has become more restrictive (a tag added to it), then remove any items
        /// in the view that no longer meet the filter
        /// </summary>
        public void FilterTightened()
        {
            for(int i = 0; i < _viewList.Count; i++)
            {
                if (_viewList[i] is not TimeLabelViewModel && !FilterResults(_viewList[i].GetModel()))
                    RemoveAt(i--);
            }
        }

        /// When the filter has become less restrictive (a tag removed from it), then add any items
        /// that now fit the filter
        public void FilterLoosened()
        {
            if(!_filters.AreFiltersActive())
            {
                //TODO Is it more efficient to rebuild the whole list when there are no active filters?
            }

            // Generate list of all items valid with the current filter
            List<ICollectable> items = new List<ICollectable>();
            foreach(ICollectable c in _collection)
            {
                AddICollectableToList(items, c);
            }


            foreach(ICollectable c in items)
            {
                bool found = false;
                foreach(ICollectableViewModel vm in _viewList)
                {
                    if(vm.GetModel() == c)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    AddItem(CreateICollectableViewModel(c));
            }
        }


        /// <summary>
        /// When an ICollectableViewModel property used in filtering (its list of tags) has changed,
        /// then update only that item in the view.
        /// </summary>
        /// <param name="vm"></param>
        public void FilterStatusChanged(ICollectable c)
        {
            int ind;
            for(ind = 0; ind < _viewList.Count; ind++)
            {
                if (_viewList[ind].GetModel() == c)
                    break;
            }

            bool result = FilterResults(c);

            if (result && ind == _viewList.Count)
            {
                AddItem(CreateICollectableViewModel(c));
            }
            else if (!result && ind != _viewList.Count) 
            { 
                RemoveAt(ind);
            }
        }



        /**
         * Adds an item to the view list, and any labels needed
         */
        private void AddItem(ICollectableViewModel vm)
        {
            // If the view is empty, insert the item and its labels
            if(_viewList.Count == 0)
            {
                if(_useLabels)
                {
                    if(ShouldShowYears())
                        _viewList.Add(new TimeLabelViewModel(new PrecisionDateTime(vm.Timestamp, TimeRange.Year)));
                    if(ShouldShowMonths())
                        _viewList.Add(new TimeLabelViewModel(new PrecisionDateTime(vm.Timestamp, TimeRange.Month)));
                    if(ShouldShowDays())
                        _viewList.Add(new TimeLabelViewModel(new PrecisionDateTime(vm.Timestamp, TimeRange.Day)));
                }
                _viewList.Add(vm);
                return;
            }

            PrecisionDateTime? lastLabel = null;
            // Find the point where the item belongs, sorting-wise
            for(int i = 0; i < _viewList.Count+1; i++)
            {
                if(i == _viewList.Count || _viewList[i].Timestamp > vm.Timestamp)
                {
                    if(_useLabels)
                    {
                        // Add labels if they don't already exist
                        if ((lastLabel == null || lastLabel.Year != vm.Timestamp.Year) && ShouldShowYears())
                            _viewList.Insert(i++, new TimeLabelViewModel(new PrecisionDateTime(vm.Timestamp, TimeRange.Year)));

                        if ((lastLabel == null || lastLabel.Month != vm.Timestamp.Month) && ShouldShowMonths())
                            _viewList.Insert(i++, new TimeLabelViewModel(new PrecisionDateTime(vm.Timestamp, TimeRange.Month)));

                        if ((lastLabel == null || lastLabel.Day != vm.Timestamp.Day) && ShouldShowDays())
                            _viewList.Insert(i++, new TimeLabelViewModel(new PrecisionDateTime(vm.Timestamp, TimeRange.Day)));
                    }

                    _viewList.Insert(i, vm);
                    break;
                }

                if (_useLabels && _viewList[i] is TimeLabelViewModel)
                    lastLabel = _viewList[i].Timestamp;
            }
        }


        private void RemoveAt(int i)
        {

            if (_viewList[i] is TimeLabelViewModel)
                throw new Exception("Index is a TimeLabelViewModel");

            _viewList.RemoveAt(i);

            if (!_useLabels)
                return;

            // If the last item in the collection, then remove any adjacent labels b/c no content left for them
            if(i == _viewList.Count)
            {
                i--;
                while (i >= 0 && _viewList[i] is TimeLabelViewModel)
                {
                    _viewList.RemoveAt(i--);
                }
            }

            // If the month and year labels are empty, remove them
            // Day
            else if (ShouldShowDays() && _viewList[i - 1] is TimeLabelViewModel && _viewList[i] is TimeLabelViewModel)
            {
                _viewList.RemoveAt(i - 1);
                i--;

                // Month
                if (ShouldShowMonths() && _viewList[i - 1] is TimeLabelViewModel && _viewList[i] is TimeLabelViewModel &&
                    ((TimeLabelViewModel)_viewList[i]).Timestamp.Precision != TimeRange.Day )
                {
                    _viewList.RemoveAt(i - 1);
                    i--;

                    //Year
                    if (ShouldShowYears() && _viewList[i - 1] is TimeLabelViewModel && _viewList[i] is TimeLabelViewModel &&
                        ((TimeLabelViewModel)_viewList[i]).Timestamp.Precision == TimeRange.Year)
                    {
                        _viewList.RemoveAt(i - 1); 
                    }
                }
            }
        }


        private bool ShouldShowYears()
        {
            if (_maxLabel == null)
                return HasMultipleYears();
            else
                return _maxLabel <= TimeRange.Year;
        }
        private bool ShouldShowMonths()
        {
            if (_maxLabel == null)
                return HasMultipleMonths();
            else
                return _maxLabel <= TimeRange.Month;
        }
        private bool ShouldShowDays()
        {
            return true;
            /*if (_maxLabel == null)
                return HasMultipleDays();
            else
                return _maxLabel <= TimeRange.Day;*/
        }




        private bool HasMultipleYears()
        {
            return _collection.StartTimestamp.Year != _collection.EndTimestamp.Year;
        }
        private bool HasMultipleMonths()
        {
            return HasMultipleYears() || _collection.StartTimestamp.Month != _collection.EndTimestamp.Month;
        }

        private bool HasMultipleDays()
        {
            return HasMultipleMonths() || _collection.StartTimestamp.Day != _collection.EndTimestamp.Day;
        }





        /**
         * When the MediaCollection list changes, update the full list here from it. This means
         * creating ICollectableViewModel instances for each item.
         */
        private void MediaCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding items to MediaCollection, but NewItems is null");
                    CollectionChanged_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if(e.OldItems == null)
                        throw new ArgumentException("Removing items from MediaCollection, but OldItems is null");
                    CollectionChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if(e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing items in MediaCollection, but NewItems or OldItems is null");
                    CollectionChanged_Replace(e.NewItems, e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    CollectionChanged_Reset();
                    break;
            }
        }



        private void CollectionChanged_Add(IList newColls)
        {
            List<ICollectable> newItems = new List<ICollectable>(); 
            foreach(ICollectable c in newColls)
            {
                AddICollectableToList(newItems, c);
            }

            foreach(ICollectable c in newItems)
            {
                AddItem(CreateICollectableViewModel(c));
            }
        }

        private void CollectionChanged_Remove(IList oldI)
        {
            List<ICollectable> oldColls = oldI.Cast<ICollectable>().ToList();

            List<ICollectable> oldItems = new List<ICollectable>();
            foreach(ICollectable c in oldColls)
            {
                AddICollectableToList(oldItems, c);
            }


            foreach(ICollectable c in oldItems)
            {
                int i;
                for(i = 0; i < _viewList.Count; i++)
                {
                    if (_viewList[i].GetModel() == c)
                    {
                        RemoveAt(i);
                        break;
                    }
                }
                if(i == _viewList.Count)
                    Trace.WriteLine("Error: Can't find item when removing from MediaView");
            }
        }


        private void CollectionChanged_Replace(IList newItems, IList oldItems)
        {
            CollectionChanged_Remove(oldItems);
            CollectionChanged_Add(newItems);
        }


        private void CollectionChanged_Reset()
        {
            List<ICollectable> collList = new List<ICollectable>();
            foreach(ICollectable c in _collection)
            {
                AddICollectableToList(collList, c); 
            }

            List<ICollectableViewModel> newList = new List<ICollectableViewModel>();
            foreach(ICollectable c in collList) 
                newList.Add(CreateICollectableViewModel(c));


            newList = (newList.OrderBy(media => media.Timestamp)).ToList<ICollectableViewModel>();


            if (newList.Count != 0 && _useLabels)
            {
                int startingInd = 0;
                // Insert the initial labels
                PrecisionDateTime currTime = newList[0].Timestamp;
                if(ShouldShowYears())
                    newList.Insert(startingInd++, new TimeLabelViewModel(new PrecisionDateTime(currTime, TimeRange.Year)));

                if(ShouldShowMonths())
                    newList.Insert(startingInd++, new TimeLabelViewModel(new PrecisionDateTime(currTime, TimeRange.Month)));

                if(ShouldShowDays())
                    newList.Insert(startingInd++, new TimeLabelViewModel(new PrecisionDateTime(currTime, TimeRange.Day)));

                for (int i = startingInd+1; i < newList.Count; i++)
                {
                    // If the current item is a new year or month, add a label for it
                    PrecisionDateTime ts = newList[i].Timestamp;
                    if (ts.Year != currTime.Year && ShouldShowYears())
                    {
                        newList.Insert(i++, new TimeLabelViewModel(new PrecisionDateTime(ts, TimeRange.Year)));
                    }
                    if(ts.Month != currTime.Month && ShouldShowMonths())
                    {
                        newList.Insert(i++, new TimeLabelViewModel(new PrecisionDateTime(ts, TimeRange.Month)));
                    }
                    if(ts.Day != currTime.Day && ShouldShowDays())
                    {
                        newList.Insert(i++, new TimeLabelViewModel(new PrecisionDateTime(ts, TimeRange.Day)));
                    }
                    currTime = ts;
                }
            }

            _viewList.Clear();
            foreach(ICollectableViewModel vm in newList)
                _viewList.Add(vm);
        }


        /**
         * Adds the given collectable to the given list if it fits the current filter. If the collectable is an Event,
         * and ExpandEvents is true, then evaluate & add each item within the collection instead.
         */
        private void AddICollectableToList(List<ICollectable> list, ICollectable collectable)
        {
            if(collectable is Event)
            {
                if (ExpandEvents)
                {
                    Event e = (Event)collectable;
                    foreach (ICollectable c in e.Collection)
                    {
                        AddICollectableToList(list, c);
                    }
                }
                else if(FilterResults(collectable))
                        list.Add(collectable);
            }
            else if (FilterResults(collectable))
                list.Add(collectable);
        }




        /**
         * Generator for ICollectableViewModels
         */
        private ICollectableViewModel CreateICollectableViewModel(ICollectable model)
        {
            //TODO Localize
            if (model is Media)
            {
                MediaViewModel vm = MediaViewModel.CreateMediaViewModel((Media)model, true, 0, _thumbnailHeight);
                return vm;
            }
            else
            {
                EventTileViewModel vm = new EventTileViewModel((Event)model, _nav, _thumbnailHeight);
                return vm;
            }
        }


        /** 
         * Capture when an item in the collection changes its StartTimestamp
         */
        private void CollectionItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender is Event && e.PropertyName=="StartTimestamp") 
            {
                SortPropertyChanged((ICollectable)sender);
            }
        }

        /// <summary>
        /// When an ICollectableViewModel property used to sort (its timestamp) has changed,
        /// then update only that item in the view.
        /// </summary>
        /// <param name="vm"></param>
        public void SortPropertyChanged(ICollectable c)
        {
            if(c is Event && !ExpandEvents)
            {
                int ind;
                for (ind = 0; ind < _viewList.Count; ind++)
                {
                    if (_viewList[ind].GetModel() == c)
                        break;
                }

                if (ind != _viewList.Count)
                    RemoveAt(ind);

                if (FilterResults(c))
                    AddItem(CreateICollectableViewModel(c));
            }
        }
    }
}
