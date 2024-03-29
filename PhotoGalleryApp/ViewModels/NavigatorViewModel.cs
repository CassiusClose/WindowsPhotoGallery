﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.Views;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A navigator ViewModel that maintains a history of "pages" (ViewModels w/ corresponding Views) and allows navigation between them.
    /// </summary>
    public class NavigatorViewModel : ViewModelBase
    {
        #region Constructors

        public NavigatorViewModel()
        {
            // Set up commands
            _goBackPageCommand = new RelayCommand(GoBackPage, CanGoBackPage);

            _history = new Stack<ViewModelBase>();
        }

        public override void Cleanup() { }

        #endregion Constructors



        #region Fields and Properties

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

        #endregion Fields and Properties



        #region Methods

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
                // Trigger cleanup on the page being removed
                CurrentPage.Cleanup();
                CurrentPage.NavigatorLostFocus();

                CurrentPage = _history.Pop();
                _goBackPageCommand.InvokeCanExecuteChanged();
            }
        }

        /// <summary>
        /// Opens the given popup, blocks until it is closed, and returns it results
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public PopupReturnArgs OpenPopup(PopupViewModel vm)
        {
            //TODO This isn't MVVM
            PopupWindow popup = new PopupWindow();
            popup.DataContext = vm;

            // The Accept and Cancel buttons will set the Window's DialogResult
            // property, so use that here to determine if the user accepted or
            // cancelled.
            Nullable<bool> accepted = popup.ShowDialog();
            if (!accepted.HasValue)
                throw new Exception("PopupWindow's ShowDialog returned null");

            // Set the Accepted bool here, so subclasses don't have to deal
            // with it
            PopupReturnArgs results = vm.GetPopupResults();
            results.PopupAccepted = accepted.Value;

            vm.Cleanup();

            return results;
        }

        #endregion Methods



        #region Commands

        private readonly RelayCommand _goBackPageCommand;
        /// <summary>
        /// A command that goes back one page in the history, making the previous page the current one.
        /// </summary>
        public ICommand GoBackPageCommand => _goBackPageCommand;
        

        /// <summary>
        /// Returns whether or not there is a page in the history to return to.
        /// </summary>
        /// <returns>Whether or not there is page that can be returned to.</returns>
        public bool CanGoBackPage()
        {
            return _history.Count != 0;
        }

        #endregion Commands
    }
}
