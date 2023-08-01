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

            _folders = new FolderView(coll, false);
            _folders.FolderOpened += FolderChosen;
        }

        private NavigatorViewModel _nav;
        private MediaCollection _collection;
        private FolderView _folders;

        public ObservableCollection<FolderLabelViewModel> Folders 
        {
            get { return _folders.Folders; }
        }


        private EventSelectionPopupReturnArgs returnData = new EventSelectionPopupReturnArgs(EventSelectionPopupReturnArgs.ReturnType.Cancelled);


        /**
         * Either return that the user canceled, created a new event, or chose an event
         */
        public override object? GetPopupResults()
        {
            return returnData;
        }


        public override void Cleanup()
        {
            _folders.Cleanup();
            _folders.FolderOpened -= FolderChosen;
        }

        /**
         * When an event is chosen, find the event instance and close the popup
         */
        private void FolderChosen(FolderLabelViewModel folder) 
        { 
            // If it's an event, open the event's collection
            if (folder.Timestamp.Precision == TimeRange.Second)
            {
                Event? e = EventsViewModel.GetEventFromFolder(folder, _collection);
                if (e == null)
                    return;

                returnData = new EventSelectionPopupReturnArgs(EventSelectionPopupReturnArgs.ReturnType.EventChosen, e);
                ClosePopup();
            }
        }

        private RelayCommand _createEventCommand;
        public ICommand CreateEventCommand => _createEventCommand;

        public void CreateEvent()
        {
            TextEntryPopupViewModel vm = new TextEntryPopupViewModel();
            TextEntryPopupReturnArgs args = (TextEntryPopupReturnArgs)_nav.OpenPopup(vm);
            if (args.Action == TextEntryPopupReturnArgs.ReturnType.TextEntered && args.Text != null)
            {
                returnData = new EventSelectionPopupReturnArgs(EventSelectionPopupReturnArgs.ReturnType.EventCreated, args.Text);
                ClosePopup(); 
            }
        }
    }



    /// <summary>
    /// Stores data returned from the event selection popup. Keeps track of whether the
    /// user created a new event, chose an event, or did nothing.
    /// </summary>
    public class EventSelectionPopupReturnArgs
    {
        public EventSelectionPopupReturnArgs(ReturnType returnType)
        {
            Action = returnType;
        }

        public EventSelectionPopupReturnArgs(ReturnType returnType, Event e)
        {
            Action = returnType;
            Event = e;
        }

        public EventSelectionPopupReturnArgs(ReturnType returnType, string newName)
        {
            Action = returnType;
            NewEventName = newName;
        }

        public enum ReturnType
        {
            EventChosen, EventCreated, Cancelled
        }

        public ReturnType Action;
        public Event? Event = null;
        public string? NewEventName = null;
    }
}
