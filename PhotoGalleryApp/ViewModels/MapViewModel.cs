using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    class MapViewModel : ViewModelBase
    {
        public MapViewModel(NavigatorViewModel nav)
        {
            nav = _nav;
        }

        private NavigatorViewModel _nav;
    }
}
