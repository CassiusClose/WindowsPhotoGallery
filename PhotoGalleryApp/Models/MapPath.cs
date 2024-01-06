using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Information about some movement on the map. Stores an ordered set of locations.
    /// </summary>
    public class MapPath : MapItem
    {
        public MapPath(string name)
        {
            _points = new LocationCollection();
            _name = name;
        }

        private string _name;
        public string Name { 
            get { return _name; } 
            set { _name = value; }
        }


        private LocationCollection _points;
        public LocationCollection Points { 
            get { return _points; } 
            set { _points = value; }
        }
    }
}
