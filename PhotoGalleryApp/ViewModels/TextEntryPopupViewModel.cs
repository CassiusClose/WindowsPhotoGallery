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
        public TextEntryPopupViewModel()
        {
            _confirmTextCommand = new RelayCommand(ConfirmText);
        }


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
        
        /**
         * The text currently in the textbox is not neccessarily confirmed.
         * The user could cancel the action & there would still be text in the
         * textbox. So have this as null until the user explicitly calls the
         * ConfirmTextCommand by pressing the button or hitting enter.
         */
        private string? _confirmedText = null;



        private RelayCommand _confirmTextCommand;
        public ICommand ConfirmTextCommand => _confirmTextCommand;

        /// <summary>
        /// Accept the entered text as the result and close the popup
        /// </summary>
        public void ConfirmText()
        {
            _confirmedText = Text;
            ClosePopup();
        }




        public override object? GetPopupResults()
        {
            //TODO Check for empty string
            if (_confirmedText != null)
                return new TextEntryPopupReturnArgs(Text, TextEntryPopupReturnArgs.ReturnType.TextEntered);
            return new TextEntryPopupReturnArgs(TextEntryPopupReturnArgs.ReturnType.Cancelled);
        }
    }


    /// <summary>
    /// Stores data returned from the text entry popup. Keeps track of whether the
    /// user entered text or cancelled.
    /// </summary>
    public class TextEntryPopupReturnArgs
    {
        public TextEntryPopupReturnArgs(ReturnType returnType)
        {
            Action = returnType;
        }
        public TextEntryPopupReturnArgs(string text, ReturnType returnType)
        {
            Action = returnType;
            Text = text;
        }

        public enum ReturnType
        {
            TextEntered, Cancelled
        }

        public ReturnType Action;
        public string? Text = null;
    }
}
