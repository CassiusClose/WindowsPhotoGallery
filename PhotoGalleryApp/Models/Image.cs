using Microsoft.VisualBasic;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// A Media object that refers to an image file.
    /// </summary>
    public class Image : Media
    {
        #region Constructors

        /// <summary>
        /// Creates an Image object with the given filepath.
        /// </summary>
        /// <param name="path">The filepath of the image.</param>
        public Image(string path) : base(path) { }

        /// <summary>
        /// Creates an Image object with the given path, with the given list of tags.
        /// </summary>
        /// <param name="path">The filepath of the image.</param>
        /// <param name="tags">A list of tags that the image should have.</param>
        public Image(string path, ObservableCollection<string> tags) : base(path, tags) { }

        /**
         * Needed by XmlSerializer
         */
        protected Image() : base() { }

        #endregion Constructors


        /**
         * Loads image metadata, including size and rotation.
         */
        protected override void LoadMetadata()
        {
            // Load image metadata
            FileStream fs = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
            BitmapFrame frame = BitmapFrame.Create(fs, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            BitmapMetadata meta = frame.Metadata as BitmapMetadata;

            // Image size
            _width = frame.PixelWidth;
            _height = frame.PixelHeight;


            // Process any rotation metadata
            Rotation = Rotation.Rotate0;
            if (meta != null && meta.ContainsQuery("System.Photo.Orientation"))
            {
                ushort rotAmount = (ushort)meta.GetQuery("System.Photo.Orientation");
                switch (rotAmount)
                {
                    case 6:
                        Rotation = Rotation.Rotate90;
                        break;
                    case 3:
                        Rotation = Rotation.Rotate180;
                        break;
                    case 8:
                        Rotation = Rotation.Rotate270;
                        break;
                }
            }

            LoadTimestamp(meta);

            fs.Close();
        }

        /*
         * Read in timestamp metadata and pick the best one. Look at FileCreated, FileModified,
         * and DateTaken (from exif metadata) if possible, and choose the earliest one. If the
         * filename is in the yyyymmdd_hhmmss.ext format, use that instead.
         */
        private void LoadTimestamp(BitmapMetadata meta)
        {
            //TODO Error handling
            DateTime? filenameDt = FileUtils.GetTimestampFromFilename(Path.GetFileNameWithoutExtension(Filepath));

            List<DateTime> dates = new List<DateTime>();
            dates.Add(File.GetCreationTime(Filepath));
            dates.Add(File.GetLastWriteTime(Filepath));

            if (meta.DateTaken != null)
            {
                dates.Add(DateTime.Parse(meta.DateTaken));
            }

            _timestamp = dates.Min<DateTime>();
            if (filenameDt != null)
            {
                if(_timestamp < filenameDt)
                {
                    Trace.WriteLine("Info: File has date metadata earlier than the date in its filename. Are you sure this file is named correctly? (" + Filepath + ")");
                }

                _timestamp = (DateTime)filenameDt;
            }
        }


        /*
         * Returns the media file type (image, video, etc.). This is used to determine
         * which Media subclass a Media instance belongs to.
         */
        protected override MediaFileType GetMediaType()
        {
            return MediaFileType.Image;
        }
    }
}
