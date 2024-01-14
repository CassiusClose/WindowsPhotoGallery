using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /**
     * Methods to load in map path information from files
     */
    public class PathFileFormats
    {
        /**
         * A text file exported by Garmin Homeport
         */
        public static LocationCollection LoadFromTxtFile(string filename)
        {
            LocationCollection coll = new LocationCollection();
            using (StreamReader reader = new StreamReader(filename))
            {
                string? currentLine;
                bool startedPoints = false;

                while((currentLine = reader.ReadLine()) != null) 
                { 
                    if(!startedPoints)
                    {
                        if(currentLine.StartsWith("trkpt"))
                        {
                            reader.ReadLine();
                            startedPoints = true;
                            continue;
                        }
                    }
                    else
                    {
                        double lat;
                        double lon;
                        string[] coords = currentLine.Split('\t', 5);
                        if(coords.Length != 5)
                            throw new FormatException();                            

                        lat = double.Parse(coords[2]);
                        lon = double.Parse(coords[3]);

                        Location l = new Location(lat, lon);
                        coll.Add(l);
                    }
                }
            }

            return coll;
        }
    }
}
