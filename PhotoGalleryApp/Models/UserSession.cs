using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Stores all data for one user. This includes all media and map items.
    /// </summary>
    public class UserSession
    {
        // Parameterless constructor for the XML Deserializer
        private UserSession() {}

        /**
         * sessionFile: The filepath at which to save the session data
         */
        public UserSession(string sessionFile) 
        {
            _sessionFile = sessionFile;
            Gallery = new Gallery();
            Map = new Map();
        }


        [XmlIgnore]
        private string _sessionFile;


        public Gallery Gallery;
        public Map Map;


        /// <summary>
        /// Saves the current state of the session into the associated XML file
        /// </summary>
        public void SaveSession()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserSession));
            TextWriter writer = new StreamWriter(_sessionFile);
            serializer.Serialize(writer, this);
            writer.Close();
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
            XmlSerializer serializer = new XmlSerializer(typeof(UserSession));
            FileStream fs;
            try
            {
                fs = new FileStream(sessionFile, FileMode.Open);
            }
            catch(FileNotFoundException)
            {
                Trace.WriteLine("No session file found. Creating new one");
                UserSession s = new UserSession(sessionFile);
                return s;
            }

            UserSession? session = (UserSession?)serializer.Deserialize(fs);
            //TODO Throw errors? If file exists, change filename so that corrupted one isn't overwritten
            if(session == null)
            {
                Trace.WriteLine("ERROR IN DESERIALIZATION");
                System.Windows.Application.Current.Shutdown();
                return new UserSession();
            }

            session._sessionFile = sessionFile;

            return session;
        }
    }
}
