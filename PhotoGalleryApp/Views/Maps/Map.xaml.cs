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

            SetBinding(ItemsSourceProperty, "MapItems");
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

            // Connect map layers to the code-behind collections
            PinLayer.ItemsSource = _pins;
            LineLayer.ItemsSource = _lines;
            SelectedLineLayer.ItemsSource = _selectedLines;
            SelectedPinLayer.ItemsSource = _selectedPins;
            PreviewLayer.ItemsSource = _previews;
        }



        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<MapItemViewModel>), typeof(Map),
            new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = ItemsSourceChanged
            }
        );

        /// <summary>
        /// Items to display on the map (paths & locations). These are MapItemViewModel objects.
        /// </summary>
        public ObservableCollection<MapItemViewModel> ItemsSource
        {
            get { return (ObservableCollection<MapItemViewModel>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /**
         * Maintains a mapping between MapItemViewModels and their associated control
         */
        private Dictionary<MapItemViewModel, MapItemView> _items = new Dictionary<MapItemViewModel, MapItemView>();



        #region ItemsSource Changed

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
                MapItemView? o;
                // If control doesn't exist, create it
                if(!_items.TryGetValue(vm, out o))
                {
                    if(vm.GetModel() is PhotoGalleryApp.Models.MapLocation)
                    {
                        o = new MapLocation();
                        o.Init(this, vm);
                        _items.Add(vm, o);
                    }
                    else if (vm.GetModel() is PhotoGalleryApp.Models.MapPath)
                    {
                        o = new MapPath();
                        o.Init(this, vm);
                        _items.Add(vm, o);
                    }
                }
            }

            // If a VM was removed from the list, remove its control
            foreach(MapItemViewModel vm in _items.Keys)
            {
                if (!ItemsSource.Contains(vm))
                {
                    _items[vm].RemoveAll();
                    _items.Remove(vm);
                }
            }
        }

        #endregion ItemsSource Changed



        /**
         * Prompt the user to pick a file that contains path data.
         */
        private void LoadFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text documents (.txt)|*.txt";
            dialog.Multiselect = false;

            bool? result = dialog.ShowDialog();

            // If the user picked a file
            if(result != null && result == true)
            {
                string filename = dialog.FileName;

                MapViewModel vm = (MapViewModel)DataContext;
                vm.LoadPathsFromFile(filename);
            }
        }




        #region Map Layer Collections/Functions

        /**
         * Each collection is attached to a MapItemsControl's ItemSource. This
         * lets the code-behind control what goes into each layer, and,
         * importantly, the order of the items, which determines what is drawn
         * above what.
         */
        private ObservableCollection<UIElement> _pins = new ObservableCollection<UIElement>();
        private ObservableCollection<UIElement> _lines = new ObservableCollection<UIElement>();
        private ObservableCollection<UIElement> _selectedPins = new ObservableCollection<UIElement>();
        private ObservableCollection<UIElement> _selectedLines = new ObservableCollection<UIElement>();
        private ObservableCollection<UIElement> _previews = new ObservableCollection<UIElement>();


        /** 
         * Compares Pushpins by Latitude, so southern Pushpins are shown above
         * northern ones
         */
        private int PushpinCompare(UIElement a, UIElement b)
        {
            if (a is not Pushpin || b is not Pushpin)
                return 1;

            Pushpin p1 = (Pushpin)a;
            Pushpin p2 = (Pushpin)b;

            if (p1.Location.Latitude > p2.Location.Latitude)
                return -1;

            if (p1.Location.Latitude < p2.Location.Latitude)
                return 1;

            return 0;
        }



        public void LineLayer_Add(UIElement item) { AddMapItemToLayer(_lines, item, null); }
        public void LineLayer_Remove(UIElement item) { _lines.Remove(item); }

        public void PinLayer_Add(UIElement item) { AddMapItemToLayer(_pins, item, PushpinCompare); }
        public void PinLayer_Remove(UIElement item) { _pins.Remove(item); }
        public void PinLayer_Update(UIElement item) { UpdateMapItemInLayer(_pins, item); }

        public void SelectedLineLayer_Add(UIElement item) { AddMapItemToLayer(_selectedLines, item, null); }
        public void SelectedLineLayer_Remove(UIElement item) { _selectedLines.Remove(item); }

        public void SelectedPinLayer_Add(UIElement item) { AddMapItemToLayer(_selectedPins, item, PushpinCompare); }
        public void SelectedPinLayer_Remove(UIElement item) { _selectedPins.Remove(item); }
        public void SelectedPinLayer_Update(UIElement item) { UpdateMapItemInLayer(_selectedPins, item); }

        public void PreviewLayer_Add(UIElement item) { AddMapItemToLayer(_previews, item, null); }
        public void PreviewLayer_Remove(UIElement item) { _previews.Remove(item); }


        /**
         * Adds the given UIElement to the given collection, using the given
         * sorting criteria
         */
        private void AddMapItemToLayer(ObservableCollection<UIElement> list, UIElement item, Comparison<UIElement>? comparison)
        {
            if(comparison == null)
            {
                list.Add(item);
                return;
            }

            for(int i = 0; i < list.Count; i++)
            {
                if(comparison(item, list[i]) <= 0)
                {
                    list.Insert(i, item);
                    return;
                }
            }
            list.Add(item);
        }

        /**
         * Triggers a UI update for the given UIElement in the given
         * collection. This is used for Pushpins, who do not redraw the Map on
         * Location changed, when they're part of a MapItemsControl.
         */
        public void UpdateMapItemInLayer(ObservableCollection<UIElement> list, UIElement item)
        {
            int i = list.IndexOf(item);
            if (i != -1)
            {
                list[i] = null;
                list[i] = item;
            }
        }


        #endregion Map Layer Collections/Functions



        #region Clicks


        /**
         * Enable left click on the map canvas
         */
        private LeftButtonClickBehavior _clickBehavior = new LeftButtonClickBehavior();

        /**
         * Expose the event so that children of the map can hook into the click (children of the map should
         * have transparent backgrounds that don't detect clicks)
         */
        public MouseButtonEventHandler? MouseLeftButtonClick
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
        
        #endregion Clicks
    }
}
