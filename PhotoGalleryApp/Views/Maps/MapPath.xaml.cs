using Microsoft.Maps.MapControl.WPF;
using Microsoft.VisualBasic;
using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Media;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoGalleryApp.Views.Maps
{
    /// <summary>
    /// Interaction logic for MapPath.xaml
    /// </summary>
    public partial class MapPath : MapItemView
    {
        public MapPath()
        {
            InitializeComponent();
        }


        protected override void MapObject_Loaded(object sender, RoutedEventArgs e)
        {
            base.MapObject_Loaded(sender, e);

            BindingOperations.SetBinding(this, LocationsProperty, new Binding("Points"));
        }


        protected override UIElement GetMainMapItem()
        {
            return PathLine;
        }

        protected override void EditModeChanged()
        {
            base.EditModeChanged();

            // If entering edit mode, create edit pushpins for all points
            if (EditMode)
                Locations_Reset();
            // If leaving edit mode, remove the edit pushpins
            else
                ClearPushpins();

        }



        #region Locations Property

        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register("Locations", typeof(LocationCollection), typeof(MapPath),
            new PropertyMetadata(LocationsPropertyChanged));
 
        /**
         * An ordered collection of Locations that make up the path
         */
        public LocationCollection Locations
        {
            get { return (LocationCollection)GetValue(LocationsProperty); }
            set {
                SetValue(LocationsProperty, value);
                if(EditMode)
                    Locations_Reset();
            }
        }

        private static void LocationsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapPath control = (MapPath)sender;

            if(e.OldValue != null && e.OldValue is LocationCollection)
                ((LocationCollection)e.OldValue).CollectionChanged -= control.Locations_CollectionChanged;

            if(e.NewValue != null && e.NewValue is LocationCollection)
                ((LocationCollection)e.NewValue).CollectionChanged += control.Locations_CollectionChanged;

            if(control.EditMode)
                control.Locations_Reset();
        }


        #region Locations CollectionChanged

        /**
         * If the edit pins are visible, update them
         */
        private void Locations_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!EditMode)
                return;

            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                    {
                        Trace.WriteLine("Error: MapPath - NewItems in CollChanged null");
                        break;
                    }

                    Locations_Add(e.NewItems);

                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                    {
                        Trace.WriteLine("Error: MapPath - OldItems in CollChanged null");
                        break;
                    }

                    Locations_Remove(e.OldItems);

                    break;


                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null)
                    {
                        Trace.WriteLine("Error: MapPath - NewItems in CollChanged null");
                        break;
                    }
                    if (e.OldItems == null)
                    {
                        Trace.WriteLine("Error: MapPath - OldItems in CollChanged null");
                        break;
                    }

                    Locations_Remove(e.OldItems);
                    Locations_Add(e.NewItems);

                    break;

                case NotifyCollectionChangedAction.Reset:

                default:
                    throw new NotImplementedException();
            }
        }

        private void Locations_Add(IList locs)
        {
            foreach(Location loc in locs)
            {
                AddLocationPushpin(loc);
            }    
        }

        private void Locations_Remove(IList locs)
        {
            for(int i = 0; i < locs.Count; i++)
            {
                for(int j = 0; j < Children.Count; j++)
                {
                    if (Children[j] is Pushpin)
                    {
                        Pushpin p = (Pushpin)Children[j];
                        if (p.Location == (Location)locs[i])
                        {
                            Children.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }

        public void Locations_Reset()
        {
            ClearPushpins();

            foreach(Location l in Locations)
                AddLocationPushpin(l);
        }

        #endregion Locations CollectionChanged


        #endregion Locations Property


        #region Methods


        /**
         * Create an edit pin for the given location
         */
        private void AddLocationPushpin(Location l)
        {
            Pushpin p = new Pushpin();
            p.MouseDown += Pin_MouseDown;
            p.MouseUp += Pin_MouseUp;
            p.Location = l;
            Children.Add(p);
        }


        /**
         * Remove all the edit pins
         */
        private void ClearPushpins()
        {
            for(int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is Pushpin)
                {
                    Children.RemoveAt(i);
                    i--;
                }
            }
        }


        /**
         * Inserts a point at the given location. If append is true, the point will be
         * connected to the last point in the list. Otherwise, it will be inserted in the
         * list such that it breaks up the line segment closest to it
         */
        private void InsertPointAt(Location loc, bool append)
        {
            if(append)
                PathLine.Locations.Add(loc);

            else
            {
                double minDist = double.PositiveInfinity;
                int minInd = 0;
                // Find the path segment closest to the new point
                for (int i = 0; i < PathLine.Locations.Count - 1; i++)
                {
                    double x2 = PathLine.Locations[i + 1].Latitude;
                    double x1 = PathLine.Locations[i].Latitude;
                    double y2 = PathLine.Locations[i + 1].Longitude;
                    double y1 = PathLine.Locations[i].Longitude;

                    double dist;

                    // Distance from point to line segment
                    // Algorithm from: https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
                    double l2 = Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2);
                    if (l2 == 0)
                        dist = Math.Sqrt(Math.Pow(x2 - loc.Latitude, 2) + Math.Pow(y2 - loc.Longitude, 2));
                    else
                    {
                        double t = ((loc.Latitude - x1) * (x2 - x1) + (loc.Longitude - y1) * (y2 - y1)) / l2;
                        t = Math.Max(0, Math.Min(1, t));
                        double x3 = x1 + t * (x2 - x1);
                        double y3 = y1 + t * (y2 - y1);
                        dist = Math.Sqrt(Math.Pow(x3 - loc.Latitude, 2) + Math.Pow(y3 - loc.Longitude, 2));
                    }


                    if(dist < minDist)
                    {
                        minDist = dist;
                        minInd = i;
                    }
                }

                PathLine.Locations.Insert(minInd+1, loc);
            }
        }


        protected override void OpenPreview()
        {
            if(_preview == null)
            {
                _preview = new MapPathPreview();
                _preview.DataContext = DataContext;
                Children.Add(_preview);

                // Place it slightly above the first point
                SetPositionOffset(_preview, new Point(-_preview.Width / 2, -_preview.Height - 50));
                SetPosition(_preview, Locations[0]);
            }
        }

        #endregion Methods


        #region Mouse Handling

        protected override void Map_Click(object sender, MouseEventArgs e)
        {
            // If in edit mode, clicking on the background will place a point
            // at that location, connected to the end of the path
            if(EditMode)
                InsertPointAt(_parentMap.MapView.ViewportPointToLocation(e.GetPosition(this)), true);
        }

        private void MapLayer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // If currently dragging an edit pin, update that pin's location & the location in the path
                if(_selPin != null)
                {
                    _pinDrag = true;

                    Location newloc = _parentMap.MapView.ViewportPointToLocation(Point.Add(e.GetPosition(this), _mousePos));

                    for(int i = 0; i < Locations.Count; i++)
                    {
                        //TODO How to differeniate when multiple pins have same location?
                        // Update the path's coordinate
                        if(_selPin.Location == Locations[i])
                        {
                            Locations[i] = newloc;
                            break;
                        }
                    }
                    _selPin.Location = newloc;
                }

                e.Handled = true;
            }
        }


        protected override void MainMapItem_Click(object sender, MouseEventArgs e)
        {
            // If in edit mode, clicking on the path will insert a point on the path
            if(EditMode)
            {
                InsertPointAt(_parentMap.MapView.ViewportPointToLocation(e.GetPosition(this)), false);
            }
            else
            {
                // Tell the MapViewModel to open the preview box
                Map? map = ViewAncestor.FindAncestor<Map>(this);
                if(map != null)
                {
                    MapViewModel vm = (MapViewModel)map.DataContext;
                    vm.TogglePreview(DataContext);
                }
            }

            e.Handled = true;
        }


        #region Pin Drag

        private bool _pinDrag;
        Vector _mousePos;
        private Pushpin? _selPin = null;


        private void Pin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(sender is Pushpin)
            {
                e.Handled = true;

                if(!_pinDrag && _selPin != null)
                {
                    Pushpin p = (Pushpin)sender;
                    MapPathViewModel vm = (MapPathViewModel)DataContext;
                    vm.RemovePoint(p.Location);
                }

            }


            // Disable the background from capturing mouse clicks. Otherwise, the background will prevent mouse clicks
            // from reaching the polylines
            Background = null;

            _selPin = null;
            _pinDrag = false;
        }

        private void Pin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Pushpin)
                return;

            e.Handled = true;

            if(_selPin == null) {

                Pushpin p = (Pushpin)sender;
                _selPin = p;
                _mousePos = Point.Subtract(_parentMap.MapView.LocationToViewportPoint(p.Location), e.GetPosition(this));
                _pinDrag = false;
            }

            // Set background to transparent so it captures mouse clicks. This enables the layer's MouseMove event, which is used to
            // move a pin around when it's being dragged. Wouldn't need to use this if it used the Pushpin's MouseMove instead of
            // the MapLayer's MouseMove, but when using the pushpin, its location updates too slowly, so the mouse can move outside of the
            // pushpin and it will stop moving along with the mouse.
            Background = new SolidColorBrush(Colors.Transparent);
        }

        #endregion Pin Drag


        #endregion Mouse Handling

    }
}
