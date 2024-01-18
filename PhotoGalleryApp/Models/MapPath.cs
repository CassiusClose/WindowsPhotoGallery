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
        // Parameterless constructor for XML deserialization
        private MapPath() : base("") {}

        public MapPath(string name) : base(name)
        {
            _points = new LocationCollection();
        }


        private LocationCollection _points = new LocationCollection();
        public LocationCollection Points { 
            get { return _points; } 
            set { _points = value; }
        }
    }
}
