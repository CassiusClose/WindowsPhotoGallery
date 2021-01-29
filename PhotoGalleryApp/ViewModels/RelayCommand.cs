using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    class RelayCommand : ICommand
    {
        #region Fields and Properties

        /*
         * The function that actually executes the command. A version with an argument
         * and without. The bool determines which version to use.
         */
        private readonly Action<object> _execute_arg;
        private readonly Action _execute_noarg;
        private bool _executeHasArg;

        /*
         * The function that determines whether or not the command can be executed. A version
         * with an argument and without. The bool determines which version to use.
         */ 
        private readonly Func<object, bool> _canExecuteAction_arg;
        private readonly Func<bool> _canExecuteAction_noarg;
        private bool _canExecuteActionHasArg;

        public event EventHandler CanExecuteChanged;

        #endregion Fields and Properties
         


        #region Constructors

        /*
         * Different constructors for all the possibilites of execute and canExecuteAction
         * having arguments or not.
         */
        public RelayCommand(Action<object> execute, Func<object, bool> canExecuteAction)
        {
            _execute_arg = execute;
            _executeHasArg = true;
            _canExecuteAction_arg = canExecuteAction;
            _canExecuteActionHasArg = true;
        }

        public RelayCommand(Action<object> execute, Func<bool> canExecuteAction)
        {
            _execute_arg = execute;
            _executeHasArg = true;
            _canExecuteAction_noarg = canExecuteAction;
            _canExecuteActionHasArg = false;
        }

        public RelayCommand(Action execute, Func<object, bool> canExecuteAction)
        {
            _execute_noarg = execute;
            _executeHasArg = false;
            _canExecuteAction_arg = canExecuteAction;
            _canExecuteActionHasArg = true;
        }
        
        public RelayCommand(Action execute, Func<bool> canExecuteAction)
        {
            _execute_noarg = execute;
            _executeHasArg = false;
            _canExecuteAction_noarg = canExecuteAction;
            _canExecuteActionHasArg = false;
        }

        public RelayCommand(Action<object> execute)
        {
            _execute_arg = execute;
            _executeHasArg = true;
            _canExecuteActionHasArg = false;
        }

        public RelayCommand(Action execute)
        {
            _execute_noarg = execute;
            _executeHasArg = false;
            _canExecuteActionHasArg = false;
        }

        #endregion Constructors



        #region Methods

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Argument to the command's execution function, if there is one.</param>
        public void Execute(object parameter)
        {
            if (_executeHasArg)
                _execute_arg(parameter);
            else
                _execute_noarg();
        }

        /// <summary>
        /// Returns whether or not the command can be executed.
        /// </summary>
        /// <param name="parameter">Argument to the function that determines executable status, if there is one.</param>
        /// <returns>Whether or not this command can be executed.</returns>
        public bool CanExecute(object parameter)
        {
            if(_canExecuteActionHasArg)
            {
                if (_canExecuteAction_arg != null)
                    return _canExecuteAction_arg(parameter);
            }
            else
            {
                if (_canExecuteAction_noarg != null)
                    return _canExecuteAction_noarg();
            }
            return true;
        }



        /// <summary>
        /// Signals that the conditions for <c>CanExecute</c> have changed.
        /// </summary>
        /// <remarks>
        /// Users of this class should call this function whenever executability conditions
        /// have changed for that change to be reflected to the command's bindings.
        /// </remarks>
        public void InvokeCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Methods
    }
}
