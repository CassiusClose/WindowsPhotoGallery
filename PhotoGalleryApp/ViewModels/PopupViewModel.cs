using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// Any popup window view model needs to implement this, to handle the
    /// transfer of data from the popup to the opener of it.
    /// </summary>
    public abstract class PopupViewModel : ViewModelBase
    {
        public PopupViewModel() 
        {
            _closePopupCommand = new RelayCommand(ClosePopup);
        }


        /**
         * Returns an object containing all the relevant data to the popup.
         */
        public abstract PopupReturnArgs GetPopupResults();



        private bool _open = true;
        /// <summary>
        /// Is the popup currently open
        /// </summary>
        public bool Open
        {
            get { return _open; }
            set
            {
                _open = value;
                OnPropertyChanged();
            }
        }

        private bool _accepted = false;
        /// <summary>
        /// Did the user accept or cancel the popup
        /// </summary>
        public bool Accepted
        {
            get { return _accepted; }
            set
            {
                _accepted = value;
                OnPropertyChanged();
            }
        }



        private RelayCommand _closePopupCommand;
        public ICommand ClosePopupCommand => _closePopupCommand;

        /**
         * Attempts to close the popup. The parameter should be a bool, and
         * should be true if the user pressed accept and false if the user
         * pressed cancel. If the user pressed accept, then only close the
         * popup if the data validates.
         */
        public void ClosePopup(object parameter)
        {
            if (parameter is not bool)
                throw new ArgumentException("Close Popup accepts a bool argument");

            bool acc = (bool)parameter;

            if (acc && !ValidateData())
                return;

            Accepted = acc;
            Open = false;
        }


        private string _validationErrorText = "";
        /// <summary>
        /// Displays errors with data validation, to tell the user why the
        /// popup didn't accept. Subclasses should set this from
        /// ValidateData().
        /// </summary>
        public string ValidationErrorText
        {
            get { return _validationErrorText; }
            set
            {
                _validationErrorText = value;
                OnPropertyChanged();
            }
        }

        /**
         * Should return true only if all the data in the popup is valid. For
         * example, if a TextEntry popup has no text entered, this should
         * return false if you don't want to allow an empty string.
         * Implementations of this method can set the ValidateErrorText
         * property to display error messages about what didn't pass
         * validation.
         */
        protected abstract bool ValidateData();
    }

    /// <summary>
    /// Stores data returned from a popup. Keeps track of whether the user
    /// cancelled or not.
    /// </summary>
    public class PopupReturnArgs
    {
        public bool PopupAccepted = false;
    }
}
