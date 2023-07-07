using PhotoGalleryApp.Utils;
using System;
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
            DisableTagUpdate = false;
        }

        #endregion Constructors


        #region Fields and Properties

        /// <summary>
        /// A collection of all the tags present in the collection (compiled from the tags of each image). Tags are not
        /// added here, they are added to the media within the gallery, and those changes are reflected here.
        /// </summary>
        public RangeObservableCollection<string> Tags { get; private set; }

        public delegate void CallbackMediaTagsChanged();
        /// <summary>
        /// An event triggered when any media item's tags have changed.
        /// </summary>
        public event CallbackMediaTagsChanged? MediaTagsChanged;

        /// <summary>
        /// When this is true, the MediaTagsChanged event will not be triggered. Sometimes outside classes may want to change
        /// the tags on several media items at once and receive a single notification for it, so they can use this for that.
        /// </summary>
        public bool DisableTagUpdate { get; set; }

        #endregion Fields and Properties


        #region Methods


        /// <summary>
        /// Compiles the list of all tags (Tags) from the images in the collection.
        /// </summary>
        public void UpdateTags()
        {
            //TODO Could be more efficient, by only updating based on what has changed, either from add, remove, etc.

            ObservableCollection<string> newTags = new ObservableCollection<string>();
            ObservableCollection<string> allTags = new ObservableCollection<string>();

            // Compile list of all tags from media, and list of tags that weren't previously in the list here
            foreach(ICollectable i in Items)
            {
                if(i is Media)
                {
                    Media m = (Media)i;
                    foreach (string tag in m.Tags)
                    {
                        if (!allTags.Contains(tag))
                            allTags.Add(tag);

                        if (!newTags.Contains(tag) && !Tags.Contains(tag))
                            newTags.Add(tag);
                    }
                }
                else
                {
                    MediaCollection coll = ((Event)i).Collection;
                    foreach (string tag in coll.Tags)
                    {
                        if (!allTags.Contains(tag))
                            allTags.Add(tag);

                        if (!newTags.Contains(tag) && !Tags.Contains(tag))
                            newTags.Add(tag);
                    }
                }
            }

            // Compile list of tags that were in the list here but now don't exist
            ObservableCollection<string> removeTags = new ObservableCollection<string>();
            for(int i = 0; i < Tags.Count; i++)
            {
                if (!allTags.Contains(Tags[i]))
                    removeTags.Add(Tags[i]);
            }

            Tags.RemoveRange(removeTags);
            Tags.AddRange(newTags);

            // Trigger update, even if the collection-wide tag list might not have changed. It's possible that some media
            // gained or lost tags without the collection-wide list changing.
            if(MediaTagsChanged != null)
                MediaTagsChanged();
        }


        /**
         * Adds a media item to the collection. 
         */
        protected override void InsertItem(int index, ICollectable item)
        {
            // InsertItem is used internally by functions such as Add, so overriding here is enough
            base.InsertItem(index, item);

            AddTagsChangedListener(MediaTags_CollectionChanged);

            // When a photo is added, need to refresh the list of tags
            UpdateTags();
        }

        /*
         * Recursively add a CollectionChanged listener to all items in this collection, and all
         * items in any collections (Events) that may be nested in this collection.
         */
        private void AddTagsChangedListener(NotifyCollectionChangedEventHandler handler)
        {
            foreach(ICollectable i in Items)
            {
                if(i is Media)
                {
                    ((Media)i).Tags.CollectionChanged += handler;
                }
                else
                {
                    ((Event)i).Collection.AddTagsChangedListener(handler);
                }
            }
        }


        /**
         * ObservableCollection event handler: When a media items's tags update, need to refresh the list of tags in the collection 
         */
        private void MediaTags_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!DisableTagUpdate)
                UpdateTags();
        }

        
        /**
         * Removes a media item from the collection 
         */
        protected override void RemoveItem(int index)
        {
            //RemoveItem is used internally by functions such as Remove, so overriding here is enough
            base.RemoveItem(index);
            
            // When a photo is removed, need to refresh the list of tags 
            UpdateTags();
        }


        #endregion Methods
    }
}
