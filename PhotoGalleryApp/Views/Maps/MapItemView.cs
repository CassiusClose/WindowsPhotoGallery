using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Xaml.Behaviors;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.ViewModels;
using PhotoGalleryApp.Views.Behavior;

namespace PhotoGalleryApp.Views.Maps
{
    /// <summary>
    /// Abstract code-behind for an item that can be placed on the map (paths,
    /// locations). This control is never actually added to the graphical tree,
    /// it just hooks into the Map view and adds items to its layers.
    /// 
    /// Users must call Init() for this control to work.
    /// 
    /// Each item has an edit mode, where its details can be
    /// changed (pushpins moved around, etc.), and a preview popup box that can
    /// be opened by clicking on the item.
    /// </summary>
    public abstract partial class MapItemView : UserControl
    {
        public MapItemView() { }

        public virtual void Init(Map map, MapItemViewModel context)
        {
            DataContext = context;

            _map = map;
            _map.MouseLeftButtonClick += Map_Click;

            BindingOperations.SetBinding(this, EditModeProperty, new Binding("EditMode"));
            BindingOperations.SetBinding(this, PreviewOpenProperty, new Binding("PreviewOpen"));

            Init_MainMapItem();

            MapItemClickDragBehavior b = new MapItemClickDragBehavior(_map.MapView);
            b.MouseLeftButtonClick += MainMapItem_Click;
            b.MouseDrag += MainMapItem_Drag;
            Interaction.GetBehaviors(GetMainMapItem()).Add(b);
        }



        /**
         * The main map item is the control which represents the object. For a Location, this is the pushpin. For a path,
         * it's the polyline.
         */
        protected abstract void Init_MainMapItem();
        protected abstract UIElement GetMainMapItem();

        

        protected Map _map;

        /**
         * MapLayers from the parent map
         */
        protected MapLayer PreviewLayer { get { return _map.PreviewLayer; } }
        protected MapLayer LineLayer { get { return _map.LineLayer; } }
        protected MapLayer PinLayer { get { return _map.PinLayer; } }


        #region Edit Mode Property

        public static readonly DependencyProperty EditModeProperty = DependencyProperty.RegisterAttached("EditModeProperty", typeof(bool), typeof(MapItemView),
            new PropertyMetadata(EditModePropertyChanged));

        /**
         * Whether the map item is currently editable (i.e. pushpins can be moved around, etc.)
         */
        protected bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }


        private static void EditModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapItemView control = (MapItemView)sender;
            control.EditModeChanged();
        }

        protected virtual void EditModeChanged() { }

        #endregion



        #region Preview Open Property

        public static readonly DependencyProperty PreviewOpenProperty = DependencyProperty.RegisterAttached("PreviewOpen", typeof(bool), typeof(MapItemView),
            new PropertyMetadata(PreviewOpenPropertyChanged));

        /// <summary>
        /// Whether the preview popup box for the item is open or not.
        /// </summary>
        protected bool PreviewOpen
        {
            get { return (bool)GetValue(PreviewOpenProperty); }
            set { 
                SetValue(PreviewOpenProperty, value);
                if (value == true)
                    OpenPreview();
                else
                    ClosePreview();
            }
        }


        protected static void PreviewOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MapItemView control = (MapItemView)sender;
            if (control.PreviewOpen)
                control.OpenPreview();
            else
                control.ClosePreview();
        }


        // Save the preview box when opened to be able to close it
        protected UserControl? _preview = null;


        protected abstract void OpenPreview();
        protected void ClosePreview()
        {
            if (_preview != null)
            {
                PreviewLayer.Children.Remove(_preview);
                _preview = null;
            }
        }

        #endregion Preview Open Property



        /**
         * Called when the map background is left button clicked
         */
        protected virtual void Map_Click(object sender, MouseEventArgs e) {}

        /**
         * Called when the main item is left button clicked
         */
        protected virtual void MainMapItem_Click(object sender, MouseEventArgs e) {}

        /**
         * Called when the main item is left button dragged
         */
        protected virtual void MainMapItem_Drag(object sender, MouseDragEventArgs e) {}
    }
}

