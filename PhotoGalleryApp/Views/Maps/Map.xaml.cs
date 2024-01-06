using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Schema;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Core;
using Microsoft.Xaml.Behaviors;
using PhotoGalleryApp.ViewModels;
using PhotoGalleryApp.Views.Behavior;
using PhotoGalleryApp.Views.Maps;

namespace PhotoGalleryApp.Views.Maps
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

            MapView.Loaded += MapView_Loaded;

        }

        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MapView.Height = ControlContainer.Height;

            _clickBehavior.MouseLeftButtonClick += MapView_Click;
            Interaction.GetBehaviors(MapView).Add(_clickBehavior);


            // If a polyline exists in the ItemsSource when the control is created, it will not
            // be displayed until the map coordinates are recalculated (by zooming, panning, etc.)
            // According to the following link, this is a bug with BingMaps' Map control. So once the
            // map is fully loaded, add and remove a polyline to force recalculation.
            // https://stackoverflow.com/questions/10950421/wpf-bing-maps-control-polylines-polygons-not-draw-on-first-add-to-collection 
            MapPolyline mapPolyline = new MapPolyline();
            MapView.Children.Add(mapPolyline);
            MapView.Children.Remove(mapPolyline);
        }




        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ICollectionView), typeof(Map),
            new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = ItemsSourceChanged
            }
        );

        /// <summary>
        /// Items to display on the map (paths & locations). These are MapItemViewModel objects.
        /// </summary>
        public ICollectionView ItemsSource
        {
            get { return (ICollectionView)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /**
         * Maintains a mapping between MapItemViewModels and their associated control
         */
        private Dictionary<MapItemViewModel, MapItemView> _items = new Dictionary<MapItemViewModel, MapItemView>();



        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Map m = (Map)d;
            if(m.ItemsSource != null)
            {
                m.ItemsSource.CollectionChanged += m.ItemsSource_CollectionChanged;
            }
            m.RefreshItems();
        }

        private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshItems();
        }



        private void RefreshItems()
        {
            if (ItemsSource == null)
                return;

            foreach(MapItemViewModel vm in ItemsSource)
            {
                Trace.WriteLine("Adding");
                Trace.WriteLine(vm);
                MapItemView? o;
                // If control doesn't exist, create it
                if(!_items.TryGetValue(vm, out o))
                {
                    if(vm is MapLocationViewModel)
                    {
                        o = new MapLocation
                        {
                            DataContext = vm
                        };
                        o.Init(this, vm);
                        MapView.Children.Add(o);
                        _items.Add(vm, o);
                    }
                    else if (vm is MapPathViewModel)
                    {
                        o = new MapPath();
                        o.Init(this, vm);
                        MapView.Children.Add(o);
                        _items.Add(vm, o);

                    }
                }
            }

            // If a VM was removed from the list, remove its control
            foreach(MapItemViewModel vm in _items.Keys)
            {
                if (!ItemsSource.Contains(vm))
                {
                    MapView.Children.Remove(_items[vm]);
                    _items.Remove(vm);
                }
            }
        }




        /**
         * Enable left click on the map canvas
         */
        private LeftButtonClickBehavior _clickBehavior = new LeftButtonClickBehavior();

        /**
         * Expose the event so that children of the map can hook into the click (children of the map should
         * have transparent backgrounds that don't detect clicks)
         */
        public MouseEventHandler? MouseLeftButtonClick
        {
            get { return _clickBehavior.MouseLeftButtonClick; }
            set { _clickBehavior.MouseLeftButtonClick += value; }
        }  

        private void MapView_Click(object sender, MouseEventArgs e)
        {
            MapViewModel vm = (MapViewModel)DataContext;
            vm.MapClickEvent();
        }

        /**
         * Prevent double click from zooming
         */
        private void MapView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
