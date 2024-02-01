using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the page displaying a MapPath.
    /// </summary>
    public class PathPageViewModel : MapItemPageViewModel
    {
        public PathPageViewModel(MapPath item) : base(item) { }

        public override void Cleanup() { }
    }
}
