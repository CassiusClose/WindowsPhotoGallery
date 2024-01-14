using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /**
     * ViewModel for the Popup Window to create a new MapPath.
     */
    public class CreatePathPopupViewModel : PopupViewModel
    {
        public CreatePathPopupViewModel() 
        {
            _clearFilenameCommand = new RelayCommand(ClearFilename);
        }

        public override void Cleanup() {}


        private string _name = "";
        /// <summary>
        /// The name of the path
        /// </summary>
        public string Name {  
            get { return _name; } 
            set { _name = value; OnPropertyChanged(); }
        }


        private string _filename = "";
        /// <summary>
        /// The file that should be used to load path data. If empty, no file should be loaded.
        /// </summary>
        public string Filename
        {
            get { return _filename; }
            set { 
                _filename = value; 
                OnPropertyChanged();
            }
        }


        public override PopupReturnArgs GetPopupResults()
        {
            return new CreatePathPopupReturnArgs(Name, Filename);
        }

        protected override bool ValidateData()
        {
            if(string.IsNullOrWhiteSpace(Name))
            {
                ValidationErrorText = "Name field cannot be empty";
                return false;
            }

            //TODO Check if file is valid format

            ValidationErrorText = "";
            return true;
        }

        private RelayCommand _clearFilenameCommand;
        public ICommand ClearFilenameCommand => _clearFilenameCommand;

        /// <summary>
        /// If a path data file was chosen, unchoose it
        /// </summary>
        public void ClearFilename()
        {
            Filename = "";
        }
    }

    public class CreatePathPopupReturnArgs : PopupReturnArgs
    {
        public CreatePathPopupReturnArgs(string name, string filename="")
        {
            Name = name;
            Filename = filename;
        }

        // The name of the path
        public string Name = "";

        // The file that contains path data
        public string Filename;

        // Whether or not to use a file to load path data
        public bool LoadFromFile
        {
            get { return !string.IsNullOrWhiteSpace(Filename); }
        }
    }
}
