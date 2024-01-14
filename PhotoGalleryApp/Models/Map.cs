using PhotoGalleryApp.Views.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Stores a collection of items to be displayed on a map
    /// </summary>
    [XmlInclude(typeof(MapPath))]
    [XmlInclude(typeof(MapLocation))]
    public class Map : ObservableCollection<MapItem>
    {
        public Map()
        {

        }
    }
}
