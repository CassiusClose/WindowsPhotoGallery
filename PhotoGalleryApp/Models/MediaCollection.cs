using PhotoGalleryApp.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        #endregion Constructors


        #region Fields and Properties

        /// <summary>
        /// A collection of all the tags present in the collection (compiled from the tags of each image). Tags are not
        /// added here, they are added to the media within the gallery, and those changes are reflected here.
        /// </summary>
        public RangeObservableCollection<string> Tags { get; private set; }

        /**
         * Pass on any children's tags CollectionChanged events.
         */
        [XmlIgnore]
        public NotifyCollectionChangedEventHandler MediaTagsChanged; 

        #endregion Fields and Properties


        #region Methods




        /**
         * Adds a media item to the collection. 
         */
        protected override void InsertItem(int index, ICollectable item)
        {
            // InsertItem is used internally by functions such as Add, so overriding here is enough
            base.InsertItem(index, item);

            if (item is Media)
                ((Media)item).TagsChanged += MediaTags_CollectionChanged;
            // Uncomment if a MediaCollection's tag list should contain tags from nested events
            /*else
                ((Event)i).Collection.AddTagsChangedListener(handler);
            */


            // When a photo is added, need to refresh the list of tags
            if (item is Media)
                MediaTagsChanged_Add(((Media)item).Tags);
            // Uncomment if a MediaCollection's tag list should contain tags from nested events
            /*else
                MediaTagsChanged_Add(((Event)item).Collection.Tags);
            */
        }


        /**
         * Removes a media item from the collection 
         */
        protected override void RemoveItem(int index)
        {
            ICollectable item = this[index];

            //RemoveItem is used internally by functions such as Remove, so overriding here is enough
            base.RemoveItem(index);
            
            // When a photo is removed, need to refresh the list of tags 
            if (item is Media)
                MediaTagsChanged_Remove(((Media)item).Tags);
            // Uncomment if a MediaCollection's tag list should contain tags from nested events
            /*else
                MediaTagsChanged_Remove(((Event)item).Collection.Tags);
            */
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
