using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for a popup that displays a folder tree of all events and lets the user choose one, or choose a create new option.
    /// </summary>
    public class EventSelectionPopupViewModel : PopupViewModel
    {
        public EventSelectionPopupViewModel(MediaCollection coll)
        {
            _collection = coll;
            _folders = new FolderView(coll, false);
            _folders.FolderOpened += FolderChosen;
        }

        private MediaCollection _collection;
        private FolderView _folders;

        public ObservableCollection<FolderLabelViewModel> Folders 
        {
            get { return _folders.Folders; }
        }


        private Event? chosenEvent = null;


        /**
         * Either return that the user canceled, created a new event, or chose an event
         */
        public override object? GetPopupResults()
        {
            if (chosenEvent == null)
                return new EventSelectionPopupReturnArgs(EventSelectionPopupReturnArgs.ReturnType.None);
            else
                return new EventSelectionPopupReturnArgs(EventSelectionPopupReturnArgs.ReturnType.EventChosen, chosenEvent);
        }


        public override void Cleanup()
        {
            _folders.Cleanup();
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

                chosenEvent = e;
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
            EventChosen, EventCreated, None
        }

        public ReturnType Action;
        public Event? Event = null;
        public string? NewEventName = null;
    }
}
