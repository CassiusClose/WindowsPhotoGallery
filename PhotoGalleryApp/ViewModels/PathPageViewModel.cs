using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the page displaying a MapPath.
    /// </summary>
    public class PathPageViewModel : ViewModelBase
    {
        public PathPageViewModel(NavigatorViewModel nav, MapPath path)
        {
            _nav = nav;
            _mapPath = path;
        }

        public override void Cleanup() { }


        private NavigatorViewModel _nav;
        private MapPath _mapPath;


        public string Name
        {
            get { return _mapPath.Name; }
        }
    }
}
