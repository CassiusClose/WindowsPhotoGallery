using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for a popup that displays a folder tree of all events and lets the user choose one, or choose a create new option.
    /// </summary>
    public class EventSelectionPopupViewModel : PopupViewModel
    {
        public EventSelectionPopupViewModel(NavigatorViewModel nav, MediaCollection coll)
        {
            _nav = nav;
            _collection = coll;

            _createEventCommand = new RelayCommand(CreateEvent);

            _folders = new YearMonthFolderTree(coll, false);
            _folders.FolderClicked += FolderChosen;
        }
        public override void Cleanup()
        {
            _folders.Cleanup();
            _folders.FolderClicked -= FolderChosen;
        }


        private NavigatorViewModel _nav;
        private MediaCollection _collection;
        private YearMonthFolderTree _folders;

        public ObservableCollection<FolderViewModel> Folders 
        {
            get { return _folders.Folders; }
        }


        // If the user creates a new event, this is the name
        private string? _newEventName = null;
        // If the user chooses an existing event, this is it
        private Event? _chosenEvent = null;



        /**
         * Either return that the user canceled, created a new event, or chose an event
         */
        public override PopupReturnArgs GetPopupResults()
        {
            if(_chosenEvent != null)
                return new EventSelectionPopupReturnArgs(EventSelectionPopupReturnArgs.EventType.EventChosen, _chosenEvent);

            if(_newEventName != null)
                return new EventSelectionPopupReturnArgs(EventSelectionPopupReturnArgs.EventType.EventCreated, _newEventName);

            return new EventSelectionPopupReturnArgs();
        }


        /**
         * When an event is chosen, find the event instance and close the popup
         */
        private void FolderChosen(FolderViewModel folderVM)
        {
            if (folderVM is not EventFolderViewModel)
                throw new Exception("EventSelectionPopup FolderViewModel is not FolderLabelViewModel");

            EventFolderViewModel folder = (EventFolderViewModel)folderVM;
            // If it's an event, open the event's collection
            if (folder.Timestamp.Precision == TimeRange.Second)
            {
                _chosenEvent = EventsViewModel.GetEventFromFolder(folder, _collection);
                if (_chosenEvent == null)
                    return;

                ClosePopup(true);
            }
        }

        private RelayCommand _createEventCommand;
        public ICommand CreateEventCommand => _createEventCommand;

        public void CreateEvent()
        {
            TextEntryPopupViewModel vm = new TextEntryPopupViewModel();
            TextEntryPopupReturnArgs args = (TextEntryPopupReturnArgs)_nav.OpenPopup(vm);
            if(args.PopupAccepted && args.Text != null)
            {
                _newEventName = args.Text;
                ClosePopup(true); 
            }
        }

        protected override bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(_newEventName) && _chosenEvent == null)
            {
                ValidationErrorText = "Pick one option";
                return false;
            }

            ValidationErrorText = "";
            return true;
        }
    }



    /// <summary>
    /// Stores data returned from the event selection popup. Keeps track of whether the
    /// user created a new event, chose an event, or did nothing.
    /// </summary>
    public class EventSelectionPopupReturnArgs : PopupReturnArgs
    {
        public EventSelectionPopupReturnArgs() { }

        public EventSelectionPopupReturnArgs(EventType action, Event e)
        {
            Action = action;
            Event = e;
        }

        public EventSelectionPopupReturnArgs(EventType action, string newName)
        {
            Action = action;
            NewEventName = newName;
        }

        public enum EventType
        {
            EventChosen, EventCreated
        }

        public EventType Action;
        public Event? Event = null;
        public string? NewEventName = null;
    }
}
