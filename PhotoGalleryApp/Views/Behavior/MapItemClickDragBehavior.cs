using Microsoft.Maps.MapControl.WPF;
using Microsoft.Xaml.Behaviors;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.Views.Maps;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PhotoGalleryApp.Views.Behavior
{
    /// <summary>
    /// A Behavior class to support both Left Click and Left Drag
    /// functionality, specifically for MapItems. A drag is a click with the
    /// mouse moving.
    /// </summary>
    public class MapItemClickDragBehavior : Behavior<UIElement>
    {
        public MapItemClickDragBehavior(Microsoft.Maps.MapControl.WPF.Map map)
        {
            _map = map;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            //TODO Remove listeners in destructor?
            AssociatedObject.MouseDown += Obj_MouseDown;
            _map.MouseMove += Map_MouseMove;
            _map.MouseUp += Map_MouseUp;
        }

        private Microsoft.Maps.MapControl.WPF.Map _map;

        public delegate void MouseDragEventHandler(object sender, MouseDragEventArgs e);

        /// <summary>
        /// Event called when the user clicks on the control with the left mouse button
        /// </summary>
        public MouseButtonEventHandler? MouseLeftButtonClick = null;

        /// <summary>
        /// Event called every time the user moves the mouse while dragging the control
        /// </summary>
        public MouseDragEventHandler? MouseDrag = null;


        private bool _drag = false;
        private bool _clickStarted = false;
        Point _clickPoint;
        Location _lastMousePos;

        private void Obj_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _clickStarted = true;
                _drag = false;

                _lastMousePos = _map.ViewportPointToLocation(e.GetPosition(_map));

                _clickPoint = e.GetPosition(_map);
                
                e.Handled = true;
            }
        }

        private void Map_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(_clickStarted && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(_map);
                if(!_drag)
                {
                    // It's only a drag if the mouse moves a certain amount
                    if (Math.Abs(mousePos.X - _clickPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(mousePos.Y - _clickPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        _drag = true;
                    }
                }

                if(_drag)
                {
                    if(MouseDrag != null)
                    {
                        // Calculate how far the mouse has moved since the last MouseMove event
                        Location mouseLoc = _map.ViewportPointToLocation(e.GetPosition(_map));
                        double latdiff = mouseLoc.Latitude - _lastMousePos.Latitude;
                        double longdiff = mouseLoc.Longitude - _lastMousePos.Longitude;
                        _lastMousePos = mouseLoc;


                        if(MouseDrag != null)
                        {
                            MouseDragEventArgs args = new MouseDragEventArgs(latdiff, longdiff);
                            MouseDrag.Invoke(AssociatedObject, args);
                        }
                    }

                }
            }
        }

        private void Map_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(_clickStarted)
            {
                // Only click if not drag
                if(!_drag)
                {
                    if (MouseLeftButtonClick != null)
                        MouseLeftButtonClick.Invoke(AssociatedObject, e);
                }

                _drag = false;
                _clickStarted = false;
            }
        }
    }



    /// <summary>
    /// Arguments for the MouseDrag event. Parameters are the distances in
    /// latitude and longitude that the user moved the mouse since the last
    /// MouseMove event.
    /// </summary>
    public class MouseDragEventArgs
    {
        public MouseDragEventArgs(double latDiff, double longDiff)
        {
            LatitudeDifference = latDiff;
            LongitudeDifference = longDiff;
        }

        public double LatitudeDifference
        {
            get; internal set;
        }

        public double LongitudeDifference
        {
            get; internal set;
        }
    }
}
