using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Xps;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel associated with the EventsView, which displays a folder list of all years, months, and events.
    /// </summary>
    class EventsViewModel : ViewModelBase
    {
        public EventsViewModel(NavigatorViewModel nav, MediaCollection coll)
        {
            _nav = nav; 
            _collection = coll;
            _collection.CollectionChanged += MediaCollectionChanged;

            _labels = new ObservableCollection<FolderLabelViewModel>();

            ResetList();
        }


        private NavigatorViewModel _nav;

        private MediaCollection _collection;

        private ObservableCollection<FolderLabelViewModel> _labels;
        /// <summary>
        /// A list of folders, one for each year that contains media
        /// </summary>
        public ObservableCollection<FolderLabelViewModel> Labels
        { get { return _labels; } }


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
                if (c is Media)
                    continue;

                // For each new event, add an entry into its proper folder
                Event e = (Event)c;
                FolderLabelViewModel month = GetMonthFolder(e.Collection.StartTimestamp);
                FolderLabelViewModel eventFolder = CreateEventFolder(e);
                eventFolder.FolderOpened += FolderOpened;
                month.Children.Add(eventFolder);
            }
        }

        private void MediaCollectionChanged_Remove(IList oldItems)
        {
            foreach(ICollectable c in oldItems)
            {
                PrecisionDateTime ts;
                if (c is Media)
                    ts = ((Media)c).Timestamp;
                else
                    ts = ((Event)c).Collection.StartTimestamp;

                // Find the year and month folder of the event

                int yearInd = GetYearIndex(ts);
                if (yearInd == -1)
                    throw new Exception("Year folder doesn't exist when removing item");
                FolderLabelViewModel year = Labels[yearInd];

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
                    Labels.RemoveAt(yearInd);
            }
        }

        /**
         * Rebuilds the list of folders. There is a folder for each year and month that contains 
         * media, and a folder for each event in its month folder.
         */
        private void ResetList()

        {
            _labels.Clear();
            foreach(ICollectable vm in _collection)
            {
                if(vm is Media)
                {
                    Media m = (Media)vm;
                    // Create folder if doesn't exist
                    FolderLabelViewModel month = GetMonthFolder(m.Timestamp);
                }
                else
                {
                    Event e = (Event)vm;
                    // Create folder if doesn't exist
                    FolderLabelViewModel month = GetMonthFolder(e.Collection.StartTimestamp);
                    month.Children.Add(CreateEventFolder(e));
                }
            }
        }



        /** Create a folder for an event, and nest folders for each child event in it */
        private FolderLabelViewModel CreateEventFolder(Event e)
        {
            FolderLabelViewModel vm = new FolderLabelViewModel(e.Name, new PrecisionDateTime(e.Collection.StartTimestamp, TimeRange.Second));
            vm.FolderOpened += FolderOpened;
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
            v.FolderOpened += FolderOpened;
            year.Children.Insert(i,v);
            return v;
        }

        /** Gets the index of the given year's folder in list of folders */
        private int GetYearIndex(PrecisionDateTime ts)
        {
            PrecisionDateTime goal = new PrecisionDateTime(ts, TimeRange.Year);

            int i = 0;
            for(i = 0; i < Labels.Count; i++)
            {
                if (Labels[i].Timestamp == goal)
                    return i;
                else if (Labels[i].Timestamp > goal)
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
            for(i = 0; i < Labels.Count; i++)
            {
                if (Labels[i].Timestamp == goal)
                    return Labels[i];
                else if (Labels[i].Timestamp > goal)
                    break;
            }
            FolderLabelViewModel v = new FolderLabelViewModel(ts.Year.ToString(), goal);
            v.FolderOpened += FolderOpened;
            Labels.Insert(i,v);
            return v;
        }




        /**
         * When one of the folders is opened, they will call this
         */
        private void FolderOpened(FolderLabelViewModel folder)
        {
            // If it's an event, open the event's collection
            if (folder.Timestamp.Precision == TimeRange.Second)
            {
                foreach (ICollectable c in _collection)
                {
                    if (c is Event)
                    {
                        Event e = (Event)c;
                        if (folder.Timestamp == e.Collection.StartTimestamp)
                        {
                            _nav.NewPage(new GalleryViewModel(_nav, e.Collection, null));
                            return;
                        }
                    }
                }
            }

            // If it's a time range (year or month), collect all the media for that time range and open it
            else 
            {
                List<ICollectable> list = new List<ICollectable>();
                foreach (ICollectable c in _collection)
                {
                    if (c is Event)
                    {
                        if(((Event)c).Collection.TimestampInRange(folder.Timestamp))
                            list.Add(c);
                    }
                    else
                    {
                        if (((Media)c).Timestamp.Matches(folder.Timestamp))
                            list.Add(c);
                    }
                }

                _nav.NewPage(new GalleryViewModel(_nav, new MediaCollection(list), folder.Timestamp.Precision+1));
            }
        }
    }
}
