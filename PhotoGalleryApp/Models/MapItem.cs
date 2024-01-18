using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    public abstract class MapItem : NotifyPropertyChanged
    {
        public MapItem(string name)
        {
            _name = name;
        }

        protected string _name;
        public string Name { 
            get { return _name; } 
            set { 
                _name = value;
                OnPropertyChanged();
            }
        }
    }
}
