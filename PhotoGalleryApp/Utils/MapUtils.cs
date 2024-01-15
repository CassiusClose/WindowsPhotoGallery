using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A set of helpful geometry related functions.
    /// </summary>
    public class MapUtils
    {
        /// <summary>
        /// Given a path of line segments and a point p, finds the closest
        /// vertex of the path to point p.
        ///
        /// Locations are converted to viewport Points, and the calculations
        /// are done with those . The result is converted back to a Location.
        /// </summary>
        /// <param name="coll">The path of line segments. Should be type LocationCollection.</param>
        /// <param name="clickPos">The point p. Should be type Point.</param>
        /// <param name="mapContainer">The Map that the path belongs to. Used to convert between Locations and viewport Points.</param>
        /// <returns>The Location of the path vertex that is closest to point p.</returns>
        public static Location? GetClosestVertexOnPath(LocationCollection coll, Point clickPos, Map mapContainer) 
        { 
            double minDist = double.PositiveInfinity;
            int minInd = 0;
            // Find the closest path point
            for (int i = 0; i < coll.Count; i++)
            {
                Point pointLoc = mapContainer.LocationToViewportPoint(coll[i]);

                // If point outside the visible bounds, then don't need to display it
                if (pointLoc.X < 0 || pointLoc.Y < 0 || pointLoc.X > mapContainer.RenderSize.Width || pointLoc.Y > mapContainer.RenderSize.Height)
                    continue;

                double dist = MapUtils.Dist(clickPos, pointLoc);
                if (dist < minDist)
                {
                    minDist = dist;
                    minInd = i;
                }
            }

            if (minDist == double.PositiveInfinity)
                return null;

            return coll[minInd];
        }


        /// <summary>
        /// Given a path of line segments and a point p, finds the closest
        /// point on the path to point p.
        ///
        /// Locations are converted to viewport Points, and the calculations
        /// are done with those . The result is converted back to a Location.
        /// </summary>
        /// <param name="coll">The path of line segments. Should be type LocationCollection.</param>
        /// <param name="clickPos">The point p. Should be type Point.</param>
        /// <param name="mapContainer">The Map that the path belongs to. Used to convert between Locations and viewport Points.</param>
        /// <returns>The Location on the path that is closest to point p.</returns>
        public static Location GetClosestLocationOnPath(LocationCollection coll, Point clickPos, Map mapContainer)
        {
            Point minPoint;
            double minDist = double.PositiveInfinity;
            // For each segment, find the closest point. Keep track of the closest segment.
            for (int i = 0; i < coll.Count-1; i++)
            {
                Point a = mapContainer.LocationToViewportPoint(coll[i]);
                Point b = mapContainer.LocationToViewportPoint(coll[i + 1]);

                Point s = MapUtils.GetClosestPointOnSegment(a, b, clickPos);
                double dist = MapUtils.Dist(clickPos, s);
                if (dist < minDist)
                {
                    minDist = dist;
                    minPoint = s;
                }
            }
            return mapContainer.ViewportPointToLocation(minPoint);
        }


        /// <summary>
        /// Given a line segment (ab) and a point not on the line segment (p),
        /// returns the point on the line segment closest to point p.
        ///
        /// Algorithm taken from https://stackoverflow.com/questions/47481774/getting-point-on-line-segment-that-is-closest-to-another-point
        /// </summary>
        /// <param name="a">Point 1 of the line segment</param>
        /// <param name="b">Point 2 of the line segment</param>
        /// <param name="p">A point not on the line segement</param>
        /// <returns>The closest point on segement ab to point p.</returns>
        public static Point GetClosestPointOnSegment(Point a, Point b, Point p)
        {
            double lengthSqrAB = Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2);

            // Interpolation amount between a & b
            double t = (((p.X - a.X) * (b.X - a.X)) + ((p.Y - a.Y) * (b.Y - a.Y))) / lengthSqrAB;

            // Cap to from 0-1 (restricts to the segment ab, instead of the line ab)
            if (t < 0)
                t = 0;
            if (t > 1)
                t = 1;
            
            double sx = a.X + t * (b.X - a.X);
            double sy = a.Y + t * (b.Y - a.Y);

            return new Point(sx, sy);
        }




        /// <summary>
        /// Returns the Euclidean distance between two Points.
        /// </summary>
        /// <param name="a">Point 1</param>
        /// <param name="b">Point 2</param>
        /// <returns>The Euclidean distance between the two Points.</returns>
        public static double Dist(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Returns the Euclidean distance between two Locations.
        /// </summary>
        /// <param name="a">Location 1</param>
        /// <param name="b">Location 2</param>
        /// <returns>The Euclidean distance between the two Locations</returns>
        public static double Dist(Location a, Location b)
        {
            return Math.Sqrt(Math.Pow(a.Latitude - b.Latitude, 2) + Math.Pow(a.Longitude - b.Longitude, 2));
        }
    }
}
