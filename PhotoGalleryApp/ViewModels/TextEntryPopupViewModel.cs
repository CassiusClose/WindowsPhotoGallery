using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for a popup where the user enters a string of text.
    /// </summary>
    public class TextEntryPopupViewModel : PopupViewModel
    {
        public override void Cleanup() { }


        private string _text="";
        /// <summary>
        /// The text in the textbox
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        protected override bool ValidateData()
        {
            if(string.IsNullOrWhiteSpace(Text))
            {
                ValidationErrorText = "Text entry cannot be empty";    
                return false;
            }

            ValidationErrorText = "";
            return true;
        }


        public override PopupReturnArgs GetPopupResults()
        {
            return new TextEntryPopupReturnArgs(Text);
        }
    }


    /// <summary>
    /// Stores data returned from the text entry popup. Keeps track of whether the
    /// user entered text or cancelled.
    /// </summary>
    public class TextEntryPopupReturnArgs : PopupReturnArgs
    {
        public TextEntryPopupReturnArgs(string? text)
        {
            Text = text;
        }

        public string? Text = null;
    }
}
