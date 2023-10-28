using PhotoGalleryApp.Models;
using PhotoGalleryApp.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// Maintains a list of folders for each year, month, and event of a MediaCollection.
    /// </summary>
    public class FolderView
    {
        public FolderView(MediaCollection coll, bool showFoldersWithoutEvents=true)
        {
            _showFoldersWithoutEvents = showFoldersWithoutEvents;

            _collection = coll;
            _collection.CollectionChanged += MediaCollectionChanged;

            Folders = new ObservableCollection<FolderLabelViewModel>();
            ResetList();
        }


        /// <summary>
        /// Cleans up instance when it's no longer used. Will not function properly
        /// after this is called. Use this if you want to clean things up at a determined
        /// time, rather than waiting for garbage collection.
        /// </summary>
        public void Cleanup()
        {
            _collection.CollectionChanged -= MediaCollectionChanged;
            foreach(FolderLabelViewModel vm in Folders)
                vm.Cleanup();
        }


        private MediaCollection _collection;

        public ObservableCollection<FolderLabelViewModel> Folders
        {
            get;
        }


        /**
         * Whether date folders that contain no events should be displayed
         */
        private bool _showFoldersWithoutEvents;



        public FolderLabelViewModel.FolderOpenedDelegate? FolderOpened;

        /** Pass child folder opened events along to listeners */
        private void ChildFolderOpened(FolderLabelViewModel vm)
        {
            if(FolderOpened != null)
                FolderOpened(vm);
        }



        /** Update the list of folders if events removed or added */
        private void MediaCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding items to MediaCollection, but NewItems is null");

                    MediaCollectionChanged_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing items from MediaCollection, but OldItems is null");

                    MediaCollectionChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing items in MediaCollection, but NewItems or OldItems is null");

                    MediaCollectionChanged_Add(e.NewItems);
                    MediaCollectionChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ResetList();
                    break;
            }
        }

        private void MediaCollectionChanged_Add(IList newItems)
        {
            foreach(ICollectable c in newItems)
            {
                c.PropertyChanged += Item_PropertyChanged;
                if (c is Media && _showFoldersWithoutEvents)
                {
                    GetMonthFolder(((Media)c).Timestamp);
                    continue;
                }

                // For each new event, add an entry into its proper folder
                Event e = (Event)c;
                FolderLabelViewModel month = GetMonthFolder(e.StartTimestamp);
                FolderLabelViewModel eventFolder = CreateEventFolder(e);
                eventFolder.FolderOpened += ChildFolderOpened;
                month.Children.Add(eventFolder);
            }
        }

        private void MediaCollectionChanged_Remove(IList oldItems)
        {
            foreach(ICollectable c in oldItems)
            {
                c.PropertyChanged -= Item_PropertyChanged;

                if (!_showFoldersWithoutEvents && c is Media)
                    continue;

                PrecisionDateTime ts;
                if (c is Media)
                    ts = ((Media)c).Timestamp;
                else
                    ts = ((Event)c).StartTimestamp;

                // Find the year and month folder of the event

                int yearInd = GetYearIndex(ts);
                if (yearInd == -1)
                    throw new Exception("Year folder doesn't exist when removing item");
                FolderLabelViewModel year = Folders[yearInd];

                int monthInd = GetMonthIndex(year, ts);
                if (monthInd == -1)
                    throw new Exception("Month folder doesn't exist when removing item");
                FolderLabelViewModel month = year.Children[monthInd];



                if (c is Event)
                {
                    Event e = (Event)c;

                    // Find the event, and remove it
                    for (int i = 0; i < month.Children.Count; i++)
                    {
                        if (month.Children[i].Label == e.Name)
                        {
                            month.Children.RemoveAt(i);
                            break;
                        }
                    }
                }

                if(_showFoldersWithoutEvents)
                {
                    // Find if media exists for the year and month
                    bool monthFound = false;
                    bool yearFound = false;
                    PrecisionDateTime pdt;
                    foreach(ICollectable co in _collection)
                    {
                        if (co is Media)
                            pdt = ((Media)co).Timestamp;
                        else
                            pdt = ((Event)co).Collection.StartTimestamp;

                        if (pdt.Year == ts.Year)
                            yearFound = true;

                        if(pdt.Month == ts.Month)
                        {
                            monthFound = true;
                            break;
                        }
                    }

                    // If the month or year folders are now empty, remove them
                    if (!monthFound)
                        year.Children.RemoveAt(monthInd);

                    if (!yearFound)
                        Folders.RemoveAt(yearInd);
                }
                else if(c is Event)
                {
                    if (month.Children.Count == 0)
                        year.Children.RemoveAt(monthInd);

                    if (year.Children.Count == 0)
                        Folders.RemoveAt(yearInd);
                }
            }
        }

        /**
         * Rebuilds the list of folders. There is a folder for each year and month that contains 
         * media, and a folder for each event in its month folder.
         */
        private void ResetList()

        {
            Folders.Clear();
            foreach(ICollectable model in _collection)
            {
                model.PropertyChanged -= Item_PropertyChanged;
                model.PropertyChanged += Item_PropertyChanged;
                if(model is Media)
                {
                    if(_showFoldersWithoutEvents)
                    {
                        Media m = (Media)model;
                        // Create folder if doesn't exist
                        FolderLabelViewModel month = GetMonthFolder(m.Timestamp);
                    }
                }
                else
                {
                    Event e = (Event)model;
                    // Create folder if doesn't exist
                    FolderLabelViewModel month = GetMonthFolder(e.StartTimestamp);
                    month.Children.Add(CreateEventFolder(e));
                }
            }
        }




        /** Create a folder for an event, and nest folders for each child event in it */
        private FolderLabelViewModel CreateEventFolder(Event e)
        {
            FolderLabelViewModel vm = new FolderLabelViewModel(e.Name, new PrecisionDateTime(e.StartTimestamp, TimeRange.Second));
            vm.FolderOpened += ChildFolderOpened;
            foreach(ICollectable c in e.Collection)
            {
                if (c is Event)
                    vm.Children.Add(CreateEventFolder((Event)c));
            }
            return vm;
        }




        /** Gets the index of the given month's folder in the year folder */
        private int GetMonthIndex(FolderLabelViewModel year, PrecisionDateTime ts)
        {
            PrecisionDateTime goal = new PrecisionDateTime(ts, TimeRange.Month);

            int i = 0;
            for(i = 0; i < year.Children.Count; i++)
            {
                if (year.Children[i].Timestamp == goal)
                    return i;
                else if (year.Children[i].Timestamp > goal)
                    break;
            }

            return -1;
        }

        /** 
         * Returns the month folder given by the timestamp. Will create the year or month folder
         * if they do not already exist
         */
        private FolderLabelViewModel GetMonthFolder(PrecisionDateTime ts)
        {
            FolderLabelViewModel year = GetYearFolder(ts);

            PrecisionDateTime goal = new PrecisionDateTime(ts, TimeRange.Month);

            int i = 0;
            for(i = 0; i < year.Children.Count; i++)
            {
                if (year.Children[i].Timestamp == goal)
                    return year.Children[i];
                else if (year.Children[i].Timestamp > goal)
                    break;
            }

            FolderLabelViewModel v = new FolderLabelViewModel(ts.ToString("MMMM"), goal);
            v.FolderOpened += ChildFolderOpened;
            year.Children.Insert(i,v);
            return v;
        }

        /** Gets the index of the given year's folder in list of folders */
        private int GetYearIndex(PrecisionDateTime ts)
        {
            PrecisionDateTime goal = new PrecisionDateTime(ts, TimeRange.Year);

            int i = 0;
            for(i = 0; i < Folders.Count; i++)
            {
                if (Folders[i].Timestamp == goal)
                    return i;
                else if (Folders[i].Timestamp > goal)
                    break;
            }

            return -1;
        }

        /** 
         * Returns the year folder given by the timestamp. Will create the year folder
         * if it does not already exist
         */
        private FolderLabelViewModel GetYearFolder(PrecisionDateTime ts)
        {
            PrecisionDateTime goal = new PrecisionDateTime(ts, TimeRange.Year);

            int i = 0;
            for(i = 0; i < Folders.Count; i++)
            {
                if (Folders[i].Timestamp == goal)
                    return Folders[i];
                else if (Folders[i].Timestamp > goal)
                    break;
            }
            FolderLabelViewModel v = new FolderLabelViewModel(ts.Year.ToString(), goal);
            v.FolderOpened += ChildFolderOpened;
            Folders.Insert(i,v);
            return v;
        }




        /**
         * When a item's timestamp has changed, resort it in the list
         */
        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender == null || sender is not ICollectableViewModel)
                return;

            // If Media type
            if(e.PropertyName == "Timestamp")
            {
                // There is no folder for a Media item, so no way to know where the media used to be.
                // Instead of searching the collection for folders that have no items, just rebuild the list
                ResetList();
            }
            // If Event type
            else if(e.PropertyName == "StartTimestamp")
            {
                Event model = (Event)sender;

                // Remove the old folder
                RemoveEventWithNewTime(Folders, model);

                // Create a new one
                CreateEventFolder(model);
            }
        }


        /**
         * When an event's timestamp changes, must remove its old entry from the folder structure. But its
         * timestamp is new, so have to search through the whole folder structure to find its old entry
         * to remove.
         */
        private bool RemoveEventWithNewTime(ObservableCollection<FolderLabelViewModel> list, Event e)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i].Label == e.Name)
                {
                    list.RemoveAt(i);
                    return true;
                }

                if (RemoveEventWithNewTime(list[i].Children, e))
                    return true;
            }

            return false;
        }
    }
}
