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
    /// A collection of Media objects, can be both photo and video.
    /// </summary>
    public class MediaGallery : ObservableCollection<Media>
    {
        #region Constructors

        /**
         * XML de-serialization requires a default constructor
         */
        private MediaGallery() : this("Gallery") { }

        /// <summary>
        /// Creates a MediaGallery object with the given name.
        /// </summary>
        /// <param name="name">The name of the gallery.</param>
        public MediaGallery(string name)
        {
            Name = name;
            Tags = new RangeObservableCollection<string>();
            DisableTagUpdate = false;
        }

        #endregion Constructors


        #region Fields and Properties

        /// <summary>
        /// The gallery's name.
        /// </summary>
        public string Name;

        /// <summary>
        /// A collection of all the tags present in the gallery (compiled from the tags of each image). Tags are not
        /// added here, they are added to the media within the gallery, and those changes are reflected here.
        /// </summary>
        public RangeObservableCollection<string> Tags { get; private set; }

        public event CallbackMediaTagsChanged MediaTagsChanged;

        public bool DisableTagUpdate { get; set; }

        #endregion Fields and Properties


        #region Methods

        /**
         * Compiles the list of all tags from the images in the gallery.
         */
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
         * Adds a media item to the gallery
         */
        protected override void InsertItem(int index, Media item)
        {
            // InsertItem is used internally by functions such as Add, so overriding here is enough
            base.InsertItem(index, item);

            // Add handler for when the photo's tags change, so we can update this gallery's master list of tags
            item.Tags.CollectionChanged += MediaTags_CollectionChanged;

            // When a photo is added, need to refresh the list of tags
            UpdateTags();
        }


        /**
         * ObservableCollection event handler: When a photo's tags update, need to refresh the list of tags in the gallery
         */
        private void MediaTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("MediaTags_CollectionChanged");
            if (!DisableTagUpdate)
                UpdateTags();
        }


        /**
         * Removes a media item from the gallery
         */
        protected override void RemoveItem(int index)
        {
            //RemoveItem is used internally by functions such as Remove, so overriding here is enough
            base.RemoveItem(index);
            
            // When a photo is removed, need to refresh the list of tags 
            UpdateTags();
        }


        #endregion Methods


        public delegate void CallbackMediaTagsChanged();


        #region Static


        /// <summary>
        /// De-serializes and creates a PhotoGallery instance from the given XML file
        /// </summary>
        /// <param name="filename">The XMl file's name</param>
        /// <returns>The constructed PhotoGallery object.</returns>
        public static MediaGallery LoadGallery(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MediaGallery));
            FileStream fs = new FileStream(filename, FileMode.Open);

            return (MediaGallery)serializer.Deserialize(fs);
        }


        #endregion Static
    }
}
