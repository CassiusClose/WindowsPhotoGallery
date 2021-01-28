using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// An implemenation of the ICommand interface.
    /// </summary>
    /// <remarks>
    /// Implementation taken from
    /// <see href="https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern"/>
    /// and
    /// <see href="https://intellitect.com/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/"/>
    /// </remarks>
    class RelayCommand : ICommand
    {

        /// <summary>
        /// The function that actually executes the command.
        /// </summary>
        private readonly Action<object> _execute;

        // The function that determines whether or not the command can be executed
        /// <summary>
        /// 
        /// </summary>
        private readonly Func<object, bool> _canExecuteAction;


        public RelayCommand(Action<object> execute, Func<object, bool> canExecuteAction)
        {
            _execute = execute;
            _canExecuteAction = canExecuteAction;
        }

        public RelayCommand(Action<object> execute) : this(execute, null) { }


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Argument to the command's execution function</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Returns whether or not the command can be executed.
        /// </summary>
        /// <param name="parameter">Argument to the function that determines executable status.</param>
        /// <returns>Whether or not this command can be executed.</returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecuteAction != null)
                return _canExecuteAction(parameter);
            return true;
        }

        public event EventHandler CanExecuteChanged;


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
    }
}
