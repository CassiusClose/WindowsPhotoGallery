using Microsoft.Maps.MapControl.WPF;
using Microsoft.Xaml.Behaviors;
using System;
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
    /// A Behavior class to support Left Click functionality. Kind of suprising that controls will
    /// support double click but not single click.
    /// </summary>
    public class LeftButtonClickBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += Obj_MouseDown;
            AssociatedObject.MouseUp += Obj_MouseUp;
            AssociatedObject.MouseMove += Obj_MouseMove;
        }

        /// <summary>
        /// Event triggered when the control is clicked on with the left mouse button. Perhaps not the best way to do this,
        /// as I'm not sure you can hook into this without accessing the behavior in the code-behind.
        /// </summary>
        public MouseButtonEventHandler? MouseLeftButtonClick = null;


        // If a potential click has been started (mouse down)
        private bool _clickStarted = false;

        // The original location of the mouse when clicked
        private Point _clickPoint;
        
        // Whether the mouse has moved since the click started
        private bool _drag = false;


        /**
         * Potential click has started, save mouse location
         */
        private void Obj_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _clickPoint = e.GetPosition(AssociatedObject);
                _clickStarted = true;
                _drag = false;
                e.Handled = true;
            }
        }

        /**
         * If the mouse moves during a click, then it's actually a drag
         */
        private void Obj_MouseMove(object sender, MouseEventArgs e)
        {
            if(_clickStarted)
            {
                Point mousePos = e.GetPosition(AssociatedObject);
                if (Math.Abs(mousePos.X - _clickPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(mousePos.Y - _clickPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _drag = true;
                }
            }
        }

        /**
         * If the mouse releases & hasn't dragged, then it's a click
         */
        private void Obj_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_clickStarted)
            {
                Point mousePos = e.GetPosition(AssociatedObject);
                if (Math.Abs(mousePos.X - _clickPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(mousePos.Y - _clickPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                {
                    if (!_drag && MouseLeftButtonClick != null)
                        MouseLeftButtonClick.Invoke(sender, e);
                }

                _clickStarted = false;
                _drag = false;
            }
        }
    }
}
