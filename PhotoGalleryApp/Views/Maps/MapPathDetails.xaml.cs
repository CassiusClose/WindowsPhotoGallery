using System;
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
    /// Interaction logic for MapPathDetails.xaml
    /// </summary>
    public partial class MapPathDetails : UserControl
    {
        public MapPathDetails()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)CoordinateList.Items).CollectionChanged += MapPathDetails_CollectionChanged;
        }

        private void MapPathDetails_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Trace.WriteLine("Coll Changed");
            /*foreach(var item in CoordinateList.Items)
            {
                if (ReferenceEquals(item, lastSel))
                {
                    CoordinateList.SelectedItem = item;
                    break;
                }
            }*/
            CoordinateList.SelectedItem = lastSel;

            Trace.WriteLine(CoordinateList.SelectedItem);
        }

        private object lastSel;

        private void CoordinateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Trace.WriteLine("Sel Changed");
            if(e.RemovedItems != null && e.RemovedItems.Count > 0 && (e.AddedItems == null || e.AddedItems.Count == 0))
                lastSel = e.RemovedItems[0];
            Trace.WriteLine(lastSel);
        }
    }
}
