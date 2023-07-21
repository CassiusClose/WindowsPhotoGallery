using PhotoGalleryApp.Models;
using PhotoGalleryApp.ViewModels;
using System;
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
using System.Windows.Ink;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A class used to hold, sort, and filter a list of ICollectableViewModel items.
    /// </summary>
    public class MediaView
    {
        public MediaView(NavigatorViewModel nav, MediaCollection collection, int thumbnailHeight, bool useLabels=true)
        {
            _nav = nav;
            _collection = collection;
            _thumbnailHeight = thumbnailHeight;
            _useLabels = useLabels;
            _collection.CollectionChanged += MediaCollectionChanged;

            _fullList = new ObservableCollection<ICollectableViewModel>();
            _fullList.CollectionChanged += RefreshView;
            _viewList = new ObservableCollection<ICollectableViewModel>();

            // Build the initial list
            MediaCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        private NavigatorViewModel _nav;
        private MediaCollection _collection;


        // _fullList has all the items. _viewList contains the list filtered and sorted, with labels added
        private ObservableCollection<ICollectableViewModel> _fullList;
        private ObservableCollection<ICollectableViewModel> _viewList;

        public ObservableCollection<ICollectableViewModel> View
        {
            get { return _viewList; }
        }

        public ObservableCollection<ICollectableViewModel> AllItems
        {
            get { return _fullList; }
        }


        private int _thumbnailHeight;
        private bool _useLabels;


        // The method used to filter the items
        public delegate bool FilterDelegate(ICollectableViewModel vm);
        public FilterDelegate? Filter;








        /// <summary>
        /// Completely rebuild the view. Used when a more specific action cannot be determined. Otherwise, use one
        /// of the other public methods here.
        /// </summary>
        public void Refresh()
        {
            RefreshView(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// When an ICollectableViewModel property used in filtering (its list of tags) has changed,
        /// then update only that item in the view.
        /// </summary>
        /// <param name="vm"></param>
        public void FilterStatusChanged(ICollectableViewModel vm)
        {
            if (Filter == null)
                return;

            int ind = _viewList.IndexOf(vm);
            if (Filter(vm) && ind == -1)
            {
                RefreshView_Add(new List<ICollectableViewModel> { vm });
            }
            else if (!Filter(vm) && ind != -1) 
            { 
                RemoveAt(ind);
            }
        }

        /// <summary>
        /// When an ICollectableViewModel property used to sort (its timestamp) has changed,
        /// then update only that item in the view.
        /// </summary>
        /// <param name="vm"></param>
        public void SortPropertyChanged(ICollectableViewModel vm)
        {
            int ind = _viewList.IndexOf(vm);
            if(ind != -1)
            {
                RemoveAt(ind);
                RefreshView_Add(new List<ICollectableViewModel> { vm });
            }
        }

        /// <summary>
        /// When the filter has become more restrictive (a tag added to it), then remove any items
        /// in the view that no longer meet the filter
        /// </summary>
        public void FilterMoreRestrictive()
        {
            if (Filter == null)
                return;

            for(int i = 0; i < _viewList.Count; i++)
            {
                if (_viewList[i] is not TimeLabelViewModel && Filter != null && !Filter(_viewList[i]))
                    RemoveAt(i--);
            }
        }

        /// When the filter has become less restrictive (a tag removed from it), then add any items
        /// that now fit the filter
        public void FilterLessRestrictive()
        {
            for (int i = 0; i < _fullList.Count; i++)
            {
                List<ICollectableViewModel> newItems = new List<ICollectableViewModel>();
                if( (Filter == null && !_viewList.Contains(_fullList[i])) ||
                    (Filter != null && Filter(_fullList[i]) && !_viewList.Contains(_fullList[i]))
                  )
                    newItems.Add(_fullList[i]);

                RefreshView_Add(newItems);
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
                    _viewList.RemoveAt(i--);
            }

            // If the month and year labels are empty, remove them
            // Day
            else if (_viewList[i - 1] is TimeLabelViewModel && _viewList[i] is TimeLabelViewModel)
            {
                _viewList.RemoveAt(i - 1);
                i--;

                // Month
                if (_viewList[i - 1] is TimeLabelViewModel && _viewList[i] is TimeLabelViewModel &&
                    ((TimeLabelViewModel)_viewList[i]).Category != TimeRange.Day )
                {
                    _viewList.RemoveAt(i - 1);
                    i--;

                    //Year
                    if (_viewList[i - 1] is TimeLabelViewModel && _viewList[i] is TimeLabelViewModel &&
                        ((TimeLabelViewModel)_viewList[i]).Category == TimeRange.Year)
                    {
                        _viewList.RemoveAt(i - 1); 
                    }
                }
            }
        }



        /**
         * When the full list of items changes, update the view
         */
        private void RefreshView(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding items to MediaView, but NewItems is null");

                    RefreshView_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if(e.OldItems == null)
                        throw new ArgumentException("Removing items from MediaView, but OldItems is null");

                    RefreshView_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if(e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing items in MediaView, but NewItems or OldItems is null");

                    RefreshView_Replace(e.NewItems, e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    RefreshView_Reset();
                    break;

                default:
                    return;
            }
        }

        private void RefreshView_Add(IList newItems)
        {
            foreach(ICollectableViewModel vm in newItems)
            {
                // Don't add if doesn't meet filter
                if (Filter != null && !Filter(vm))
                    continue;

                // If the view is empty, insert the item and its labels
                if(_viewList.Count == 0)
                {
                    if(_useLabels)
                    {
                        _viewList.Add(new TimeLabelViewModel(vm.Timestamp, TimeRange.Year));
                        _viewList.Add(new TimeLabelViewModel(vm.Timestamp, TimeRange.Month));
                        _viewList.Add(new TimeLabelViewModel(vm.Timestamp, TimeRange.Day));
                    }
                    _viewList.Add(vm);
                    continue;
                }

                DateTime? lastLabel = null;
                // Find the point where the item belongs, sorting-wise
                for(int i = 0; i < _viewList.Count+1; i++)
                {
                    if(i == _viewList.Count || _viewList[i].Timestamp > vm.Timestamp)
                    {
                        if(_useLabels)
                        {
                            // Add labels if they don't already exist
                            if (lastLabel == null || ((DateTime)lastLabel).Year != vm.Timestamp.Year)
                                _viewList.Insert(i++, new TimeLabelViewModel(vm.Timestamp, TimeRange.Year));

                            if (lastLabel == null || ((DateTime)lastLabel).Month != vm.Timestamp.Month)
                                _viewList.Insert(i++, new TimeLabelViewModel(vm.Timestamp, TimeRange.Month));

                            if (lastLabel == null || ((DateTime)lastLabel).Day != vm.Timestamp.Day)
                                _viewList.Insert(i++, new TimeLabelViewModel(vm.Timestamp, TimeRange.Day));
                        }

                        _viewList.Insert(i, vm);
                        break;
                    }

                    if (_useLabels && _viewList[i] is TimeLabelViewModel)
                        lastLabel = _viewList[i].Timestamp;
                }
            }
        }

        private void RefreshView_Remove(IList oldItems)
        {
            // Find the item and remove it
            foreach(ICollectableViewModel vm in oldItems)
            {
                int i;
                for(i = 0; i < _viewList.Count; i++)
                {
                    if (vm == _viewList[i])
                        break;
                }

                RemoveAt(i);
            }
        } 

        private void RefreshView_Replace(IList newItems, IList oldItems)
        {
            RefreshView_Add(newItems);
            RefreshView_Remove(oldItems);
        }

        private void RefreshView_Reset()
        {
            // Copy and sort the full list
            _viewList = new ObservableCollection<ICollectableViewModel>(_fullList.OrderBy(media => media.Timestamp));

            // Remove any items that don't meet the filter criteria
            for (int i = 0; i < _viewList.Count; i++)
            {
                if (Filter != null && !Filter(_viewList[i]))
                {
                    _viewList.RemoveAt(i);
                    i--;
                }
            }

            if (_viewList.Count == 0)
                return;

            if (!_useLabels)
                return;

            // Insert the initial labels
            int year = _viewList[0].Timestamp.Year;
            int month = _viewList[0].Timestamp.Month;
            int day = _viewList[0].Timestamp.Day;
            _viewList.Insert(0, new TimeLabelViewModel(year));
            _viewList.Insert(1, new TimeLabelViewModel(year, month, day, TimeRange.Month));
            _viewList.Insert(2, new TimeLabelViewModel(day, month, day, TimeRange.Day));
            for (int i = 4; i < _viewList.Count; i++)
            {
                // If the current item is a new year or month, add a label for it
                DateTime ts = _viewList[i].Timestamp;
                if (ts.Year != year)
                {
                    year = ts.Year;
                    _viewList.Insert(i++, new TimeLabelViewModel(year));
                }
                if(ts.Month != month)
                {
                    month = ts.Month;
                    _viewList.Insert(i++, new TimeLabelViewModel(year, month, day, TimeRange.Month));
                }
                if(ts.Day != day)
                {
                    day = ts.Day;
                    _viewList.Insert(i++, new TimeLabelViewModel(year, month, day, TimeRange.Month));
                }
            }
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


        private void CollectionChanged_Add(IList newItems)
        {
            foreach(ICollectable model in newItems)
            {
                ICollectableViewModel vm = CreateICollectableViewModel(model);
                _fullList.Add(vm);
            }
        }

        private void CollectionChanged_Remove(IList oldI)
        {
            List<ICollectable> oldItems = oldI.Cast<ICollectable>().ToList();

            if(oldItems.Count > 0)
            {
                // For each item in the list, iterate through the removed items to find a match.
                // Do this because likely the removed items list will be much shorter than the
                // full list
                for (int i = 0; i < _fullList.Count; i++)
                {
                    for (int j = 0; j < oldItems.Count; j++)
                    {
                        if (_fullList[i].GetModel() == oldItems[j])
                        {
                            _fullList.RemoveAt(i);
                            oldItems.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }

        private void CollectionChanged_Replace(IList newItems, IList oldItems)
        {
            CollectionChanged_Remove(oldItems);
            CollectionChanged_Add(newItems);
        }

        private void CollectionChanged_Reset()
        {
            // Remove any view models where the model doesn't exist anymore
            for (int i = 0; i < _fullList.Count; i++)
            {
                bool found = false;
                foreach (ICollectable c in _collection)
                {
                    if(c == _fullList[i].GetModel())
                    {
                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    _fullList.RemoveAt(i);
                    i--;
                }
            }

            // Add view models for media that is new in the collection
            List<ICollectableViewModel> toAdd = new List<ICollectableViewModel>();
            foreach (ICollectable c in _collection)
            {
                bool found = false;
                foreach (ICollectableViewModel vm in _fullList)
                {
                    if(vm.GetModel() == c)
                    {
                        found = true;
                        break;
                    }
                }
                if(!found)
                    _fullList.Add(CreateICollectableViewModel(c));
            }
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
                vm.PropertyChanged += Item_PropertyChanged;
                return vm;
            }
            else
            {
                EventTileViewModel vm = new EventTileViewModel((Event)model, _nav, _thumbnailHeight);
                vm.PropertyChanged += Item_PropertyChanged;
                return vm;
            }
        }

        /**
         * When a item's timestamp has changed, resort it in the list
         */
        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender == null || sender is not ICollectableViewModel)
                return;

            if(e.PropertyName == "Timestamp")
            {
                ICollectableViewModel vm = (ICollectableViewModel)sender;
                SortPropertyChanged(vm);
            }
        }
    }
}
