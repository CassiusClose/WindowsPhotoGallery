using PhotoGalleryApp.Views.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Stores a collection of items to be displayed on a map
    /// </summary>
    [CollectionDataContract]
    public class Map : ObservableCollection<MapItem>
    {
        public Map()
        {

        }
    }
}
