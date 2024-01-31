using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Information about some movement on the map. Stores an ordered set of locations.
    /// </summary>
    [DataContract]
    public class MapPath : MapItem
    {
        public MapPath(string name) : base(name)
        {
            _locations = new LocationCollection();
        }


        private LocationCollection _locations = new LocationCollection();
        public LocationCollection Locations { 
            get { return _locations; } 
            set { _locations = value; }
        }


        // Store specific properties from Location so only those are serialized
        [DataMember(Name=nameof(Locations))]
        private LocationCollectionSerializable _pointsSerializable
        {
            get { return new LocationCollectionSerializable(Locations); }
            set { Locations = value.GetLocationCollection(); }
        }




        /// <summary>
        /// Merges the two given paths into a third MapPath and returns it. The
        /// merged path has the given name.
        /// 
        /// To merge the paths, the algorithm finds the first section of
        /// matching points. That section goes into the merged path. The
        /// algorithm looks at the points in each path before the matched
        /// section, and whichever has more points, uses those points. It
        /// does the same thing for the points after the matched section.
        /// 
        /// A better algorithm probably would handle multiple sections of
        /// matching points, but this is good enough for now.
        /// </summary>
        /// <param name="p1">A path to merge</param>
        /// <param name="p2">A path to merge</param>
        /// <param name="newName">The name of the merged path</param>
        /// <returns>The merged path</returns>
        public static MapPath MergeOverlappingPaths(MapPath p1, MapPath p2, string newName)
        {
            int startInd1 = 0;
            int startInd2 = 0;
            bool found = false;

            int i, j;
            // Find the first matching point
            for (i = 0; i < p1.Locations.Count; i++)
            {
                for (j = 0; j < p2.Locations.Count; j++)
                {
                    if (Equals(p1.Locations[i], p2.Locations[j]))
                    {
                        startInd1 = i;
                        startInd2 = j;
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }

            if (!found)
                throw new ArgumentException("The two paths do not overlap");


            int endInd1 = startInd1+1;
            int endInd2 = startInd1+2;


            i = startInd1+1;
            j = startInd2+1;

            // Find the last matching point in the section (look for the first consecutive non-matching point)
            while(i < p1.Locations.Count && j < p2.Locations.Count)
            {
                if (Equals(p1.Locations[i], p2.Locations[j]))
                {
                    endInd1 = i+1; 
                    endInd2 = j+1;
                }
                else
                    break;

                i++; j++;
            }


            MapPath p = new MapPath(newName);

            // Copy points before the matched section, from whichever path has more points before it
            if(startInd1 > startInd2)
            {
                for(i = 0; i < startInd1; i++)
                    p.Locations.Add(p1.Locations[i]);
            }
            else
            {
                for(j = 0; j < startInd2; j++)
                    p.Locations.Add(p2.Locations[j]);
            }


            // Copy over the matched section
            for (i = startInd1; i < endInd1; i++)
                p.Locations.Add(p1.Locations[i]);

            
            // Copy points after the matched section, from whichever path has more points after it
            if((p1.Locations.Count - endInd1) > (p2.Locations.Count - endInd2))
            {
                for(i = endInd1; i < p1.Locations.Count; i++)
                    p.Locations.Add(p1.Locations[i]);
            }
            else
            {
                for(j = endInd2; j < p2.Locations.Count; j++)
                    p.Locations.Add(p2.Locations[j]);
            }

            return p;
        }
    }
}
