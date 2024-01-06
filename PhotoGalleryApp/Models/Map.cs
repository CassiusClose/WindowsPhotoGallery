using PhotoGalleryApp.Views.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Stores a collection of items to be displayed on a map
    /// </summary>
    public class Map
    {
        public Map()
        {

        }

        private ObservableCollection<MapItem> _items = new ObservableCollection<MapItem>();
        public ObservableCollection<MapItem> Items { get {  return _items; } }
    }
}
