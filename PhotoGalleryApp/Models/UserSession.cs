using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Stores all data for one user. This includes all media and map items.
    /// </summary>
    [DataContract]
    public class UserSession
    {
        /**
         * sessionFile: The filepath at which to save the session data
         */
        public UserSession(string sessionFile) 
        {
            _sessionFile = sessionFile;
            Gallery = new Gallery();
            Map = new Map();
        }


        private string _sessionFile;


        private Map _map;

        [DataMember(Order = 0)]
        public Map Map
        {
            get { return _map; }
            set
            {
                if(_map != null)
                    _map.CollectionChanged -= Map_CollectionChanged;

                _map = value;

                if(_map != null)
                    _map.CollectionChanged += Map_CollectionChanged;
            }
        }

        [DataMember(Order = 1)]
        public Gallery Gallery;


        /// <summary>
        /// Saves the current state of the session into the associated XML file
        /// </summary>
        public void SaveSession()
        {
            FileStream fs = new FileStream(_sessionFile, FileMode.Create);
            DataContractSerializer serializer = new DataContractSerializer(typeof(UserSession));
            serializer.WriteObject(fs, this);
            fs.Close();
        }


        /// <summary>
        /// Attempts to load and return a session from the given filepath. If
        /// the file does not exist, creates a new session and returns that.
        /// If there are errors in XML deserialization, throws an exception.
        /// </summary>
        /// <param name="sessionFile">The file from which to load session data</param>
        /// <returns>The session stored in the given file, or a new session if the file does not exist.</returns>
        public static UserSession LoadSession(string sessionFile)
        {
            if(!File.Exists(sessionFile))
            {
                return new UserSession(sessionFile);
            }

            FileStream fs = new FileStream(sessionFile, FileMode.Open);
            DataContractSerializer serializer = new DataContractSerializer(typeof(UserSession));
            UserSession? session = (UserSession?)serializer.ReadObject(fs);
            if (session == null)
                throw new Exception("Couldn't deserialize");

            session._sessionFile = sessionFile;
            return session;
        }



        /**
         * When a MapItem is removed from the Map, remove it from any Media that might belong to it
         */
        private void Map_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems == null)
                    throw new ArgumentException();

                foreach(MapItem item in e.OldItems)
                {
                    foreach(ICollectable coll in Gallery.Collection)
                    {
                        if(coll is Media)
                        {
                            Media m = (Media)coll;
                            if (m.MapItem == item)
                                m.MapItem = null;
                        }
                    }
                }
            }
        }

    }
}
