using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// An abstract ViewModel representing one folder in a folder tree.
    /// Subclasses should provide the value for Label.
    ///
    /// The list of children is stored here, which means it's a list of generic
    /// FolderViewModels. It's less duplicate code to have the list defined
    /// once, but it does mean more casting in subclasses to get the members of
    /// the desired subclass type. Luckily, subclasses of FolderViewModel tend
    /// to be pretty self-contained, and it's not very hard to ensure all
    /// members of Children are the proper type.
    /// </summary>
    public abstract class FolderViewModel : ViewModelBase
    {
        public FolderViewModel()
        {
            _toggleExpandCommand = new RelayCommand(ToggleExpand);
        }

        /// <summary>
        /// The display name of this folder
        /// </summary>
        private string _label;
        public virtual string Label { 
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FolderViewModel> Children
        {
            get; protected set;
        }


        private bool _hasChildren;
        /// <summary>
        /// Whether the folder contains any child folders
        /// </summary>
        public bool HasChildren
        {
            get { return _hasChildren; }
            internal set
            {
                _hasChildren = value;
                if (_hasChildren == false)
                    IsExpanded = false;
                OnPropertyChanged();
            }
        }




        /// <summary>
        /// Whether the folder is expanded to show its children in the view
        /// </summary>
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }


        public delegate void FolderClickedDelegate(FolderViewModel folder);
        public FolderClickedDelegate? FolderClicked;
        /** Send the Open event to any listeners */
        protected void _clickFolder(FolderViewModel vm)
        {
            if (FolderClicked!= null)
                FolderClicked(vm);
        }

        public void ClickFolder(object sender, EventArgs eArgs)
        {
            _clickFolder(this);
        }


        private RelayCommand _toggleExpandCommand;
        public ICommand ToggleExpandCommand => _toggleExpandCommand;

        public void ToggleExpand()
        {
            IsExpanded = !IsExpanded && HasChildren;
        }


        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if(_isSelected != value) 
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public void DeselectAll()
        {
            IsSelected = false;
            foreach(FolderViewModel f in Children)
                f.DeselectAll();
        }
    }
}
