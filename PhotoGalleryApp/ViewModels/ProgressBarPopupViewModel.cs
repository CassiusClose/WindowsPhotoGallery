using PhotoGalleryApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for a popup box that displays a progress bar. You give this a
    /// work function, which should perform the task and report its progress,
    /// and when the function is completed, this will automatically close the
    /// popup. The popup return args will return
    /// </summary>
    public class ProgressBarPopupViewModel : PopupViewModel
    {
        public ProgressBarPopupViewModel(DoWorkEventHandler workFunction, List<object>? functionArg = null) : base(false, false)
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.DoWork += workFunction;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            // Start the task
            worker.RunWorkerAsync(functionArg);
        }
        public override void Cleanup() { }


        /**
         * When task is done, close the popup
         */
        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            _result = e.Result;
            ClosePopup(false);
        }



        /**
         * Update the UI with progress changes
         */
        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }


        // Return value from the work function
        object? _result = null;



        private int _progress = 0;
        /// <summary>
        /// Progress of the task, 0-100
        /// </summary>
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }


        private BackgroundWorker worker; 

        public override PopupReturnArgs GetPopupResults()
        {
            return new ProgressBarPopupReturnArgs(_result);
        }

        protected override bool ValidateData()
        {
            return true;
        }

    }

    class ProgressBarPopupReturnArgs : PopupReturnArgs
    {
        public ProgressBarPopupReturnArgs(object? result) { Result = result; }

        public object? Result;
    }
}
