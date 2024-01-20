using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Information about some movement on the map. Stores an ordered set of locations.
    /// </summary>
    [DataContract]
    public class MapPath : MapItem
    {
        public MapPath(string name) : base(name)
        {
            _locations = new LocationCollection();
        }


        private LocationCollection _locations = new LocationCollection();
        public LocationCollection Locations { 
            get { return _locations; } 
            set { _locations = value; }
        }


        // Store specific properties from Location so only those are serialized
        [DataMember(Name=nameof(Locations))]
        private LocationCollectionSerializable _pointsSerializable
        {
            get { return new LocationCollectionSerializable(Locations); }
            set { Locations = value.GetLocationCollection(); }
        }
    }
}
