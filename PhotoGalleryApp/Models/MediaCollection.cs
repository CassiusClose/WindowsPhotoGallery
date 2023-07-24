using PhotoGalleryApp.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// A collection of Media objects. 
    /// </summary>
    public class MediaCollection : ObservableCollection<ICollectable>
    {
        #region Constructors

        /// <summary>
        /// Creates a MediaGallery object with the given name.
        /// </summary>
        public MediaCollection()
        {
            Tags = new RangeObservableCollection<string>();
        }

        public MediaCollection(List<ICollectable> list) : base(list)
        {
            Tags = new RangeObservableCollection<string>();
        }

        #endregion Constructors


        #region Fields and Properties

        /// <summary>
        /// A collection of all the tags present in the collection (compiled from the tags of each image). Tags are not
        /// added here, they are added to the media within the gallery, and those changes are reflected here.
        /// </summary>
        public RangeObservableCollection<string> Tags { get; private set; }



        /// <summary>
        /// The timestamp of the earliest media in the event.
        /// </summary>
        //TODO What if nothing in the event? Right now, default is year 1.
        private PrecisionDateTime? _startTimestamp = null;
        public PrecisionDateTime StartTimestamp
        {
            get 
            {
                if (_startTimestamp == null)
                    return new PrecisionDateTime();
                return (PrecisionDateTime)_startTimestamp; 
            }
            set
            {
                _startTimestamp = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(StartTimestamp)));
            }
        }


        //TODO Implement ISerializable and make internal set
        private PrecisionDateTime? _endTimestamp = null;
        public PrecisionDateTime EndTimestamp
        {
            get 
            {
                if (_endTimestamp == null)
                    return new PrecisionDateTime();
                return (PrecisionDateTime)_endTimestamp; 
            }
            set
            {
                _endTimestamp = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(EndTimestamp)));
            }
        }

        /// <summary>
        /// Returns whether the given timestamp is in the time range defined by the contents of
        /// this collection.
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public bool TimestampInRange(PrecisionDateTime ts)
        {
            if (StartTimestamp.Matches(ts) || EndTimestamp.Matches(ts) || (ts > StartTimestamp && ts < EndTimestamp))
                return true;

            return false;
        }



        /**
         * Pass on any children's tags CollectionChanged events.
         */
        [XmlIgnore]
        public NotifyCollectionChangedEventHandler? MediaTagsChanged; 

        #endregion Fields and Properties


        #region Methods


        /**
         * Reset the start and end timestamps
         */
        private void ResetTimeRange()
        {
            PrecisionDateTime? earliest = null;
            PrecisionDateTime? latest = null;
            foreach(ICollectable c in this)
            {
                if(c is Media)
                {
                    PrecisionDateTime t = ((Media)c).Timestamp;
                    if (earliest == null || t < earliest)
                        earliest = t;
                    if (latest == null || t > latest)
                        latest = t;
                }
                else
                {
                    PrecisionDateTime? t = ((Event)c).Collection.StartTimestamp;
                    if (t != null && (earliest == null || t < earliest))
                        earliest = t;
                    if (t != null && (latest == null || t > latest))
                        latest = t;
                }
            }

            if(earliest != null)
                StartTimestamp = earliest;
            if (latest != null)
                EndTimestamp = latest;
        }



        /**
         * When an item in the collection changes its timestamps, reset the timerange here
         */
        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender is Media)
            {
                Media m = (Media)sender;
                if (e.PropertyName == nameof(m.Timestamp))
                    ResetTimeRange();
            }
            else
            {
                Event ev = (Event)sender;
                if(e.PropertyName == nameof(ev.Collection.StartTimestamp))
                    ResetTimeRange();
            }
        }


        /**
         * Adds a media item to the collection. 
         */
        protected override void InsertItem(int index, ICollectable item)
        {
            if (item is Media)
            {
                Media m = (Media)item;
                m.TagsChanged += MediaTags_CollectionChanged;

                MediaTagsChanged_Add(m.Tags);

                if (_startTimestamp == null || m.Timestamp < StartTimestamp)
                    StartTimestamp = m.Timestamp;
                if (_endTimestamp == null || m.Timestamp > EndTimestamp)
                    EndTimestamp = m.Timestamp;
            }
            else
            {
                Event ev = (Event)item;

                // Uncomment if a MediaCollection's tag list should contain tags from nested events
                //((Event)i).Collection.AddTagsChangedListener(handler);
                //MediaTagsChanged_Add(((Event)item).Collection.Tags);

                if(StartTimestamp == null || ev.Collection.StartTimestamp < StartTimestamp)
                    StartTimestamp = ev.Collection.StartTimestamp;
                if(EndTimestamp == null || ev.Collection.EndTimestamp > EndTimestamp)
                    EndTimestamp = ev.Collection.EndTimestamp;
            }

            item.PropertyChanged += Item_PropertyChanged;

            // Do this after above, so the time range is reset by the time CollectionChanged notifications go out
            // InsertItem is used internally by functions such as Add, so overriding here is enough
            base.InsertItem(index, item);
        }


        /**
         * Removes a media item from the collection 
         */
        protected override void RemoveItem(int index)
        {
            ICollectable item = this[index];

            // When a photo is removed, need to refresh the list of tags 
            if (item is Media)
            {
                Media m = (Media)item;

                MediaTagsChanged_Remove(((Media)m).Tags);

                if (m.Timestamp.Equals(StartTimestamp) || m.Timestamp.Equals(EndTimestamp))
                    ResetTimeRange();
            }
            else
            {
                Event ev = (Event)item;

                // Uncomment if a MediaCollection's tag list should contain tags from nested events
                MediaTagsChanged_Remove(((Event)item).Collection.Tags);

                if (ev.Collection.StartTimestamp.Equals(StartTimestamp) || ev.Collection.EndTimestamp.Equals(EndTimestamp))
                    ResetTimeRange();
            }

            // Do this after above, so the time range is reset by the time CollectionChanged notifications go out
            // RemoveItem is used internally by functions such as Remove, so overriding here is enough
            base.RemoveItem(index);
        }

        
        /**
         * Recursively add a CollectionChanged listener to all items in this collection, and all
         * items in any collections (Events) that may be nested in this collection.
         */
        public void AddTagsChangedListener(NotifyCollectionChangedEventHandler handler)
        {
            foreach(ICollectable i in Items)
            {
                if(i is Media)
                    ((Media)i).TagsChanged += handler;
                // Uncomment if a MediaCollection's tag list should contain tags from nested events
                /*else
                    ((Event)i).Collection.AddTagsChangedListener(handler);
                */
            }
        }



        /**
         * ObservableCollection event handler: When a media items's tags update, need to refresh the list of tags in the collection 
         */
        private void MediaTags_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding tags to child of MediaCollection, but NewItems is null");

                    MediaTagsChanged_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing tags from a child of MediaCollection, but OldItems is null");

                    MediaTagsChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing tags from a child of MediaCollection, but OldItems or NewItems is null");

                    MediaTagsChanged_Add(e.NewItems);
                    MediaTagsChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    MediaTagsChanged_Reset();
                    break;
            }


            if (MediaTagsChanged != null)
                MediaTagsChanged(sender, e);

        }

        private void MediaTagsChanged_Add(IList newItems)
        {
            foreach(string tag in newItems)
            {
                if (!Tags.Contains(tag))
                    Tags.Add(tag);
            }
        }

        private void MediaTagsChanged_Remove(IList oldI)
        {
            List<string> oldItems = oldI.Cast<string>().ToList();

            foreach(ICollectable item in this)
            {
                for (int i = 0; i < oldItems.Count; i++)
                {
                    if(item is Media)
                    {
                        if (((Media)item).Tags.Contains(oldItems[i]))
                        {
                            oldItems.RemoveAt(i--);
                            continue;
                        }
                    }
                    // Uncomment if a MediaCollection's tag list should contain tags from nested events
                    /*else
                    {
                        if (((Event)item).Collection.Tags.Contains(oldItems[i]))
                            oldItems.RemoveAt(i--);
                    }*/
                }
            }

            foreach(string tag in oldItems)
            {
                Tags.Remove(tag);
            }
        }

        private void MediaTagsChanged_Reset()
        {
            List<string> newTags = new List<string>();

            foreach(ICollectable c in this)
            {
                if(c is Media)
                {
                    foreach (string tag in ((Media)c).Tags)
                    {
                        if (!newTags.Contains(tag))
                            newTags.Add(tag);
                    }
                }
                // Uncomment if a MediaCollection's tag list should contain tags from nested events
                /*else
                {
                    foreach(string tag in ((Event)c).Collection.Tags) 
                    { 
                        if (!newTags.Contains(tag))
                            newTags.Add(tag);
                    }
                }*/
            }

            // Replace the list & send out one "reset" notification. It's likely more efficient than
            // sending out a "reset" when calling Clear() and then sending out an "add" for each tag
            Tags.ReplaceItems(newTags);
        }
        



        /// <summary>
        /// Returns a list of all the Event objects nested in this collection.
        /// </summary>
        /// <returns>All the events contained in the collection.</returns>
        /*public List<Event> GetEvents()
        {
            List<Event> events = new List<Event>();

            foreach(ICollectable c in this)
            {
                if(c is Event && c is not TimeEvent)
                {
                    Event ev = (Event)c;
                    events.Add(ev);

                    foreach (Event e in ev.Collection.GetEvents())
                        events.Add(e);
                }
            }

            return events;
        }*/


        #endregion Methods
    }
}
