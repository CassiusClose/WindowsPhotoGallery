using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Core;
using PhotoGalleryApp.ViewModels;

namespace PhotoGalleryApp.Views
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl
    {
        public Map()
        {
            InitializeComponent();
            MapView.CredentialsProvider = new ApplicationIdCredentialsProvider(Keys.BingAPIKey);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MapView.Height = ControlContainer.Height;
        }


        public static readonly DependencyProperty LocationsSourceProperty = DependencyProperty.Register("LocationsSource", typeof(ICollectionView), typeof(Map),
            new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = LocationsSourceChanged
            }
        );
        public ICollectionView LocationsSource
        {
            get { return (ICollectionView)GetValue(LocationsSourceProperty); }
            set { SetValue(LocationsSourceProperty, value); }
        }


        private Dictionary<MapLocationViewModel, Pushpin> pushpins = new Dictionary<MapLocationViewModel, Pushpin>();
        private Dictionary<MapLocationViewModel, MapLocationPreview> pushpinPopups = new Dictionary<MapLocationViewModel, MapLocationPreview>();
        

        private static void LocationsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Map m = (Map)d;
            m.LocationsSource.CollectionChanged += m.LocationsSource_CollectionChanged;
            m.RefreshLocations();
        }

        private void LocationsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshLocations();
        }


        private void RefreshLocations()
        {
            foreach(MapLocationViewModel vm in LocationsSource)
            {
                Pushpin? p;
                if(!pushpins.TryGetValue(vm, out p))
                {
                    p = new Pushpin
                    {
                        Location = vm.Coordinates,
                        Content = vm.Name,
                        DataContext = vm
                    };
                    p.MouseLeftButtonDown += Pin_Clicked;
                    MapView.Children.Add(p);
                    pushpins.Add(vm, p);
                }

                MapLocationPreview? prev;
                if(!pushpinPopups.TryGetValue(vm, out prev))
                {
                    prev = new MapLocationPreview()
                    {
                        Visibility = Visibility.Hidden,
                        DataContext = vm
                    };
                    MapOverlay.Children.Add(prev);
                    pushpinPopups.Add(vm, prev);
                }    
            }
        }

        private void Pin_Clicked(object sender, RoutedEventArgs e)
        {
            if(openPreview != null)
            {
                openPreview.Visibility = Visibility.Hidden;
                openPreview = null;
            }

            Pushpin p = (Pushpin)sender;
            MapLocationViewModel vm = (MapLocationViewModel)p.DataContext;
            MapLocationPreview? prev;
            if (pushpinPopups.TryGetValue(vm, out prev))
            {
                prev.Visibility = Visibility.Visible;
                openPreview = prev;
                UpdatePopupLocation(prev);
            }
            else
                Trace.WriteLine("ERROR: Pin_Clicked in Map.xaml.cs");
        }




        private MapLocationPreview? openPreview = null;

        private void UpdatePopupLocation(MapLocationPreview? popup)
        {
            if (popup != null)
            {
                MapLocationViewModel vm = (MapLocationViewModel)popup.DataContext;
                Pushpin? pin;
                if(pushpins.TryGetValue(vm, out pin))
                {
                    Point p = MapView.LocationToViewportPoint(pin.Location);
                    p.X -= popup.Width / 2;
                    p.Y -= popup.Height + 50;

                    popup.SetValue(Canvas.LeftProperty, p.X);
                    popup.SetValue(Canvas.TopProperty, p.Y);
                }

            }
        }

        private void MapView_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            UpdatePopupLocation(openPreview);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePopupLocation(openPreview);
        }
    }
}
