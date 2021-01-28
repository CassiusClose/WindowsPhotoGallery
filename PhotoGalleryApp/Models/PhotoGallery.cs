using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// A collection of Photos.
    /// </summary>
    class PhotoGallery : ObservableCollection<Photo>
    {
        public PhotoGallery(string name)
        {
            Name = name;
            Tags = new ObservableCollection<string>();
        }

        /// <summary>
        /// The gallery's name.
        /// </summary>
        public string Name;

        /// <summary>
        /// A collection of all the tags present in the gallery (compiled from the tags of each image).
        /// </summary>
        public ObservableCollection<string> Tags { get; private set; }


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
                    bool found = false;
                    // Has the tag already been added to our compiled list
                    foreach (string t in tags)
                    {
                        if (t == tag)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        tags.Add(tag);
                }
            }

            Tags = tags;
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

    }
}
