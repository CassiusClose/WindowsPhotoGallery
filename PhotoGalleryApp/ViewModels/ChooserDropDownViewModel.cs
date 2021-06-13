using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// The ViewModel for an element similar to a combobox. Typing in a textbox filters a collection
    /// of items which are displayed in a drop-down list. You can either choose an item from the
    /// existing list, or create a new item from the entered text.
    /// </summary>
    class ChooserDropDownViewModel : ViewModelBase
    {

        #region Constructors

        /// <summary>
        /// Initializes a ChooserDropDown with a collection of items.
        /// </summary>
        /// <param name="items"></param>
        public ChooserDropDownViewModel(ICollectionView items) : this(items, null) { }

        /// <summary>
        /// Initializes a ChooserDropDown with a collection of items. Also stores a callback
        /// function to be called when an item is chosen or created.
        /// </summary>
        /// <param name="items">The items that will be choosable in the dropdown menu.</param>
        /// <param name="addTagCallback">A callback function that will be called when an item is
        /// either chosen or created.</param>
        public ChooserDropDownViewModel(ICollectionView items, ItemCallback addTagCallback)
        {
            // Init commands
            _itemSelectedCommand = new RelayCommand(ItemSelected);
            _newItemCommand = new RelayCommand(CreateNewItem);
            _textboxFocusedCommand = new RelayCommand(TextboxFocused);
            _textboxUnfocusedCommand = new RelayCommand(TextboxUnfocused);


            ItemSelectedCallback = addTagCallback;
            _textInput = "";


            // Setup collection of items
            Items = items;
            Items.Filter += ItemFilter;
        }

        #endregion Constructors



        #region Fields and Properties

        /// <summary>
        /// The items that can be chosen with the drop-down.
        /// </summary>
        public ICollectionView Items { get; }



        private String _textInput;
        /// <summary>
        /// The text that is the content of the textbox.
        /// </summary>
        public String TextInput
        {
            get { return _textInput; }
            set
            {
                _textInput = value;

                // When the input changes, adjust the item filter
                Items.Refresh();

                OnPropertyChanged();
            }
        }


        private bool _showDropDown;
        /// <summary>
        /// Whether or not the drop-down should be visible at the current time.
        /// </summary>
        public bool ShowDropDown
        {
            get { return _showDropDown; }
            set
            {
                _showDropDown = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A callback type that deals with when an item is selected from the drop-down.
        /// </summary>
        /// <param name="item">The item that was selected.</param>
        /// <param name="isNew">True if the item was just created (it wasn't in the items
        /// previously) and false if the item selected already existed.</param>
        public delegate void ItemCallback(string item, bool isNew);

        /// <summary>
        /// The callback function that will be called when an item from the drop-down list
        /// is selected or a new item is created.
        /// </summary>
        public ItemCallback ItemSelectedCallback;


        /*
        private bool _showCreateButton;
        public bool ShowCreateButton { get { return _showCreateButton; } }


        private bool _emptyTextShowsAll;
        public bool EmptyTextShowsAll { get { return _emptyTextShowsAll; } }

        */


        #endregion Fields and Properties


   
        #region Methods

        /// <summary>
        /// Filters the drop-down's items based on the input in the textbox.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ItemFilter(object item)
        {
            if (TextInput == null)
                return false;

            // If no input, then show all the items
            if (TextInput == "")
                return true;

            string tag = item as string;
            if (tag == null)
                return false;

            // Match any item where the user has typed the first (or whole) part of it
            if (tag.StartsWith(TextInput))
                return true;

            // Otherwise, don't match
            return false;
        }

        /**
         * Marks the popup as open and clears the textbox.
         */
        private void OpenPopup()
        {
            TextInput = "";
            ShowDropDown = true;
        }

        /**
         * Marks the popup as closed and clears the textbox. When the user
         * clicks off of the textbox, no text should remain.
         */
        private void ClosePopup()
        {
            ShowDropDown = false;
            TextInput = "";
        }


        #endregion Methods



        #region Commands


        private RelayCommand _itemSelectedCommand;
        /// <summary>
        /// A command which is called when an item in the drop-down is selected.
        /// </summary>
        public ICommand ItemSelectedCommand { get { return _itemSelectedCommand; } }

        /**
         * Called when an already existing item is selected from the dropdown.
         * This calls the item callback function and closes the popup.
         */
        private void ItemSelected(object parameter)
        {
            if (ItemSelectedCallback != null)
                ItemSelectedCallback(parameter as string, false);

            ClosePopup();
        }


        private RelayCommand _newItemCommand;
        /// <summary>
        /// A command which is called when an item is created in the drop-down list.
        /// </summary>
        public ICommand NewItemCommand { get { return _newItemCommand; } }

        /*
         * Called when an item is created in the drop-down list. This calls the item callback
         * function and closes the popup.
         */
        private void CreateNewItem()
        {
            if (ItemSelectedCallback != null)
                ItemSelectedCallback(TextInput, true);

            ClosePopup();
        }


        private RelayCommand _textboxFocusedCommand;
        /// <summary>
        /// A command which is called when the textbox gains focus.
        /// </summary>
        public ICommand TextboxFocusedCommand { get { return _textboxFocusedCommand; } }

        /**
         * When the user clicks on the textbox, open the popup.
         */
        private void TextboxFocused()
        {
            OpenPopup();
        }


        private RelayCommand _textboxUnfocusedCommand;
        /// <summary>
        /// A command which is called when the textbox loses focus.
        /// </summary>
        public ICommand TextboxUnfocusedCommand { get { return _textboxUnfocusedCommand; } }

        /**
         * When the user clicks off of the textbox, close the popup.
         */
        private async void TextboxUnfocused()
        {
            /*
             * As far as I can tell, the issue is that when the user clicks on some other button,
             * the LostFocus event is called before that button's Click event. The is an issue
             * when the button happens to be one in the drop-down list. Here (in LostFocus), the
             * popup is closed, and thus the buttons are destroyed before the Click event fires.
             * The result: the item callbacks (for add or select) are not called. The only way I 
             * could get this to work (regrettably), is to swap the order of the events by delaying
             * LostFocus by some amount of time so that Click is run first. It's super hacky, but
             * I went around and around with this problem and this was the only solution I could
             * come up with.
             */
            await Task.Delay(150); //HORRIBLE
            ClosePopup();
        }


        #endregion Commands
    }
}
