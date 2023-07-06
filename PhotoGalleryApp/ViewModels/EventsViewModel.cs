using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel associated with the EventsView, which displays a list of all events.
    /// </summary>
    class EventsViewModel : ViewModelBase
    {
        public EventsViewModel(NavigatorViewModel nav, Gallery gallery)
        {
            _nav = nav; 
            _gallery = gallery;

            _openEventCommand = new RelayCommand(OpenEvent);

            _events = new ObservableCollection<EventViewModel>();
            foreach (Event e in gallery.Events)
            {
                _events.Add(new EventViewModel(e, _nav));
            }
            EventsView = CollectionViewSource.GetDefaultView(_events);
        }

        private NavigatorViewModel _nav;

        private Gallery _gallery;

        private ObservableCollection<EventViewModel> _events;

        public ICollectionView EventsView { get; }


        private RelayCommand _openEventCommand;
        public ICommand OpenEventCommand => _openEventCommand;
        /// <summary>
        /// Opens given EventViewModel's page
        /// </summary>
        /// <param name="parameter">The EventViewModel to open</param>
        public void OpenEvent(object parameter)
        {
            EventViewModel vm = (EventViewModel)parameter;
            _nav.NewPage(vm);
        }
    }
}
