﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Utils;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Information about a single location on the map
    /// </summary>
    [DataContract]
    public class MapLocation : MapItem
    {
        public MapLocation(string name, Location coordinates) : base(name)
        {
            _location = coordinates;
            Children = new ObservableCollection<MapLocation>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private Location _location;
        public Location Location
        {
            get { return _location; }
            set { _location = value; }
        }


        // Store specific properties from Location so only those are serialized
        [DataMember(Name = nameof(Location))]
        private LocationSerializable _locationSerializable
        {
            get { return new LocationSerializable(Location); }
            set { this.Location = value.GetLocation(); }
        }


        private MapLocation? _parent = null;
        public MapLocation? Parent
        {
            get { return _parent; }
            protected set { _parent = value; }
        }


        public ObservableCollection<MapLocation> Children
        {
            get; internal set;
        }



        public delegate void ChildrenLocationChangedDelegate(MapLocation location);
        /// <summary>
        /// Called when coordinates of children of this MapLocation change
        /// </summary>
        public ChildrenLocationChangedDelegate? ChildrenLocationChanged = null;


        /** When children of this Location change their coordinates, call a
         * delegate function
         */
        private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("NewItems is null");

                    foreach (MapLocation loc in e.NewItems)
                    {
                        loc.PropertyChanged += Child_PropertyChanged;
                        loc.Parent = this;
                    }


                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("OldItems is null");

                    foreach (MapLocation loc in e.OldItems)
                    {
                        loc.PropertyChanged -= Child_PropertyChanged;
                        if (loc.Parent == this)
                            loc.Parent = null;
                    }

                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null)
                        throw new ArgumentException("NewItems is null");
                    if (e.OldItems == null)
                        throw new ArgumentException("OldItems is null");

                    foreach (MapLocation loc in e.OldItems)
                    {
                        loc.PropertyChanged -= Child_PropertyChanged;
                        if (loc.Parent == this)
                            loc.Parent = null;
                    }
                    foreach (MapLocation loc in e.NewItems)
                    {
                        loc.PropertyChanged += Child_PropertyChanged;
                        loc.Parent = this;
                    }

                    break;

                default:
                    foreach (MapLocation loc in Children)
                    {
                        loc.PropertyChanged += Child_PropertyChanged;
                        loc.Parent = this;
                    }

                    break;
            }
        }


        private void Child_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Location))
            {
                if (ChildrenLocationChanged != null)
                    ChildrenLocationChanged(this);
            }
        }


        public static bool FarApart(MapLocation loc, double zoomLevel)
        {
            if (loc.Children.Count == 0)
                return false;

            if (loc.Children.Count == 1)
                return true;


            double mapHeight = 256 * Math.Pow(2, zoomLevel);

            // How much latitude/longitude is covered by a single pixel
            double latPerPix = (85.05 * 2) / mapHeight;
            // 10 pixels is too close
            double distanceBound = latPerPix * 25;

            bool farApart = true;

            for (int i = 0; i < loc.Children.Count; i++)
            {
                for (int j = i + 1; j < loc.Children.Count; j++)
                {
                    Location l1 = loc.Children[i].Location;
                    Location l2 = loc.Children[j].Location;
                    double dist = Math.Sqrt(Math.Pow(l1.Latitude - l2.Latitude, 2) + Math.Pow(l1.Longitude - l2.Longitude, 2));

                    if (dist < distanceBound)
                    {
                        farApart = false;
                        break;
                    }
                }

                if (!farApart)
                    break;
            }

            return farApart;
        }
    }
}
