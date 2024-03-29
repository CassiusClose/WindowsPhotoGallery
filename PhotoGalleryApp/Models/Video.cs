﻿using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// A Media object that refers to a video file and holds a Media object for its thumbnail
    /// </summary>
    [KnownType(typeof(ICollectable))]
    [KnownType(typeof(Media))]
    [DataContract]
    public class Video : Media
    {
        // The relative directory that video thumbnails should be stored in
        public const string THUMBNAIL_DIRECTORY = "thumbnails/";


        #region Constructors

        /// <summary>
        /// Creates an Image object with the given filepath.
        /// </summary>
        /// <param name="path">The filepath of the image.</param>
        public Video(string path) : base(path) { }

        /// <summary>
        /// Creates an Image object with the given path, with the given list of tags.
        /// </summary>
        /// <param name="path">The filepath of the image.</param>
        /// <param name="tags">A list of tags that the image should have.</param>
        public Video(string path, ObservableCollection<string> tags) : base(path, tags) { }

        #endregion Constructors


        #region Fields and Properties

        /// <summary>
        /// The Image that is the video's thumbnail
        /// </summary>
        public Image Thumbnail;

        #endregion Fields and Properties


        /**
         * Loads the video file's associated data.
         */
        protected override void LoadMetadata()
        {
            LoadDateTime();
        }

        /*
         * Read in timestamp metadata and pick the best one. Look at FileCreated and FileModified,
         * and choose the earliest one. If the filename is in the yyyymmdd_hhmmss.ext format, use that instead.
         */
        private void LoadDateTime()
        {
            //TODO Error handling
            DateTime? filenameDt = FileUtils.GetTimestampFromFilename(Path.GetFileNameWithoutExtension(Filepath));

            List<DateTime> dates = new List<DateTime>();
            dates.Add(File.GetCreationTime(Filepath));
            dates.Add(File.GetLastWriteTime(Filepath));

            DateTime ts = dates.Min<DateTime>();
            if (filenameDt != null)
            {
                if(ts < filenameDt)
                {
                    Trace.WriteLine("Info: File has date metadata earlier than the date in its filename. Are you sure this file is named correctly? (" + Filepath + ")");
                }

                ts = (DateTime)filenameDt;
            }
            // If this is ever changed to be a higher precision, some other methods are going to
            // need additional checks
            _timestamp = new PrecisionDateTime(ts, TimeRange.Second);
        }


        /// <summary>
        /// If the video's thumbnail has not been created/loaded as an Image object,
        /// then extract the video's thumbnail, save it to disk, and create an
        /// Image object for it.
        /// LoadThumbnail() is not called automatically. Users must call it manually on
        /// a Video object.
        /// </summary>
        public void LoadThumbnail()
        {
            if (Thumbnail != null && File.Exists(Thumbnail.Filepath))
                return;

            if (!File.Exists(Filepath))
                return;

            string[] files = Directory.GetFiles(THUMBNAIL_DIRECTORY);

            // The first try for the thumbnail is the video file's name as a jpg
            string _thumbnailName = Path.GetFileNameWithoutExtension(Filepath);
            string _thumbnailFile = Path.Combine(new string[] { THUMBNAIL_DIRECTORY, _thumbnailName + ".jpg" });
            int conflictCount = 0;

            // If that thumbnail name alreay exists, then add "_1", "_2", etc. until there is no conflict
            while (files.Contains<string>(_thumbnailFile))
            {
                conflictCount++;
                _thumbnailFile = Path.Combine(new string[] { THUMBNAIL_DIRECTORY, _thumbnailName + "_" + conflictCount + ".jpg" });
            }


            // Assemble the command & process to run that extract the thumbnail from the video
            string command = "ffmpeg -i " + Filepath + " -ss 00:00:00 -vframes 1 " + _thumbnailFile;

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c " + command;
            process.StartInfo = startInfo;

            // Run the process
            process.Start();
            process.WaitForExit();

            // If the process failed somehow, abort
            if (process.ExitCode != 0)
            {
                Console.WriteLine("Something went wrong creating a thumbnail: " + process.ExitCode);
                return;
            }

            // If everything succeeded, then create the Image object for the thumbnail.
            Thumbnail = new Image(_thumbnailFile);
        }


        /*
         * Returns the media file type (image, video, etc.). This is used to determine
         * which Media subclass a Media instance belongs to.
         */
        protected override MediaFileType GetMediaType()
        {
            return MediaFileType.Video;
        }
    }
}
