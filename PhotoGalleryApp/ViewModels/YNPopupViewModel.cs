using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A popup that displays only a string and the Accept/Cancel buttons. Use for yes/no input.
    /// </summary>
    public class YNPopupViewModel : PopupViewModel
    {
        public YNPopupViewModel(string message = "") { _message = message; }

        public override void Cleanup() {}



        private string _message;
        /// <summary>
        /// The message displayed in the popup
        /// </summary>
        public string Message
        {
            get { return _message; }
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
