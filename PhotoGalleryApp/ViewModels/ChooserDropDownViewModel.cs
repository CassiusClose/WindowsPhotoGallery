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
        /**
         * The text that the Create New button in the drop-down list will have. The button, when 
         * clicked on, is sent over to the VM like a regular tag, so the VM needs to know what text
         * it contains to process it correctly.
         */
        private const string CREATE_NEW_TEXT = "Create New";

        #region Constructors

        /// <summary>
        /// Initializes a ChooserDropDown with a collection of items.
        /// </summary>
        /// <param name="items"></param>
        public ChooserDropDownViewModel(CollectionViewSource items) : this(items, null) { }


        /// <summary>
        /// Initializes a ChooserDropDown with a collection of items. Also stores a callback
        /// function to be called when an item is chosen or created.
        /// </summary>
        /// <param name="items">The items that will be choosable in the dropdown menu.</param>
        /// <param name="addTagCallback">A callback function that will be called when an item is
        /// either chosen or created.</param>
        public ChooserDropDownViewModel(CollectionViewSource items, ItemCallback addTagCallback) : this(items, addTagCallback, true, true) { }


        /// <summary>
        /// Initializes a ChooserDropDown with a collection of items. Also stores a callback
        /// function to be called when an item is chosen or created.
        /// </summary>
        /// <param name="items">The items that will be choosable in the dropdown menu.</param>
        /// <param name="addTagCallback">A callback function that will be called when an item is
        /// either chosen or created.</param>
        /// <param name="showCreateButton">Whether the "create new item" button should be shown.</param>
        /// <param name="emptyTextShowsAll">Whether no input in the textbox should display nothing or all the drop-down's items.</param>
        public ChooserDropDownViewModel(CollectionViewSource items, ItemCallback addTagCallback, bool showCreateButton, bool emptyTextShowsAll)
        {
            // Init commands
            _itemSelectedCommand = new RelayCommand(ItemSelected);
            _newItemCommand = new RelayCommand(CreateNewItem);
            _textboxFocusedCommand = new RelayCommand(TextboxFocused);
            _textboxUnfocusedCommand = new RelayCommand(TextboxUnfocused);
            
            CreateButtonVisible = showCreateButton;
            _emptyTextShowsAll = emptyTextShowsAll;

            ItemSelectedCallback = addTagCallback;
            _textInput = "";


            // Setup collection of items
            _itemsSource = items;

            Items.Filter += ItemFilter;
        }

        #endregion Constructors



        #region Fields and Properties

        /**
         * Store the items as a CollectionViewSource and have the ICollectionView
         * request a new view from the CollectionViewSource every time its getter is
         * called. Otherwise, if we kept the same ICollectionView the whole time, it
         * could become stale and be garbage collected.
         */
        private CollectionViewSource _itemsSource;
        /// <summary>
        /// The items that can be chosen with the drop-down.
        /// </summary>
        public ICollectionView Items 
        { 
            get { return _itemsSource.View; } 
        }



        private String _textInput;
        /// <summary>
        /// The text that is the content of the textbox.
        /// </summary>
        public String TextInput
        {
            get { return _textInput; }
            set
            {
                // Whether or not backspace was just typed.
                bool isBackspace = _textInput.Length > value.Length;

                // Check if the user pressed Enter
                if (value.Contains('\n') || value.Contains('\r'))
                {
                    // If the user pressed eneter, find the selected item. If it's the Create New
                    // button, then carry that out. Otherwise, select the chosen item.
                    List<string> list = Items.Cast<string>().ToList();
                    if (SelectedIndex < 0 || SelectedIndex >= list.Count || list[SelectedIndex] == CREATE_NEW_TEXT)
                    {
                        if (_showCreateButton)
                            CreateNewItem();
                    }
                    else
                    {
                        ItemSelected(list[SelectedIndex]);
                    }
                    return;
                }

                // Check if the user pressed tab
                if (value.Contains('\t'))
                {
                    // If the user pressed tab, fill the textbox with the text of the selected item.
                    // If there are no tags in the dropdown list, then do nothing (remove the tabs
                    // from the string and continue normally)
                    List<string> list = Items.Cast<string>().ToList();
                    if (SelectedIndex < 0 || SelectedIndex >= list.Count || list[SelectedIndex] == CREATE_NEW_TEXT)
                    {
                        _textInput = value.Replace("\t", "");
                    }
                    else
                    {
                        _textInput = (string)list[SelectedIndex];
                    }
                }
                else
                {
                    _textInput = value;
                }


                // When the input changes, adjust the item filter
                Items.Refresh();

                OnPropertyChanged();
                OnPropertyChanged("CreateButtonVisible");

                List<string> l = Items.Cast<string>().ToList();
                /* Conditions for not resetting the selected item to the first of the list:
                 * - If the selected item can't be found (the list is empty, selected index is -1, etc.),
                 * - If the selected item is not the first on the list & the user didn't backspace
                 *      (if the user uses arrow keys to select something other than the first on the
                 *      list, then keep that item selected as long as possible, if the user keeps
                 *      typing. Backspacing resets the selection)
                 */
                if (SelectedIndex >= 0 && SelectedIndex < l.Count && (string) l[0] != (string) l[SelectedIndex] && !isBackspace)
                {
                }
                else
                {
                    SelectedIndex = 0;
                }
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


        
        private bool _showCreateButton;
        /// <summary>
        /// Whether the "Create New" button should be visible. This determines whether the drop-down is
        /// just used to choose existing items or if it can be used to create new items as well.
        /// </summary>
        public bool CreateButtonVisible
        {
            get
            {
                // Hide the Create New button if there's no input (can't create an empty tag) or
                // if the text exactly matches a tag (can't duplicate a tag)
                if (TextInput == "" || Items.Contains(TextInput))
                    return false;

                return _showCreateButton;
            }
            set
            {
                _showCreateButton = value;
                OnPropertyChanged();
            }
        }


        /**
         * Whether or not an empty textbox should display all the drop-down's items or none of them.
         */
        private bool _emptyTextShowsAll;


        private int _selectedIndex;
        /// <summary>
        /// The index of the selected item in the drop-down list. This might be -1 or greater than the
        /// size of the Items property (because the Create New item is added in the XAML)
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

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

            // If there's no input in the textbox, determine whether to show all or none of the items
            if (TextInput == "")
            { 
                if (_emptyTextShowsAll)
                    return true;
                return false;
            }

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

            // Send focus back to the main window. Not really MVVM, but it's a slight
            // trangression and it's not clear how I would do it otherwise
            ((MainWindow)Application.Current.MainWindow).SeizeFocus();
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
            {
                string item = parameter as string;
                // If the item selected was actually the Create New button, carry that out
                if (item == CREATE_NEW_TEXT)
                    CreateNewItem();
                // Otherwise, process it as a normal tag
                else
                    ItemSelectedCallback(parameter as string, false);

            }

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
            if (TextInput == "")
                return;

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
