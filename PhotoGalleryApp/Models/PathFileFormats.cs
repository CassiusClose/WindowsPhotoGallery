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
        public static List<MapPath> LoadFromTxtFile(string filename)
        {
            Dictionary<int, MapPath> tracks = new Dictionary<int, MapPath>();

            using (StreamReader reader = new StreamReader(filename))
            {
                string? currentLine;

                int id;
                bool foundTracks = false;

                // Read definitions of each track
                while((currentLine = reader.ReadLine()) != null) 
                { 
                    if(!foundTracks)
                    {
                        if(currentLine == "trk")
                        {
                            reader.ReadLine();
                            foundTracks = true;
                            continue;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(currentLine))
                            break;

                        string[] items = currentLine.Split('\t', 8);
                        if (items.Length != 8)
                            throw new FormatException();

                        id = int.Parse(items[0]);
                        string name = items[1];
                        tracks[id] = new MapPath(name);
                    }
                }

                reader.DiscardBufferedData();
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                bool foundPoints = false;
                double lat;
                double lon;

                // Read points
                while ((currentLine = reader.ReadLine()) != null)
                {
                    if (!foundPoints)
                    {
                        if (currentLine == "trkpt")
                        {
                            reader.ReadLine();
                            foundPoints = true;
                            continue;
                        }
                    }
                    else
                    {
                        string[] coords = currentLine.Split('\t', 5);
                        if(coords.Length != 5)
                            throw new FormatException();                            

                        id = int.Parse(coords[1]);
                        lat = double.Parse(coords[2]);
                        lon = double.Parse(coords[3]);

                        Location l = new Location(lat, lon);
                        tracks[id].Locations.Add(l);
                    }
                }
            }

            return tracks.Values.ToList<MapPath>();
        }
    }
}
