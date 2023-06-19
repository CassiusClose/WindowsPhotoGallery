using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public class MediaCollection : ObservableCollection<Media>
    {
        #region Constructors

        /**
         * XML de-serialization requires a default constructor
         */
        private MediaCollection() : this("Gallery") { }

        /// <summary>
        /// Creates a MediaGallery object with the given name.
        /// </summary>
        /// <param name="name">The name of the collection.</param>
        public MediaCollection(string name)
        {
            Name = name;
            Tags = new RangeObservableCollection<string>();
            DisableTagUpdate = false;
        }

        #endregion Constructors


        #region Fields and Properties

        /// <summary>
        /// The collection's name.
        /// </summary>
        public string Name;

        /// <summary>
        /// A collection of all the tags present in the collection (compiled from the tags of each image). Tags are not
        /// added here, they are added to the media within the gallery, and those changes are reflected here.
        /// </summary>
        public RangeObservableCollection<string> Tags { get; private set; }

        public delegate void CallbackMediaTagsChanged();
        /// <summary>
        /// An event triggered when the list of all tags in the collection changes.
        /// </summary>
        public event CallbackMediaTagsChanged MediaTagsChanged;

        //TODO Try with RangeObservableCollection
        public bool DisableTagUpdate { get; set; }

        #endregion Fields and Properties


        #region Methods


        /// <summary>
        /// Compiles the list of all tags (Tags) from the images in the collection.
        /// </summary>
        public void UpdateTags()
        {
            ObservableCollection<string> newTags = new ObservableCollection<string>();
            ObservableCollection<string> allTags = new ObservableCollection<string>();

            foreach (Media p in Items)
            {
                // For each tag in the photo
                foreach (string tag in p.Tags)
                {
                    if (!allTags.Contains(tag))
                        allTags.Add(tag);

                    if (!newTags.Contains(tag) && !Tags.Contains(tag))
                        newTags.Add(tag);
                }
            }

            ObservableCollection<string> removeTags = new ObservableCollection<string>();
            for(int i = 0; i < Tags.Count; i++)
            {
                if (!allTags.Contains(Tags[i]))
                    removeTags.Add(Tags[i]);
            }

            Tags.RemoveRange(removeTags);
            Tags.AddRange(newTags);

            if(MediaTagsChanged != null)
                MediaTagsChanged();
        }


        /**
         * Adds a media item to the collection. 
         */
        protected override void InsertItem(int index, Media item)
        {
            // InsertItem is used internally by functions such as Add, so overriding here is enough
            base.InsertItem(index, item);

            // Add handler for when the photo's tags change, so we can update this collections's master list of tags
            item.Tags.CollectionChanged += MediaTags_CollectionChanged;

            // When a photo is added, need to refresh the list of tags
            UpdateTags();
        }


        /**
         * ObservableCollection event handler: When a photo's tags update, need to refresh the list of tags in the collection 
         */
        private void MediaTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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



        #region Static


        /// <summary>
        /// De-serializes and creates a PhotoGallery instance from the given XML file
        /// </summary>
        /// <param name="filename">The XMl file's name</param>
        /// <returns>The constructed PhotoGallery object.</returns>
        public static MediaCollection LoadGallery(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MediaCollection));
            FileStream fs = new FileStream(filename, FileMode.Open);

            return (MediaCollection)serializer.Deserialize(fs);
        }


        #endregion Static
    }
}
