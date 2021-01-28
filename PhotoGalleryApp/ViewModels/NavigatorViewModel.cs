using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A navigator ViewModel that maintains a history of "pages" (ViewModels w/ corresponding Views) and allows navigation between them.
    /// </summary>
    class NavigatorViewModel : ViewModelBase
    {
        public NavigatorViewModel()
        {
            // Set up commands
            _goBackPageCommand = new RelayCommand(GoBackPage, CanGoBackPage);

            _history = new Stack<ViewModelBase>();
        }

        // Holds past pages. Does not contain the current page.
        private Stack<ViewModelBase> _history;

        private ViewModelBase _currentPage;
        /// <summary>
        /// The current page's ViewModel.
        /// </summary>
        public ViewModelBase CurrentPage
        {
            get { return _currentPage; }
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Adds a ViewModel as the new current page. The previously current page is saved in the history.
        /// </summary>
        /// <param name="vm">The ViewModel to make the current page.</param>
        public void NewPage(ViewModelBase vm)
        {
            if(CurrentPage != null)
            {
                _history.Push(CurrentPage);
                _goBackPageCommand.InvokeCanExecuteChanged();
            }

            CurrentPage = vm;
        }

        /// <summary>
        /// Goes back one page in the history, making the previous page the current one.
        /// </summary>
        /// <param name="parameter">Unused command parameter</param>
        public void GoBackPage(object parameter)
        {
            if(_history.Count != 0)
            {
                CurrentPage = _history.Pop();
                _goBackPageCommand.InvokeCanExecuteChanged();
            }
        }
                


        private readonly RelayCommand _goBackPageCommand;
        /// <summary>
        /// A command that goes back one page in the history, making the previous page the current one.
        /// </summary>
        public ICommand GoBackPageCommand => _goBackPageCommand;
        
        private bool CanGoBackPage(object parameter)
        {
            return _history.Count != 0;
        }
    }
}
