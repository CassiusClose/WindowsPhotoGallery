using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Contains a user's entire session, including all of their media, events, locations, etc.
    /// </summary>
    // This class stores MediaCollection, which is a collection of an abstract type, so have to
    // include all possible subclasses to serialize.
    [XmlInclude(typeof(Image))]
    [XmlInclude(typeof(Video))]
    [XmlInclude(typeof(Event))]
    public class Gallery
    {
        public Gallery() : this("Gallery", null) { }

        public Gallery(string name, MediaCollection? media) 
        {
            _name = name;

            if(media != null)
                _media = media;

            _events = new ObservableCollection<Event>();
        }


        private string _name;
        /// <summary>
        /// The display name of the gallery. 
        /// </summary>
        //TODO Does this really have a place? Should be some user identifier maybe if there are multiple users?
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private MediaCollection _media;
        /// <summary>
        /// A collection of all the media in the gallery
        /// </summary>
        public MediaCollection MediaList { 
            get { return _media; } 
            set { _media = value; }
        }


        private ObservableCollection<Event> _events;
        /// <summary>
        /// A list of all the events associated with the media in the gallery
        /// </summary>
        public ObservableCollection<Event> Events { 
            get { return _events; } 
            set { _events = value; }
        }


        #region Static


        /// <summary>
        /// De-serializes and creates a Gallery instance from the given XML file
        /// </summary>
        /// <param name="filename">The XMl file's name</param>
        /// <returns>The constructed PhotoGallery object.</returns>
        public static Gallery? LoadGallery(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Gallery));
            FileStream fs = new FileStream(filename, FileMode.Open);
            return (Gallery?)serializer.Deserialize(fs);
        }


        #endregion Static
    }
}
