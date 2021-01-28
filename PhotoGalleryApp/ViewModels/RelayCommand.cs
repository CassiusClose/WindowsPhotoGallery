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
        // The function that executes the command
        private readonly Action<object> _execute;

        // The function that determines whether or not the command can be executed
        private readonly Func<object, bool> _canExecuteAction;


        public RelayCommand(Action<object> execute, Func<object, bool> canExecuteAction)
        {
            _execute = execute;
            _canExecuteAction = canExecuteAction;
        }

        public RelayCommand(Action<object> execute) : this(execute, null) { }


        /*
         * Execute the command
         */
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /*
         * Returns whether or not the command can be executed
         */
        public bool CanExecute(object parameter)
        {
            if (_canExecuteAction != null)
                return _canExecuteAction(parameter);
            return true;
        }

        public event EventHandler CanExecuteChanged;

        /*
         * Tell the ICommand interface that the conditions that affect whether or not
         * the command can execute have changed. Any classes that use this class should
         * call this when changing those conditions.
         */
        public void InvokeCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
