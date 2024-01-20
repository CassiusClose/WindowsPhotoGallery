using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Contains a user's entire session, including all of their media, events, locations, etc.
    /// </summary>
    // This class stores MediaCollection, which is a collection of an abstract type, so have to
    // include all possible subclasses to serialize.
    [DataContract]
    public class Gallery
    {
        public Gallery() : this("Gallery", null) { }

        public Gallery(string name, MediaCollection? media) 
        {
            _name = name;

            if (media != null)
                _media = media;
            else
                _media = new MediaCollection();
        }


        private string _name;
        /// <summary>
        /// The display name of the gallery. 
        /// </summary>
        //TODO Does this really have a place? Should be some user identifier maybe if there are multiple users?
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private MediaCollection _media;
        /// <summary>
        /// A collection of all the media in the gallery
        /// </summary>
        [DataMember]
        public MediaCollection Collection { 
            get { return _media; } 
            set { _media = value; }
        }
    }
}
