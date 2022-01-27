using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PhotoGalleryApp.Views
{
    public static class ScrollStoppedBehavior
    {

        private static Dictionary<ScrollViewer, DispatcherTimer> timers = new Dictionary<ScrollViewer, DispatcherTimer>();


        public static bool GetDetectScrollStopped(DependencyObject o)
        {
            return (bool)o.GetValue(DetectScrollStoppedProperty);
        }

        public static void SetDetectScrollStopped(DependencyObject o, bool value)
        {
            o.SetValue(DetectScrollStoppedProperty, value);
        }

        public static readonly DependencyProperty DetectScrollStoppedProperty = DependencyProperty.RegisterAttached("DetectScrollStopped", typeof(bool), typeof(ScrollStoppedBehavior), new PropertyMetadata(false, DetectScrollStoppedChanged));

        private static void DetectScrollStoppedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if(viewer != null && (bool)args.NewValue)
            {
                viewer.ScrollChanged += Viewer_ScrollChanged;
                viewer.Unloaded += Viewer_Unloaded;
            }
        }



        public static readonly RoutedEvent ScrollStoppedEvent = EventManager.RegisterRoutedEvent("ScrollStopped", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScrollStoppedBehavior));
        public static void AddScrollStoppedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ScrollViewer viewer = d as ScrollViewer;
            if (viewer != null)
            {
                viewer.AddHandler(ScrollStoppedEvent, handler);
            }

        }


        public static void RemoveScrollStoppedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ScrollViewer viewer = d as ScrollViewer;
            if (viewer != null)
                viewer.RemoveHandler(ScrollStoppedEvent, handler);
        }


        private static void Viewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;

            RoutedEventArgs args = new RoutedEventArgs
            {
                RoutedEvent = ScrollStoppedEvent
            };
            if (viewer != null)
            {
                DispatcherTimer timer;
                if(timers.TryGetValue(viewer, out timer))
                {
                    if (timer.IsEnabled)
                        timer.Stop();
                    timer.Start();
                }
                else
                {
                    timer = new DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 150);
                    timer.Tick += (s, eventArgs) => { TimerComplete(viewer); };

                    timers.Add(viewer, timer);
                    timer.Start();
                }
            }
        }

        private static void TimerComplete(ScrollViewer viewer)
        {
            DispatcherTimer timer;
            if (timers.TryGetValue(viewer, out timer))
            {
                timer.Stop();
            }
            timers.Remove(viewer);
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ScrollStoppedEvent;
            viewer.RaiseEvent(args);

        }


        private static void Viewer_Unloaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (viewer != null)
            {
                viewer.ScrollChanged -= Viewer_ScrollChanged;
                viewer.Unloaded -= Viewer_Unloaded;
            }   
        }
    }
}
