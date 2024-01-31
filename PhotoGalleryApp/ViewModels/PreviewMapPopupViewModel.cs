using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// Popup box to show a map, with no toolbar or previews.
    /// </summary>
    class PreviewMapPopupViewModel : PopupViewModel
    {
        public PreviewMapPopupViewModel(Map map) : base(false)
        {
            Map = new MapViewModel(MainWindow.GetNavigator(), map, true, true, false);
        }

        public PreviewMapPopupViewModel() : this(MainWindow.GetCurrentSession().Map) { }


        public override void Cleanup() 
        {
            Map.Cleanup();
        }




        public MapViewModel Map
        {
            get; private set;
        }


        public override PopupReturnArgs GetPopupResults()
        {
            return new PopupReturnArgs();
        }

        protected override bool ValidateData()
        {
            return true;
        }
    }
}
