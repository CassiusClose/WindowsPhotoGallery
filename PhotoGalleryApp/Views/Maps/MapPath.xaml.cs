﻿using Microsoft.Maps.MapControl.WPF;
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
using System.ComponentModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
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


        public override void Init(Map map, MapItemViewModel context)
        {
            base.Init(map, context);

            map.MouseMove += Map_MouseMove;
            map.MouseUp += Map_MouseUp;
            // Use preview to disable zoom on double click
            map.PreviewMouseDoubleClick += Map_DoubleClick;
            map.MapView.ViewChangeOnFrame += MapView_ViewChangeOnFrame;

            BindingOperations.SetBinding(this, LocationsProperty, new Binding("Points"));
            BindingOperations.SetBinding(this, SelectionRangeProperty, new Binding("SelectionRange"));
            BindingOperations.SetBinding(this, OverrideStrokeThicknessProperty, new Binding("OverrideStrokeThickness"));
            BindingOperations.SetBinding(this, OverridePathColorProperty, new Binding("OverridePathColor"));

            Binding b = new Binding("IsInView");
            b.Mode = BindingMode.OneWayToSource;
            BindingOperations.SetBinding(this, IsInViewProperty, b);

            b = new Binding("BoundingBox");
            b.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, BoundingBoxProperty, b);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Locations.CollectionChanged -= Locations_CollectionChanged;
            _map.MouseMove -= Map_MouseMove;
            _map.MouseUp -= Map_MouseUp;
            _map.PreviewMouseDoubleClick -= Map_DoubleClick;
            _map.MapView.ViewChangeOnFrame -= MapView_ViewChangeOnFrame;
        }

        /**
         * Removes all components from any MapLayers
         */
        public override void RemoveAll()
        {
            if (_preview != null)
                ClosePreview();
            if (_nearbyPin != null)
                RemoveNearbyPin();
            if (_selectionLine != null)
                RemoveSelectionLine();
            if (_selectionPin != null)
                RemoveSelectionPin();
            _map.LineLayer_Remove(PathLine);
            _map.SelectedLineLayer_Remove(PathLine);
        }




        #region Path Line

        private MapPolyline PathLine;


        protected override void Init_MainMapItem()
        {
            PathLine = new MapPolyline();
            StrokeThicknessChanged();
            PathColorChanged();
            _map.LineLayer_Add(PathLine);
        }

        protected override UIElement GetMainMapItem()
        {
            return PathLine;
        }

        #endregion Path Line





        #region Selection Range Property

        public static readonly DependencyProperty SelectionRangeProperty = DependencyProperty.Register("SelectionRange", typeof(System.Drawing.Point), typeof(MapPath),
            new PropertyMetadata(SelectionRangePropertyChanged));

        /// <summary>
        /// The user can select one or multiple points on this path. The points
        /// will always be consecutive on the path. This set of points is
        /// represented by a starting and ending index. The starting index  is
        /// inclusive and the end index is exclusive. If nothing is selected,
        /// the indices are -1.
        /// </summary>
        public System.Drawing.Point SelectionRange
        {
            get { return (System.Drawing.Point)GetValue(SelectionRangeProperty); }
            set { 
                SetValue(SelectionRangeProperty, value);
                if (EditMode)
                    RebuildSelection();
            }
        }

        public static void SelectionRangePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapPath m = (MapPath)sender;
            if(m.EditMode)
                m.RebuildSelection();
        }

        #endregion Selection Range Property



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
                LocationsChanged();
            }
        }

        private static void LocationsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapPath control = (MapPath)sender;

            if(e.OldValue != null && e.OldValue is LocationCollection)
                ((LocationCollection)e.OldValue).CollectionChanged -= control.Locations_CollectionChanged;

            control.LocationsChanged();
        }

        private void LocationsChanged()
        {
            Locations.CollectionChanged += Locations_CollectionChanged;
            PathLine.Locations = Locations;

            if (EditMode)
                RebuildSelection();

            // The map will not redraw if the Locations property is reset, so
            // trigger reset
            MapPolyline line = new MapPolyline();
            _map.MapView.Children.Add(line);
            _map.MapView.Children.Remove(line);
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
                                SetNearbyPinLocation((Location)e.NewItems[i]);
                    }

                    // If the point marked by the Selection Pin changes, move the Selection Pin with it
                    if(_selectionPin != null)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                            if (_selectionPin.Location == (Location)e.OldItems[i])
                                SetSelectionPinLocation((Location)e.NewItems[i]);
                    }

                    // If any of the points changed are on the Selection Line,
                    // change the associated points on the Selection Line to
                    // match
                    int numReplaced = e.OldItems.Count;
                    int endIndex = e.OldStartingIndex + numReplaced;
                    if(_selectionLine != null)
                    {
                        // If changed items start before the selection
                        if(e.OldStartingIndex < SelectionRange.X)
                        {
                            // If changed items extend into the selection
                            if(endIndex > SelectionRange.X)
                            {
                                int i = 0;
                                for (int j = SelectionRange.X; j < endIndex; j++, i++)
                                    _selectionLine.Locations[i] = Locations[j];
                            }
                        }
                        // If changed items start in the selection
                        else if(e.OldStartingIndex >= SelectionRange.X && e.OldStartingIndex < SelectionRange.Y)
                        {
                            int end = Math.Min(SelectionRange.Y, endIndex);
                            int i = e.OldStartingIndex - SelectionRange.X;
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



        #region Is In View Property

        public static readonly DependencyProperty IsInViewProperty = DependencyProperty.Register("IsInView", typeof(bool), typeof(MapPath));
 

        /// <summary>
        /// Whether the path is visible in the current map viewport
        /// </summary>
        public bool IsInView 
        {
            get { return (bool)GetValue(IsInViewProperty); }
            set { 
                if(IsInView != value)
                    SetValue(IsInViewProperty, value); 
            }
        }


        private void MapView_ViewChangeOnFrame(object? sender, MapEventArgs e)
        {
            UpdateIsInView();
        }

        private void UpdateIsInView()
        {
            if (Locations.Count == 0)
                return;

            Size viewportSize = _map.MapView.ViewportSize;

            Point topLeft = _map.MapView.LocationToViewportPoint(new Location(BoundingBox.Bottom, BoundingBox.Left));
            Point bottomRight = _map.MapView.LocationToViewportPoint(new Location(BoundingBox.Top, BoundingBox.Right));

            bool val = false;
            // If the map viewport intersects with the path bounding box
            if(topLeft.X < viewportSize.Width && bottomRight.X > 0 && topLeft.Y > 0 && bottomRight.Y < viewportSize.Height)
            {
                val = true;
            }

            IsInView = val;
        }

        #endregion Is In View Property


        #region Bounding Box Property

        public static readonly DependencyProperty BoundingBoxProperty = DependencyProperty.Register("BoundingBox", typeof(RectangleD), typeof(MapPath),
            new FrameworkPropertyMetadata(BoundingBoxPropertyChanged));

        /**
         * Rectangular bounding box around the path. Used to determine whether
         * the path is in view or not
         */
        public RectangleD BoundingBox
        {
            get { return (RectangleD)GetValue(BoundingBoxProperty); }
            set 
            { 
                SetValue(BoundingBoxProperty, value);
                UpdateIsInView();
            }
        }


        private static void BoundingBoxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapPath path = (MapPath)d;
            path.UpdateIsInView();
        }

        #endregion Bounding Box Property


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
            _selectionPin.Location = Locations[SelectionRange.X];
            _selectionPin.MouseDown += SelectionPin_MouseDown;

            MapItemClickDragBehavior b = new MapItemClickDragBehavior(_map.MapView);
            b.MouseDrag += Selection_MouseDrag;
            b.MouseLeftButtonClick += Selection_Click;
            Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_selectionPin).Add(b);

            _map.SelectedPinLayer_Add(_selectionPin);
        }

        private void RemoveSelectionPin()
        {
            if(_selectionPin != null)
            {
                _map.SelectedPinLayer_Remove(_selectionPin);
                _selectionPin.MouseDown -= SelectionPin_MouseDown;
                foreach(var behavior in Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_selectionPin))
                {
                    if(behavior is  MapItemClickDragBehavior)
                    {
                        MapItemClickDragBehavior b = (MapItemClickDragBehavior)behavior;
                        b.MouseDrag -= Selection_MouseDrag;
                        b.MouseLeftButtonClick -= Selection_Click;
                        break;
                    }
                }
                _selectionPin = null;
            }
        }

        /**
         * Sets the Selection Pin's location, which includes notifying the parent
         * Map, so a redraw is triggered
         */
        private void SetSelectionPinLocation(Location l)
        {
            if(_selectionPin != null)
            {
                _selectionPin.Location = l;
                _map.SelectedPinLayer_Update(_selectionPin);
            }
        }


        private void CreateSelectionLine()
        {
            _selectionLine = new MapPolyline();
            _selectionLine.Stroke = SelectionColorBrush;
            _selectionLine.StrokeThickness = 6;
            _selectionLine.Locations = new LocationCollection();
            _selectionLine.MouseDown += SelectionLine_MouseDown;

            MapItemClickDragBehavior b = new MapItemClickDragBehavior(_map.MapView);
            b.MouseDrag += Selection_MouseDrag;
            b.MouseLeftButtonClick += Selection_Click;
            Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_selectionLine).Add(b);

            for(int i = SelectionRange.X; i < SelectionRange.Y; i++)
            {
                _selectionLine.Locations.Add(Locations[i]);
            }
            _map.SelectedLineLayer_Add(_selectionLine);
        }

        private void RemoveSelectionLine()
        {
            if(_selectionLine != null)
            {
                _map.SelectedLineLayer_Remove(_selectionLine);
                _selectionLine.MouseDown -= SelectionLine_MouseDown;
                foreach(var behavior in Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_selectionLine))
                {
                    if(behavior is  MapItemClickDragBehavior)
                    {
                        MapItemClickDragBehavior b = (MapItemClickDragBehavior)behavior;
                        b.MouseDrag -= Selection_MouseDrag;
                        b.MouseLeftButtonClick -= Selection_Click;
                        break;
                    }
                }
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

            if (SelectionRange.X == -1 || SelectionRange.Y == -1)
                return;

            if(SelectionRange.Y - SelectionRange.X == 1)
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
            MapItemClickDragBehavior b = new MapItemClickDragBehavior(_map.MapView);
            b.MouseDrag += NearbyPin_MouseDrag;
            Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_nearbyPin).Add(b);
            _map.SelectedPinLayer_Add(_nearbyPin);
        }

        private void RemoveNearbyPin()
        {
            if(_nearbyPin != null)
            {
                _map.SelectedPinLayer_Remove(_nearbyPin);
                _nearbyPin.MouseDown -= NearbyPin_MouseDown;
                foreach(var behavior in Microsoft.Xaml.Behaviors.Interaction.GetBehaviors(_nearbyPin))
                {
                    if(behavior is  MapItemClickDragBehavior)
                    {
                        MapItemClickDragBehavior b = (MapItemClickDragBehavior)behavior;
                        b.MouseDrag -= NearbyPin_MouseDrag;
                        break;
                    }
                }
                _nearbyPin = null;
            }
        }

        /**
         * Sets the Nearby Pin's location, which includes notifying the parent
         * Map, so a redraw is triggered
         */
        private void SetNearbyPinLocation(Location l)
        {
            if(_nearbyPin != null)
            {
                _nearbyPin.Location = l;
                _map.SelectedPinLayer_Update(_nearbyPin);
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
            //TODO If the nearest point hasn't changed, just leave the pin
            // Was causing flickering before
            if (_nearbyPin != null)
                RemoveNearbyPin();

            Location? vert = MapUtils.GetClosestVertexOnPath(Locations, clickLoc, _map.MapView);
            if (vert == null)
                return;

            // Don't add the point if it's on the selection line.
            if(_selectionLine != null)
            {
                foreach(Location l in _selectionLine.Locations)
                    if (l == vert)
                        return;
            }

            // If the point is close enough, show the nearby pin
            double dist = MapUtils.Dist(clickLoc, _map.MapView.LocationToViewportPoint(vert));
            if (dist < 20)
            {
                if (_selectionPin == null || _selectionPin.Location != vert)
                    CreateNearbyPin(vert);
            }
        }

        #endregion Nearby Pin




        #region Path Style


        public static readonly DependencyProperty OverrideStrokeThicknessProperty = DependencyProperty.Register("OverrideStrokeThickness", typeof(double?), typeof(MapPath),
            new PropertyMetadata(null, StrokeThicknessPropertyChanged));

        /// <summary>
        /// If null, then the default thickness will be used. If not, use this.
        /// </summary>
        public double? OverrideStrokeThickness 
        {
            get { return (double?)GetValue(OverrideStrokeThicknessProperty); }
            set { SetValue(OverrideStrokeThicknessProperty, value); }
        }

        private static void StrokeThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapPath path = (MapPath)d;
            path.StrokeThicknessChanged();
        }

        private void StrokeThicknessChanged()
        {
            if (OverrideStrokeThickness != null)
                PathLine.StrokeThickness = (double)OverrideStrokeThickness;
            else
                PathLine.StrokeThickness = _defaultThickness;
        }

        private int _defaultThickness = 5;




        public static readonly DependencyProperty OverridePathColorProperty = DependencyProperty.Register("OverridePathColor", typeof(Color?), typeof(MapPath),
            new PropertyMetadata(null, PathColorPropertyChanged));

        /// <summary>
        /// If null, then the default color will be used. If not, use this.
        /// </summary>
        public Color? PathColor
        {
            get { return (Color?)GetValue(OverridePathColorProperty); }
            set { SetValue(OverridePathColorProperty, value); }
        }

        private static void PathColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapPath path = (MapPath)d;
            path.PathColorChanged();
        }

        private void PathColorChanged()
        {
            if (PathColor != null)
                OriginalColorBrush = new SolidColorBrush((Color)PathColor);
            else
                OriginalColorBrush = new SolidColorBrush(DefaultColor);

            if(!PreviewOpen)
                PathLine.Stroke = OriginalColorBrush;
        }

        public Color DefaultColor = Colors.Red;
        public SolidColorBrush OriginalColorBrush = new SolidColorBrush(Colors.Red);
        public SolidColorBrush FadedColorBrush = new SolidColorBrush(Colors.DarkRed);
        public SolidColorBrush SelectionColorBrush = new SolidColorBrush(Colors.Blue);


        protected override void FadedColorChanged()
        {
            if(!PreviewOpen)
            {
                if (FadedColor)
                    PathLine.Stroke = FadedColorBrush;
                else
                    PathLine.Stroke = OriginalColorBrush;
            }
        }

        #endregion Path Style




        #region Methods

        protected override void OpenPreview()
        {
            if(_preview == null)
            {
                MapItemViewModel vm = (MapItemViewModel)DataContext;

                // Get the specified type for the Preview box, and instanciate it
                Type? PreviewType = vm.PreviewType;
                if (PreviewType == null)
                    return;

                _preview = (UserControl?)Activator.CreateInstance(PreviewType);
                if (_preview == null)
                    return;

                _preview.DataContext = DataContext;
                _map.PreviewLayer_Add(_preview);

                if(Locations != null && Locations.Count > 0)
                {
                    MapLayer.SetPositionOffset(_preview, new Point(-_preview.Width / 2, -_preview.Height - 20));

                    Location previewLoc = MapUtils.GetClosestLocationOnPath(Locations, _lastPathClickPos, _map.MapView);
                    MapLayer.SetPosition(_preview, previewLoc);
                }

                _map.LineLayer_Remove(PathLine);
                PathLine.Stroke = SelectionColorBrush;
                _map.SelectedLineLayer_Add(PathLine);
            }
        }

        protected override void ClosePreview()
        {
            if(_preview != null)
            {
                if(!EditMode)
                {
                    _map.SelectedLineLayer_Remove(PathLine);
                    _map.LineLayer_Add(PathLine);
                }

                if (FadedColor)
                    PathLine.Stroke = FadedColorBrush;
                else
                    PathLine.Stroke = OriginalColorBrush;
            }

            base.ClosePreview();
        }


        protected override void EditModeChanged()
        {
            base.EditModeChanged();
            
            // Show the path line above all other lines
            if(EditMode)
            {
                if(!PreviewOpen)
                {
                    _map.LineLayer_Remove(PathLine);
                    _map.SelectedLineLayer_Add(PathLine);
                }
            }

            // If leaving edit mode, remove all edit-mode related components
            else
            {
                RemoveSelectionLine();
                RemoveSelectionPin();
                RemoveNearbyPin();

                if(!PreviewOpen)
                {
                    _map.SelectedLineLayer_Remove(PathLine);
                    _map.LineLayer_Add(PathLine);
                }
            }
        }


        #endregion Methods



        #region Mouse Handling


        /**
         * Keeps track of the last place the user clicked on the path Polyline.
         * This lets the popup window be opened at that position.
         */
        private Point _lastPathClickPos;

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
                RemoveNearbyPin();
            }
            // If in edit mode, clicking on the path will insert a point on the path
            else if(EditMode)
            {
                if(_nearbyPin == null)
                {
                    ((MapPathViewModel)DataContext).ClearSelection();
                    Location l = _map.MapView.ViewportPointToLocation(e.GetPosition(_map.MapView));
                    ((MapPathViewModel)DataContext).InsertPointAt(l, false);
                    ((MapPathViewModel)DataContext).SelectPoint(l);
                    RebuildNearbyPin(e.GetPosition(_map.MapView));
                }
            }
            // If not in edit mode, clicking on the path will open the preview box
            else
            {
                // Tell the MapViewModel to open the preview box
                MapViewModel vm = (MapViewModel)_map.DataContext;
                _lastPathClickPos = e.GetPosition(_map.MapView);
                vm.MapItemClick(DataContext);
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

            ((MapPathViewModel)DataContext).InsertPointAt(_map.MapView.ViewportPointToLocation(e.GetPosition(_map.MapView)), true);
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
                RebuildNearbyPin(e.GetPosition(_map.MapView));

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
            Point pos = e.GetPosition(_map.MapView);
            if (pos == _lastMousePos)
                return;

            // Don't show the Nearby Pin if any of the pins/paths are been clicked or dragged
            if (_selectionLineClick == false && _selectionPinClick == false && _nearbyPinClick == false && EditMode)
            {
                RebuildNearbyPin(e.GetPosition(_map.MapView));
            }
            else if (_nearbyPinClick == false)
                RemoveNearbyPin();

            _lastMousePos = e.GetPosition(_map.MapView);
        }


        #endregion Mouse Handling
    }
}
