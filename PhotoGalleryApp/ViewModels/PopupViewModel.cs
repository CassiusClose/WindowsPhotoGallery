using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// Any popup window needs to implement this, to handle the transfer of data from the
    /// popup to the opener of it.
    /// </summary>
    public abstract class PopupViewModel : ViewModelBase
    {
        public abstract object GetPopupResults();
    }
}
