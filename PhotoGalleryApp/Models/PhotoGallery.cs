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
    /// A collection of Photos.
    /// </summary>
    public class PhotoGallery : ObservableCollection<Photo>
    {
        #region Constructors

        /*
         * Used for XMl de-serialization
         */
        private PhotoGallery() : this("Gallery") { }

        public PhotoGallery(string name)
        {
            Name = name;
            Tags = new RangeObservableCollection<string>();
        }

        #endregion Constructors


        #region Members

        /// <summary>
        /// The gallery's name.
        /// </summary>
        public string Name;

        /// <summary>
        /// A collection of all the tags present in the gallery (compiled from the tags of each image).
        /// </summary>
        public RangeObservableCollection<string> Tags { get; private set; }

        #endregion Members


        #region Methods

        /*
         * Compiles the list of all tags from the images in the gallery.
         */
        private void UpdateTags()
        {
            ObservableCollection<string> tags = new ObservableCollection<string>();

            foreach (Photo p in Items)
            {
                // For each tag in the photo
                foreach (string tag in p.Tags)
                {
                    if (!tags.Contains(tag))
                        tags.Add(tag);
                }
            }

            // Update the tags list once with one notification to change listeners
            Tags.ReplaceWith(tags);
        }


        protected override void InsertItem(int index, Photo item)
        {
            // InsertItem is used internally by functions such as Add, so overriding here is enough
            base.InsertItem(index, item);

            // Add handler for when the photo's tags change
            item.Tags.CollectionChanged += PhotoTags_CollectionChanged;

            // When a photo is added, need to refresh the list of tags
            UpdateTags();
        }

        /*
         * ObservableCollection event handler: When a photo's tags update, need to refresh the list of tags in the gallery
         */
        private void PhotoTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateTags();
        }


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
        public static PhotoGallery LoadGallery(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PhotoGallery));
            FileStream fs = new FileStream(filename, FileMode.Open);

            return (PhotoGallery)serializer.Deserialize(fs);
        }


        #endregion Static
    }
}
