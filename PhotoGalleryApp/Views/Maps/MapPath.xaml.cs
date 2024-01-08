using Microsoft.Maps.MapControl.WPF;
using Microsoft.VisualBasic;
using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Media;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.ViewModels;
using PhotoGalleryApp.Views.Behavior;
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
using System.Windows.Ink;
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


            // Hook map container events, because the only way to capture mouse
            // movement across the whole canvas is to make this MapLayer's
            // background not-null, and that would prevent mouse events from
            // reaching other layers.
            Map? map = ViewAncestor.FindAncestor<Map>(this);
            if (map == null)
                throw new Exception("MapPath must have a Map parent");

            map.MouseMove += Map_MouseMove;
            map.MouseUp += Map_MouseUp;
            // Use preview to disable zoom on double click
            map.PreviewMouseDoubleClick += Map_DoubleClick;

            BindingOperations.SetBinding(this, LocationsProperty, new Binding("Points"));

            BindingOperations.SetBinding(this, SelectionStartIndProperty, new Binding("SelectionStartInd"));
            BindingOperations.SetBinding(this, SelectionEndIndProperty, new Binding("SelectionEndInd"));
        }

        protected override UIElement GetMainMapItem()
        {
            return PathLine;
        }



        #region Selection Indices Properties

        public static readonly DependencyProperty SelectionStartIndProperty = DependencyProperty.Register("SelectionStartInd", typeof(int), typeof(MapPath),
            new PropertyMetadata(SelectionStartIndPropertyChanged));

        /// <summary>
        /// The user can select one or multiple points on this path. The points
        /// will always be consecutive on the path. This set of points is
        /// represented by a starting and ending index. The starting index  is
        /// inclusive and the end index is exclusive. If nothing is selected,
        /// the indices are -1.
        /// </summary>
        public int SelectionStartInd
        {
            get { return (int)GetValue(SelectionStartIndProperty); }
            set { 
                SetValue(SelectionStartIndProperty, value);
                if (EditMode)
                    RebuildSelection();
            }
        }

        public static void SelectionStartIndPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapPath m = (MapPath)sender;
            if(m.EditMode)
                m.RebuildSelection();
        }

        public static readonly DependencyProperty SelectionEndIndProperty = DependencyProperty.Register("SelectionEndInd", typeof(int), typeof(MapPath),
            new PropertyMetadata(SelectionEndIndPropertyChanged));

        /// <summary>
        /// The user can select one or multiple points on this path. The points
        /// will always be consecutive on the path. This set of points is
        /// represented by a starting and ending index. The starting index  is
        /// inclusive and the end index is exclusive. If nothing is selected,
        /// the indices are -1.
        /// </summary>
        public int SelectionEndInd
        {
            get { return (int)GetValue(SelectionEndIndProperty); }
            set { 
                SetValue(SelectionEndIndProperty, value);
                if(EditMode)
                    RebuildSelection();
            }
        }

        public static void SelectionEndIndPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapPath m = (MapPath)sender;
            if(m.EditMode)
                m.RebuildSelection();
        }

        #endregion Selection Indices Properties



        #region Locations Property

        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register("Locations", typeof(LocationCollection), typeof(MapPath),
            new PropertyMetadata(LocationsPropertyChanged));
 

        /// <summary>
        /// An ordered collection of Locations that make up the path
        /// </summary>
        public LocationCollection Locations
        {
            get { return (LocationCollection)GetValue(LocationsProperty); }
            set {
                SetValue(LocationsProperty, value);
                if (EditMode)
                    RebuildSelection();
            }
        }

        private static void LocationsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapPath control = (MapPath)sender;

            if(e.OldValue != null && e.OldValue is LocationCollection)
                ((LocationCollection)e.OldValue).CollectionChanged -= control.Locations_CollectionChanged;

            if(e.NewValue != null && e.NewValue is LocationCollection)
                ((LocationCollection)e.NewValue).CollectionChanged += control.Locations_CollectionChanged;

            if (control.EditMode)
                control.RebuildSelection();
        }


        #region Locations CollectionChanged

        private void Locations_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!EditMode)
                return;

            switch(e.Action)
            {
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

                    // If the point marked by the Nearby Pin changes, move the Nearby Pin with it
                    if(_nearbyPin != null)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                            if (_nearbyPin.Location == (Location)e.OldItems[i])
                                _nearbyPin.Location = (Location)e.NewItems[i];
                    }

                    // If the point marked by the Selection Pin changes, move the Selection Pin with it
                    if(_selectionPin != null)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                            if (_selectionPin.Location == (Location)e.OldItems[i])
                                _selectionPin.Location = (Location)e.NewItems[i];
                    }

                    // If any of the points changed are on the Selection Line,
                    // change the associated points on the Selection Line to
                    // match
                    int numReplaced = e.OldItems.Count;
                    int endIndex = e.OldStartingIndex + numReplaced;
                    if(_selectionLine != null)
                    {
                        // If changed items start before the selection
                        if(e.OldStartingIndex < SelectionStartInd)
                        {
                            // If changed items extend into the selection
                            if(endIndex > SelectionStartInd)
                            {
                                int i = 0;
                                for (int j = SelectionStartInd; j < endIndex; j++, i++)
                                    _selectionLine.Locations[i] = Locations[j];
                            }
                        }
                        // If changed items start in the selection
                        else if(e.OldStartingIndex >= SelectionStartInd && e.OldStartingIndex < SelectionEndInd)
                        {
                            int end = Math.Min(SelectionEndInd, endIndex);
                            int i = e.OldStartingIndex - SelectionStartInd;
                            for(int j = e.OldStartingIndex; j < end; j++, i++)
                                _selectionLine.Locations[i] = Locations[j];
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // If the list is reset, who knows what happened. So just
                    // rebuild the whole selection
                    if (EditMode)
                        RebuildSelection();
                    break;
            }
        }

        #endregion Locations CollectionChanged


        #endregion Locations Property



        #region Selection

        /**
         * There are two options for selecting points on the path. If one point
         * is selected, it is displayed as a single pushpin, which can be
         * deleted and moved around. If multiple points are selected, they are
         * displayed as a differently-color path. The path can be moved and
         * deleted, but individual points cannot be edited.
         */
        private Pushpin? _selectionPin;
        private MapPolyline? _selectionLine;

        /**
         * Keep track of whether the pin or line are currently being clicked or dragged.
         * Turns true on MouseDown, and turns false on MouseUp.
         */
        private bool _selectionPinClick = false;
        private bool _selectionLineClick = false;


        private void CreateSelectionPin()
        {
            _selectionPin = new Pushpin();
            _selectionPin.Location = Locations[SelectionStartInd];
            _selectionPin.MouseDown += SelectionPin_MouseDown;

            MapItemClickDragBehavior b = new MapItemClickDragBehavior(_mapContainer);
            b.MouseDrag += Selection_MouseDrag;
            b.MouseLeftButtonClick += Selection_Click;
            Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_selectionPin).Add(b);

            Children.Add(_selectionPin);
        }

        private void RemoveSelectionPin()
        {
            if(_selectionPin != null)
            {
                Children.Remove(_selectionPin);
                _selectionPin = null;
            }
        }

        private void CreateSelectionLine()
        {
            _selectionLine = new MapPolyline();
            _selectionLine.Stroke = new SolidColorBrush(Colors.Blue);
            _selectionLine.StrokeThickness = 6;
            _selectionLine.Locations = new LocationCollection();
            _selectionLine.MouseDown += SelectionLine_MouseDown;

            MapItemClickDragBehavior b = new MapItemClickDragBehavior(_mapContainer);
            b.MouseDrag += Selection_MouseDrag;
            b.MouseLeftButtonClick += Selection_Click;
            Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_selectionLine).Add(b);

            for(int i = SelectionStartInd; i < SelectionEndInd; i++)
            {
                _selectionLine.Locations.Add(Locations[i]);
            }
            Children.Add(_selectionLine);
        }

        private void RemoveSelectionLine()
        {
            if(_selectionLine != null)
            {
                Children.Remove(_selectionLine);
                _selectionLine = null;
            }
        }


        /**
         * When the selection is clicked, remove the selected points
         */
        private void Selection_Click(object sender, MouseButtonEventArgs e)
        {
            ((MapPathViewModel)DataContext).RemoveSelection();
        }

        /**
         * When the selection is dragged, move the selected points
         */
        private void Selection_MouseDrag(object sender, MouseDragEventArgs e)
        {
            ((MapPathViewModel)DataContext).MoveSelection(e.LatitudeDifference, e.LongitudeDifference);
        }


        private void SelectionPin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _selectionPinClick = true;
        }

        private void SelectionLine_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _selectionLineClick = true;
        }



        /**
         * Creates the selection item, either a Pushpin when one point is
         * selected, or a Polyline when multiple points are selected. When the
         * selection changes, call this.
         */
        private void RebuildSelection()
        {
            if(_selectionPin != null)
                RemoveSelectionPin();

            if(_selectionLine != null)
                RemoveSelectionLine();

            if (SelectionStartInd == -1 || SelectionEndInd == -1)
                return;

            if(SelectionEndInd - SelectionStartInd == 1)
                CreateSelectionPin();
            else
                CreateSelectionLine();
        }


        #endregion Selection



        #region Nearby Pin

        /**
         * Because paths are potentially too big to enable placing a pin for
         * each point, this solution is to place a pin on the closest point
         * only. This pin will appear when the mouse is within a certain radius
         * of the points. The user can click on the path to select whichever
         * point is nearest.
         */
        private Pushpin? _nearbyPin = null;

        /**
         * Keep track of whether the pin is being clicked or dragged. Turns
         * true on MouseDown, and turns false on MouseUp.
         */
        private bool _nearbyPinClick = false;

        private void CreateNearbyPin(Location l)
        {
            _nearbyPin = new Pushpin();
            _nearbyPin.Location = l;
            _nearbyPin.MouseDown += NearbyPin_MouseDown;
            MapItemClickDragBehavior b = new MapItemClickDragBehavior(_mapContainer);
            b.MouseDrag += NearbyPin_MouseDrag;
            Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_nearbyPin).Add(b);
            Children.Add(_nearbyPin);
        }

        private void RemoveNearbyPin()
        {
            if(_nearbyPin != null)
            {
                Children.Remove(_nearbyPin);
                _nearbyPin = null;
            }
        }

        /**
         * When the user drags the Nearby Pin, move the associated point
         */
        private void NearbyPin_MouseDrag(object sender, MouseDragEventArgs e)
        {
            ((MapPathViewModel)DataContext).MovePoint(_nearbyPin.Location, e.LatitudeDifference, e.LongitudeDifference);
        }

        private void NearbyPin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _nearbyPinClick = true;
        }


        /**
         * Find the nearest point to the given position, and if it's close
         * enough, display it as the Nearby Pin
         */
        private void RebuildNearbyPin(Point clickLoc)
        {
            double minDist = double.PositiveInfinity;
            int minInd = 0;
            // Find the closest path point
            for (int i = 0; i < Locations.Count; i++)
            {
                Location l = Locations[i];
                Point pointLoc = _mapContainer.LocationToViewportPoint(l);

                if (pointLoc.X < 0 || pointLoc.Y < 0 || pointLoc.X > this.RenderSize.Width || pointLoc.Y > this.RenderSize.Height)
                    continue;

                double dist = Math.Sqrt(Math.Pow(clickLoc.X - pointLoc.X, 2) + Math.Pow(clickLoc.Y - pointLoc.Y, 2));
                if (dist < minDist)
                {
                    minDist = dist;
                    minInd = i;
                }
            }

            //TODO If the nearest point hasn't changed, just leave the pin
            // Was causing flickering before
            if (_nearbyPin != null)
                RemoveNearbyPin();

            // Don't add the point if it's on the selection line.
            if(_selectionLine != null)
            {
                foreach(Location l in _selectionLine.Locations)
                {
                    if (l == Locations[minInd])
                        return;
                }
            }

            // If the point is close enough, show the nearby pin
            if (minDist < 20)
            {
                if (_selectionPin == null || _selectionPin.Location != Locations[minInd])
                    CreateNearbyPin(Locations[minInd]);
            }
        }

        #endregion Nearby Pin



        #region Methods

        protected override void OpenPreview()
        {
            if(_preview == null)
            {
                _preview = new MapPathPreview();
                _preview.DataContext = DataContext;
                Children.Add(_preview);

                // Place it slightly above the first point
                if(Locations != null && Locations.Count > 0)
                {
                    SetPositionOffset(_preview, new Point(-_preview.Width / 2, -_preview.Height - 50));
                    SetPosition(_preview, Locations[0]);
                }
            }
        }

        protected override void EditModeChanged()
        {
            base.EditModeChanged();

            // If leaving edit mode, remove all edit-mode related components
            if (!EditMode)
            {
                RemoveSelectionLine();
                RemoveSelectionPin();
                RemoveNearbyPin();
            }
        }


        #endregion Methods



        #region Mouse Handling

        /**
         * When the user clicks on the path
         */
        protected override void MainMapItem_Click(object sender, MouseEventArgs e)
        {
            /**
             * If there is a pin nearby, clicking on the path will select that point
             */
            if(_nearbyPin != null)
            {
                ((MapPathViewModel)DataContext).SelectPoint(_nearbyPin.Location);

                Children.Remove(_nearbyPin);
                _nearbyPin = null;
            }
            // If in edit mode, clicking on the path will insert a point on the path
            else if(EditMode)
            {
                if(_nearbyPin == null)
                {
                    ((MapPathViewModel)DataContext).InsertPointAt(_mapContainer.ViewportPointToLocation(e.GetPosition(_mapContainer)), false);
                    RebuildNearbyPin(e.GetPosition(_mapContainer));
                }
            }
            // If not in edit mode, clicking on the path will open the preview box
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




        /**
         * In edit mode, double clicking on the map will append a point to the path
         */
        private void Map_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!EditMode)
                return;

            // Only add a point if clicking on the map, not the path or pins.
            if (PathLine.IsMouseOver)
                return;

            if (_nearbyPin != null && _nearbyPin.IsMouseOver)
                return;

            //TODO Clicking on a selection pin will delete it, and then clicking again will
            // finish the double click on the map background. The original click should really
            // be absorbed by the pin
            if (_selectionPin != null && _selectionPin.IsMouseOver)
                return;

            ((MapPathViewModel)DataContext).InsertPointAt(_mapContainer.ViewportPointToLocation(e.GetPosition(_mapContainer)), true);
        }


        /**
         * When the user clicks on the map in edit mode, it deselects any selected points
         */
        protected override void Map_Click(object sender, MouseEventArgs e)
        {
            ((MapPathViewModel)DataContext).MapClick();
        }


        private void Map_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // If a pushpin or line was clicked on, then the Nearby Pin
            // will not be shown. So as soon as the click ends, reshow the
            // Nearby Pin. Don't wait until the user moves the mouse.
            if(_nearbyPinClick || _selectionPinClick || _selectionLineClick)
                RebuildNearbyPin(e.GetPosition(_mapContainer));

            _nearbyPinClick = false;
            _selectionPinClick = false;
            _selectionLineClick = false;
        }



        /**
         * For some reason, MouseMove gets called even when the mouse is not
         * moving when the mouse is hovering over a pushpin. Save the last
         * mouse pos to detect when the mouse isn't actually moving
         */
        private Point _lastMousePos = new Point();

        /**
         * When the mouse moves, update the Nearby Pin
         */
        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            // For some reason, MouseMove gets called even when the mouse is not moving
            // when the mouse is hovering over a pushpin. So don't do the handling if
            // the mouse isn't actually moving
            Point pos = e.GetPosition(_mapContainer);
            if (pos == _lastMousePos)
                return;

            // Don't show the Nearby Pin if any of the pins/paths are been clicked or dragged
            if (_selectionLineClick == false && _selectionPinClick == false && _nearbyPinClick == false && EditMode)
            {
                RebuildNearbyPin(e.GetPosition(_mapContainer));
            }
            else if (_nearbyPinClick == false)
                RemoveNearbyPin();

            _lastMousePos = e.GetPosition(_mapContainer);
        }


        #endregion Mouse Handling
    }
}
