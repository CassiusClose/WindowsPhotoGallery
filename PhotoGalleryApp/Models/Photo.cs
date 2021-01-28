using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Represents one image, stored somewhere on disk, associated with a list of tags.
    /// </summary>
    class Photo
    {
        public Photo(string path, ObservableCollection<string> tags)
        {
            Path = path;

            if (tags == null)
                Tags = new ObservableCollection<string>();
            else
                Tags = tags;
        }

        public Photo(string path) : this(path, null) { }


        /// <summary>
        /// The filepath to the image.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// A collection of tags associated with the image, used for easier sorting & filtering of images.
        /// </summary>
        public ObservableCollection<string> Tags { get; private set; }
    }
}
