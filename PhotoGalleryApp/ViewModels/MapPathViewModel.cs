﻿using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the MapPath model
    /// </summary>
    public class MapPathViewModel : MapItemViewModel
    {
        public MapPathViewModel(NavigatorViewModel nav, MapPath path)
        {
            _openPageCommand = new RelayCommand(OpenPage);

            PreviewType = typeof(PhotoGalleryApp.Views.Maps.MapPathPreview);

            _nav = nav;
            _path = path;

            Points.CollectionChanged += Points_CollectionChanged;
        }

        public override void Cleanup() {}


        private NavigatorViewModel _nav;
        private MapPath _path;


        public LocationCollection Points { get { return _path.Locations; } }






        #region Selected Points

        private int _selStart = -1;
        /// <summary>
        /// The user can select a consecutive range of points on the path. This
        /// range is marked by an inclusive start index and an exclusive end
        /// index. If nothing is selected, the indices are -1.
        /// </summary>
        public int SelectionStartInd { 
            get { return _selStart; } 
            set
            {
                _selStart = value;
                OnPropertyChanged();
            }
        }


        private int _selEnd = -1;
        /// <summary>
        /// The user can select a consecutive range of points on the path. This
        /// range is marked by an inclusive start index and an exclusive end
        /// index. If nothing is selected, the indices are -1.
        /// </summary>
        public int SelectionEndInd { 
            get { return _selEnd; } 
            set
            {
                _selEnd = value;
                OnPropertyChanged();
            }
        }


        /**
         * Deselect everything
         */
        private void DeselectPoints()
        {
            SelectionStartInd = -1;
            SelectionEndInd = -1;
        }

        /**
         * Updates the selection range when the collection of points changes
         */
        private void Points_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) 
            { 
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null || e.NewItems.Count == 0)
                        break;

                    if (SelectionStartInd == -1 || SelectionEndInd == -1)
                        break;

                    // Move selection range to account for new items
                    int numAdded = e.NewItems.Count;
                    if(e.NewStartingIndex <= SelectionStartInd)
                    {
                        SelectionStartInd += numAdded;
                        SelectionEndInd += numAdded;
                    }
                    else if(e.NewStartingIndex > SelectionStartInd && e.NewStartingIndex < SelectionEndInd)
                    {
                        SelectionEndInd += numAdded;
                    }

                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null || e.OldItems.Count == 0)
                        break;

                    if (SelectionStartInd == -1 || SelectionEndInd == -1)
                        break;

                    //Move selection range (& maybe shrink it) to account for removed items
                    int numRemoved = e.OldItems.Count;
                    int endIndex = e.OldStartingIndex + numRemoved;

                    if(e.OldStartingIndex < SelectionStartInd)
                    {
                        if(endIndex > SelectionStartInd)
                        {
                            SelectionStartInd = endIndex;
                            SelectionEndInd -= numRemoved;
                        }
                        else
                        {
                            SelectionStartInd -= numRemoved;
                            SelectionEndInd -= numRemoved;
                        }
                    }
                    else if(e.OldStartingIndex >= SelectionStartInd && e.OldStartingIndex < SelectionEndInd)
                    {
                        if(endIndex < SelectionEndInd)
                            SelectionEndInd -= numRemoved;
                        else
                            SelectionEndInd = e.OldStartingIndex;
                        
                    }

                    // If nothing is left to select, reset selection to nonen
                    if(SelectionEndInd <= SelectionStartInd)
                    {
                        SelectionStartInd = -1;
                        SelectionEndInd = -1;
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    DeselectPoints();
                    break;
            }

        }

        #endregion Selected Points



        #region Add,Move,Remove

        /**
         * Moves the given point by the given latitudinal and longitudinal offsets
         */
        public void MovePoint(Location pointLoc, double latdiff, double longdiff)
        {
            for(int i = 0; i < Points.Count; i++)
            {
                if (Points[i] == pointLoc)
                {
                    Points[i] = new Location(Points[i].Latitude + latdiff, Points[i].Longitude + longdiff);
                    return;
                }
            }
        }

        /**
         * Moves the selected point(s) by the given latitudinal and longitudinal offsets
         */
        public void MoveSelection(double latdiff, double longdiff)
        {
            for(int i = SelectionStartInd; i < SelectionEndInd; i++)
            {
                Points[i] = new Location(Points[i].Latitude + latdiff, Points[i].Longitude + longdiff);
            }
        }


        /**
         * Removes the given point from the path
         */
        public void RemovePoint(Location location)
        {
            for(int i = 0; i < Points.Count; i++)
            {
                if (Points[i] == location)
                {
                    Points.RemoveAt(i);
                    return;
                }
            }
        }


        /**
         * Deletes the selected point(s) from the path
         */
        public void RemoveSelection()
        {
            while (SelectionStartInd < SelectionEndInd)
                Points.RemoveAt(SelectionStartInd);
        }


        /**
         * Inserts a point at the given location. If append is true, the point will be
         * connected to the last point in the list. Otherwise, it will be inserted in the
         * list such that it breaks up the line segment closest to it
         */
        public void InsertPointAt(Location location, bool append)
        {
            if (append)
                Points.Add(location);

            else
            {
                double minDist = double.PositiveInfinity;
                int minInd = 0;
                // Find the path segment closest to the new point
                for (int i = 0; i < Points.Count - 1; i++)
                {
                    double x2 = Points[i + 1].Latitude;
                    double x1 = Points[i].Latitude;
                    double y2 = Points[i + 1].Longitude;
                    double y1 = Points[i].Longitude;

                    double dist;

                    // Distance from point to line segment
                    // Algorithm from: https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
                    double l2 = Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2);
                    if (l2 == 0)
                        dist = Math.Sqrt(Math.Pow(x2 - location.Latitude, 2) + Math.Pow(y2 - location.Longitude, 2));
                    else
                    {
                        double t = ((location.Latitude - x1) * (x2 - x1) + (location.Longitude - y1) * (y2 - y1)) / l2;
                        t = Math.Max(0, Math.Min(1, t));
                        double x3 = x1 + t * (x2 - x1);
                        double y3 = y1 + t * (y2 - y1);
                        dist = Math.Sqrt(Math.Pow(x3 - location.Latitude, 2) + Math.Pow(y3 - location.Longitude, 2));
                    }

                    if(dist < minDist)
                    {
                        minDist = dist;
                        minInd = i;
                    }
                }

                Points.Insert(minInd+1, location);
            }
        }

        #endregion Add,Move,Remove




        #region Clicks

        public void SelectPoint(Location l)
        {
            // Find the corresponding point
            int ind = -1;
            for(int i = 0; i < Points.Count; i++)
            {
                if (Points[i] == l)
                    ind = i;
            }

            if (ind == -1)
                throw new Exception("NearbyPointClick");


            // If this is the first point, select it
            if(SelectionStartInd == -1)
            {
                SelectionStartInd = ind;
                SelectionEndInd = ind + 1;
            }
            // If other points are already selected, extend the range to include this new point
            else
            {
                if (ind < SelectionStartInd)
                    SelectionStartInd = ind;

                if (ind+1 > SelectionEndInd)
                    SelectionEndInd = ind + 1;
            }
        }


        /**
         * Should be called when the user clicks on the map. This will deselect everything.
         */
        public void MapClick()
        {
            if(EditMode)
            {
                DeselectPoints();
            }
        }


        #endregion Clicks



        private RelayCommand _openPageCommand;
        public ICommand OpenPageCommand => _openPageCommand;
        
        /**
         * Opens the page with the full info about the path 
         */
        public void OpenPage()
        {
            _nav.NewPage(new PathPageViewModel(_nav, _path));
        }



        protected override void EditModeChanged()
        {
            // Don't save the selection when leaving edit mode
            if(!EditMode)
                DeselectPoints();
        }


        public override MapItem GetModel()
        {
            return _path;
        }
    }
}