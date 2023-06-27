using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// Utilities for dealing with files and file metadata.
    /// </summary>
    public class FileUtils
    {
        /// <summary>
        /// Returns a DateTime object that contains the timestamp described in the given filename,
        /// or null if the format is incorrect. The format should be "yyyymmdd_hhmmss"
        /// </summary>
        /// <param name="fn">The filename to extract the timestamp from</param>
        /// <returns>The Timestamp described in the filename, or null if the filename format is incorrect.</returns>
        public static DateTime? GetTimestampFromFilename(string fn)
        {
            DateTime? dt = null;

            if(fn.Length >= 15)
            {
                string datestr = fn.Substring(0, 8);
                string timestr = fn.Substring(9, 6);
                int date;
                int time;
                try
                {
                    date = int.Parse(datestr);
                    time = int.Parse(timestr);
                    if (fn[8] == '_')
                    {
                        int year = int.Parse(datestr.Substring(0, 4));
                        int month = int.Parse(datestr.Substring(4, 2));
                        int day = int.Parse(datestr.Substring(6, 2));
                        int hour = int.Parse(timestr.Substring(0, 2));
                        int min = int.Parse(timestr.Substring(2, 2));
                        int sec = int.Parse(timestr.Substring(4, 2));

                        dt = new DateTime(year, month, day, hour, min, sec);
                    }
                }
                catch { }
            }

            return dt;
        }
    }
}
