using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
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
        /// <summary>
        /// The list of Locations that make up the path
        /// </summary>
        public LocationCollection Locations { 
            get { return _locations; } 
            set {
                if(_locations != null)
                    _locations.CollectionChanged -= _locations_CollectionChanged;
                _locations = value;
                if(_locations != null)
                    _locations.CollectionChanged += _locations_CollectionChanged;

                RebuildZoomLevels();
                ResetBounds();
            }
        }

        // Store specific properties from Location so only those are serialized
        [DataMember(Name=nameof(Locations))]
        private LocationCollectionSerializable _pointsSerializable
        {
            get { return new LocationCollectionSerializable(Locations); }
            set { Locations = value.GetLocationCollection(); }
        }





        private List<LocationCollection> _locationsByZoom = new List<LocationCollection>();
        /// <summary>
        /// A list of paths, one for each possible zoom level. At lower zoom
        /// levels, the paths are simpler because less detail is visible. Use
        /// these for better map performance.
        /// </summary>
        public List<LocationCollection> LocationsByZoom
        {
            get { 
                if(_locationsByZoom == null)
                    RebuildZoomLevels();

                return _locationsByZoom; 
            }
        }




        private void _locations_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                    {
                        Trace.WriteLine("Error: MapPath - NewItems in CollChanged null");
                        break;
                    }

                    Bounds_Add(e.NewItems);

                    break;

                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems == null)
                    {
                        Trace.WriteLine("Error: MapPath - OldItems in CollChanged null");
                        break;
                    }
                    
                    // If any of the points were on the bounding box, then have to reset the bounds
                    foreach(Location l in e.OldItems)
                    {
                        if(l.Latitude == BoundingBox.Bottom || l.Latitude == BoundingBox.Top || l.Longitude == BoundingBox.Left || l.Longitude == BoundingBox.Right)
                        {
                            ResetBounds();
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    ResetBounds();
                    break;
            }

            RebuildZoomLevels();
        }



        #region Bounding Box

        private RectangleD _boundingBox;
        /// <summary>
        /// A rectangular bounding box that fits around the path. Used to detect
        /// when the path is in view.
        /// </summary>
        public RectangleD BoundingBox
        {
            get { return _boundingBox; }
            set
            {
                _boundingBox = value;
                OnPropertyChanged();
            }
        }


        /**
         * Calculate a bounding box to fit around the whole path
         */
        private void ResetBounds()
        {
            double top = double.NegativeInfinity;
            double bottom = double.PositiveInfinity;
            double left = double.PositiveInfinity;
            double right = double.NegativeInfinity;

            foreach(Location l in Locations)
            {
                if (l.Latitude < bottom)
                    bottom = l.Latitude;

                if (l.Latitude > top)
                    top = l.Latitude;

                if (l.Longitude < left)
                    left = l.Longitude;

                //TODO How to handle date line?
                if (l.Longitude > right)
                    right = l.Longitude;
            }

            BoundingBox = new RectangleD(bottom, left, top, right);
        }


        /**
         * Call when new locations have been added to the path
         */
        private void Bounds_Add(IList newItems)
        {
            bool change = false;

            foreach(Location l in newItems)
            {
                // If any of the new points changes the bounding box
                if (l.Latitude < BoundingBox.Bottom)
                {
                    BoundingBox.Bottom = l.Latitude;
                    change = true;
                }

                if (l.Latitude > BoundingBox.Top)
                {
                    BoundingBox.Top = l.Latitude;
                    change = true;
                }

                if (l.Longitude < BoundingBox.Left)
                {
                    BoundingBox.Left = l.Longitude;
                    change = true;
                }

                if (l.Longitude > BoundingBox.Right)
                {
                    BoundingBox.Right = l.Longitude;
                    change = true;
                }
            }

            // Set the property instead of just its fields so that change
            // notifications go out
            if (change)
                BoundingBox = new RectangleD(BoundingBox);
        }

        #endregion Bounding Box



        /**
         * Creates a simpler version of the path for each possible zoom level.
         * Zoom level 1 is very zoomed out, zoom level 21 is very zoomed in.
         * 
         * To do this, figures out the approximate lat/long that a single pixel
         * covers, and then merge whatever points fall within that pixel.
         */
        public void RebuildZoomLevels()
        {
            List<LocationCollection> newList = new List<LocationCollection>(21);

            // Create a simplified line for each zoom level.
            for(int zoom = 1; zoom < 22; zoom++)
            {
                // If no data, just have a bunch of empty paths
                if(Locations == null || Locations.Count == 0)
                {
                    newList.Add(new LocationCollection());
                    continue;
                }


                // Any zoom level above 18, use the entire path
                if (zoom > 18)
                {
                    newList.Add(Locations);
                    continue;
                }


                LocationCollection coll = new LocationCollection();

                // Pixel height & width of the map
                double mapHeight = 256 * Math.Pow(2, zoom);

                // How much latitude/longitude is covered by a single pixel
                double latPerPix = (85.05 * 2)/mapHeight;
                // Convering more points makes things quicker & doesn't seem to noticeably affect quality
                latPerPix *= 4;



                double currSquareLat = -1;
                double currSquareLon = -1;

                // Keep a list of all the points in the current pixel, & then merge them into one
                List<Location> currPixelList = new List<Location>();

                // Always add the first point
                coll.Add(Locations[0]);

                int i;
                for(i = 1; i < Locations.Count-1; i++)
                {
                    // If we haven't selected a current pixel, do that
                    if(currSquareLat == -1 || currSquareLon == -1)
                    {
                        // From the current Location, find the lower
                        // (magnitude) bounds in lat/long of the pixel that
                        // contains it
                        int num = (int) (Locations[i].Latitude / latPerPix);
                        currSquareLat = latPerPix * num;
                        num = (int)(Locations[i].Longitude / latPerPix);
                        currSquareLon = latPerPix * num;

                        currPixelList.Add(Locations[i]);
                        continue;
                    }

                    // Some funkiness with the pixel area when negative. This
                    // is because truncating the int makes it larger when
                    // negative.
                    int latMult = 1;
                    int lonMult = 1;
                    if (currSquareLat < 0)
                        latMult = -1;
                    if(currSquareLon < 0)
                        lonMult = -1;


                    bool match = true;

                    // Does the current point fit in the bounds of the current pixel
                    if(currSquareLat < 0)
                    {
                        if (!(Locations[i].Latitude <= currSquareLat && Locations[i].Latitude > currSquareLat + (latPerPix * latMult)))
                            match = false;
                    }
                    else
                    {
                        if (!(Locations[i].Latitude >= currSquareLat && Locations[i].Latitude < currSquareLat + (latPerPix * latMult)))
                            match = false;
                    }

                    if(currSquareLon < 0)
                    {
                        if (!(Locations[i].Longitude <= currSquareLon && Locations[i].Longitude > currSquareLon + (latPerPix * lonMult)))
                            match = false;
                    }
                    else
                    {
                        if (!(Locations[i].Longitude >= currSquareLon && Locations[i].Longitude < currSquareLon + (latPerPix * lonMult)))
                            match = false;
                    }

                    // If the current location fits within the current pixel, add it to the list
                    if(match)
                    {
                        currPixelList.Add(Locations[i]);
                    }
                    // Otherwise, merge the points in the current pixel, and move onto the next pixel
                    else
                    {
                        currSquareLat = -1;
                        currSquareLon = -1;
                        // Decrement i so that the same point is looked at next
                        // time (except that the new pixel will be chosen from
                        // it)
                        i--;

                        // Currently, just pick the first pixel in the list.
                        // Later, this should probably be chosen to avoid a
                        // super sharp angle, if possible
                        coll.Add(currPixelList[0]);
                        currPixelList.Clear();
                    }
                }

                // Always add the last point
                coll.Add(Locations[Locations.Count-1]);

                newList.Add(coll);
            }

            _locationsByZoom = newList;
        }




        #region Static

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

        #endregion Static
    }
}
