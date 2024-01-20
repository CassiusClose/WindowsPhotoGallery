using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    [KnownType(typeof(MapLocation))]
    [KnownType(typeof(MapPath))]
    [DataContract(IsReference = true)]
    public abstract class MapItem : NotifyPropertyChanged
    {
        public MapItem(string name)
        {
            _name = name;
        }

        protected string _name;
        [DataMember]
        public string Name { 
            get { return _name; } 
            set { 
                _name = value;
                OnPropertyChanged();
            }
        }
    }
}
