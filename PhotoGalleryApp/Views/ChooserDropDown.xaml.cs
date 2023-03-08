using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoGalleryApp.Views
{
    /// <summary>
    /// ChooserDropDown is a user control that functions as a drop-down list of items, filterable by what the user enters into
    /// a textbox. A button opens/closes a popup, which contains the textbox and list of items. It is similar in function to
    /// Gmail's "move to folder" feature. If the "Create New" button is enabled, then when the user enters text that does not
    /// match an item on the list, one of the options on the list will be to create that text as a new item. The user can select
    /// different items on the list with the up/down arrow keys. Pressing "Tab" will autofill the text and pressing Enter will
    /// choose the selected item. When the user chooses an item, an event will fire.
    /// </summary>
    public partial class ChooserDropDown : UserControl
    {
        public ChooserDropDown()
        {
            InitializeComponent();
        }

        
       
        #region DependencyProperties

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<string>), typeof(ChooserDropDown),
            new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = ItemsSourceChanged
            }
        );
        /// <summary>
        /// The collection of items that are displayed/filter on the control's drop down.
        /// </summary>
        public ObservableCollection<string> ItemsSource
        {
            get { return (ObservableCollection<string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /**
         * When the set of items changes, refresh the display of the items.
         */
        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChooserDropDown control = (ChooserDropDown)d;
            if (control != null)
            {
                control.RefreshView();
            }
        }







        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ChooserDropDown),
            new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = TextPropertyChanged,
                DefaultValue = ""
            }
        );
        /// <summary>
        /// The text that is in the control's TextBox
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /**
         * When the textbox's text changes, call the instance's change handler
         */
        private static void TextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChooserDropDown control = (ChooserDropDown)d;
            if (control != null)
                control.TextChanged();
        }

        /**
         * When the textbox's text changes, update the display in a number of ways
         */
        private void TextChanged()
        {
            /* Hide the "Create New" button if:
             *      - the textbox is empty (can't create an empty item)
             *      - the textbox contains the exact content of one of the items (can't create duplicate items)
             *      - for whatever reason, the Text or ItemsSource properties are null
             */
            if (Text == null || Text == "" || ItemsSource == null || ItemsSource.Contains(Text))
            {
                // If it's already hidden, do nothing, avoid a UI refresh
                if (CreateButtonVisible)
                    CreateButtonVisible = false;
            }
            // Otherwise, there's text in the textbox which does not exactly match an item, so the create button
            // should be visible if it's enabled by the creator of this control.
            else
            {
                // If it's already set property, do nothing, avoid a UI refresh
                if (CreateButtonVisible != ShowCreateButton)
                    CreateButtonVisible = ShowCreateButton;
            }


            // The selected item before the view is refreshed
            string origSel = (string)SelectedItem;

            // The text which filters the items has changed, so refresh the list of items
            RefreshView();


            // If the previously selected item is still in the list, but unselected, select it.
            // Sometimes the listbox will lose a selection (possibly due to changing its ItemsSource),
            // so this makes everything work as it should.
            if (DropDownListBox.Items.Contains(origSel) && (string)SelectedItem != origSel)
                DropDownListBox.SelectedItem = (string)origSel;

            // And in case the selection index is invalid, then reset the selection to the first item.
            int selectedIndex = DropDownListBox.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= DropDownListBox.Items.Count)
                SelectFirst(); 
        }



        
        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(string), typeof(ChooserDropDown),
            new PropertyMetadata("Drop Down"));
        /// <summary>
        /// The text that should display in the button that opens the popup.
        /// </summary>
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }




        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(ChooserDropDown));
        /// <summary>
        /// The item currently selected in the control's drop-down list.
        /// </summary>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }





        public static readonly DependencyProperty ShowCreateButtonProperty = DependencyProperty.Register("ShowCreateButton", typeof(bool), typeof(ChooserDropDown),
            new PropertyMetadata(false));
        /// <summary>
        /// Whether the drop-down list should show the "Create New" button when the user has entered text that is not one of the items.
        /// </summary>
        public bool ShowCreateButton
        {
            get { return (bool)GetValue(ShowCreateButtonProperty); }
            set { SetValue(ShowCreateButtonProperty, value); }
        }




        // Read-only property - the user should set ShowCreateButton to enable the feature, and this will control the button's visibility
        // in more detail (even if the feature is enabled, the button will not always be visible)
        private static readonly DependencyPropertyKey CreateButtonVisiblePropertyKey = DependencyProperty.RegisterReadOnly("CreateButtonVisible", typeof(bool), typeof(ChooserDropDown),
            new PropertyMetadata(false));
        public static readonly DependencyProperty CreateButtonVisibleProperty = CreateButtonVisiblePropertyKey.DependencyProperty;
        /// <summary>
        /// Whether the "Create New" option the drop-down list is visible or not. Users of the control should set ShowCreateButton
        /// to enable the feature. Even if the feature is enabled, the button will not always be visible, so this property represents
        /// that.
        /// </summary>
        public bool CreateButtonVisible
        {
            get { return (bool)GetValue(CreateButtonVisibleProperty); }
            protected set { SetValue(CreateButtonVisiblePropertyKey, value); }
        }





        public static readonly DependencyProperty EmptyTextShowsAllProperty = DependencyProperty.Register("EmptyTextShowsAll", typeof(bool), typeof(ChooserDropDown),
            new PropertyMetadata(true));
        /// <summary>
        /// When the textbox is empty, whether the control should display nothing (false) or every item on the list (true).
        /// </summary>
        public bool EmptyTextShowsAll
        {
            get { return (bool)GetValue(EmptyTextShowsAllProperty); }
            set { SetValue(EmptyTextShowsAllProperty, value); }
        }



        // Read-only property, user shouldn't be able to control if the popup opens or not.
        private static readonly DependencyPropertyKey PopupOpenPropertyKey = DependencyProperty.RegisterReadOnly("PopupOpen", typeof(bool), typeof(ChooserDropDown), new PropertyMetadata(false));
        public static readonly DependencyProperty PopupOpenProperty = PopupOpenPropertyKey.DependencyProperty;
        /// <summary>
        /// Whether the drop-down list is open (visible) or not.
        /// </summary>
        public bool PopupOpen
        {
            get { return (bool)GetValue(PopupOpenProperty); }
            protected set { SetValue(PopupOpenPropertyKey, value); }
        }

        #endregion DependencyProperties




        #region Events

        /// <summary>
        /// An Event which gets triggered anytime the user chooses to select an item on the
        /// drop-down list or create a new item. Arguments are stored in ItemChosenEventArgs.
        /// </summary>
        public event EventHandler<ItemChosenEventArgs> ItemSelected;

        #endregion Events




        #region Methods


        /**
         * The filter that determines which items are shown in the drop down. Any items that
         * begin with the contents of the textbox will be accepted.
         */
        public void FilterItem(object sender, FilterEventArgs e)
        {
            if (Text == null)
            {
                e.Accepted = false;
                return;
            }

            // If there's no input in the textbox, determine whether to show all or none of the items
            if (Text == "")
            {
                if (EmptyTextShowsAll)
                {
                    e.Accepted = true;
                    return;
                }
                e.Accepted = false;
                return;
            }


            string item = e.Item as string;
            if (item == null)
            {
                e.Accepted = false;
                return;
            }

            // Convert to lowercase so caps don't matter
            item = item.ToLower();
            string text = Text.ToLower();


            // Match any item where the user has typed the first (or whole) part of it
            if (item.StartsWith(text))
            {
                e.Accepted = true;
                return;
            }

            // Otherwise, don't match
            e.Accepted = false;
            return;
        }


        /**
         * Sends out an event that the given item has been selected by the user. If the item is the text in
         * the "Create New" option, then sends out as an item creation, rather than choosing an existing item.
         */
        private void ChooseItem(string item)
        {
            if (item != null)
            {
                // If the "Create New Item" option was picked
                if (item == (string)FindResource("CreateNewString"))
                {
                    // Only if the feature is enabled
                    if(ShowCreateButton)
                    {
                        ItemSelected.Invoke(this, new ItemChosenEventArgs(Text, true));
                        ClosePopup();
                    }
                }
                // Normal item selection
                else
                {
                    ItemSelected.Invoke(this, new ItemChosenEventArgs(item, false));
                    ClosePopup();
                }
            }
        }



        /**
       * Refresh the ICollectionView that displays the list of items so that it redraws.
       */
        private void RefreshView()
        {
            CollectionViewSource viewSource = (CollectionViewSource)FindResource("ItemsViewSource");
            if (viewSource != null && viewSource.View != null)
                viewSource.View.Refresh();
        }



        /**
         * Opens the drop-down list popup box.
         */
        private void OpenPopup()
        {
            Text = "";
            SelectFirst(); // Select the first item each time the popup opens
            PopupOpen = true;
            DropDown_TextBox.Focus();
        }

        /**
         * Closes the drop-down list popup box. Does this by taking focus away from this user control, so
         * the rest of the popup-closing code is in Textbox_LostFocus().
         */
        private void ClosePopup()
        {
            ((MainWindow)Application.Current.MainWindow).SeizeFocus();
        }


        /**
         * Selects the first item in the dropdown list.
         */
        private void SelectFirst()
        {
            DropDownListBox.SelectedIndex = 0;
        }



        /**
         * Autofills the text in the textbox as far as possible so that it matches the beginning
         * of each tag in the list. If the existing tags are "abcde", "abczz", and "abcdf", and
         * the textbox has "a", this will set the textbox to have "abc".
         */
        private void AutofillText()
        {
            if (SelectedItem == null || SelectedItem == FindResource("CreateNewString"))
                return;

            List<string> list = DropDownListBox.Items.Cast<string>().ToList();

            if (list.Count == 0)
                return;

            // Get length of shortest item - we'll be finding a string that
            // all of the tabs start with, so we only need to search them as
            // far as the shortest one.
            int minLen = int.MaxValue;
            foreach (string item in list)
                if (item.Length < minLen)
                    minLen = item.Length;


            bool done = false;
            // Start with what's already in the text input. That's guaranteed to be the
            // string that all of the items visible start with.
            string newText = Text;
            // Go through each character after the text input.
            for (int i = Text.Length; i < minLen; i++)
            {
                char c = list[0][i]; // Get the first strings char
                                     // Compare to the chars of the rest of the strings
                for (int j = 1; j < list.Count; j++)
                {
                    if (list[j] == (string)FindResource("CreateNewString"))
                        continue;
                    // If any of the strings have a char at this position that don't match
                    // the others, then we've reached our stopping point - we can't autofill
                    // this character.
                    if (list[j][i] != c)
                    {
                        done = true;
                        break;
                    }
                }

                if (done) // If we've found a discontinuity, then don't include the char. We're done.
                    break;
                else // Otherwise, the current position's character is shared by all strings, add it to the text
                    newText += c;
            }


            Text = newText;
            // Set the textbox cursor to the end of the string
            DropDown_TextBox.CaretIndex = Text.Length;
        }


        #endregion Methods



        #region Event Handlers

        /**
         * When the button is clicked on, either open or close the popup.
         */
        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (!PopupOpen)
                OpenPopup();
            else
                ClosePopup();
            e.Handled = true;
        }

      
        /**
         * When the textbox loses focus (is clicked off of, the window is hidden, etc.), close the drop-down list popup
         */
        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            Text = "";
            PopupOpen = false;
        }


        /**
         * When the user clicks on an item, "choose" that item
         */
        private void ItemClickedOn(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            if (item != null)
                ChooseItem(item.Content as string);

        }

   

        /**
         * Handles non-text key input to the textbox, such as "enter", "tab" functionality
         */
        private void Textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Pressing enter chooses the selected item
                case Key.Enter:
                    ChooseItem(SelectedItem as string);
                    e.Handled = true;
                    break;


                // Pressing tab autofills the textbox
                case Key.Tab:
                    AutofillText();
                    e.Handled = true;
                    break;


                /*
                 * Up/Down arrow keys scroll through the drop-down list.
                 */
                case Key.Up:
                    if (DropDownListBox.SelectedIndex == 0)
                        DropDownListBox.SelectedIndex = DropDownListBox.Items.Count - 1;
                    else
                        DropDownListBox.SelectedIndex -= 1;
                    break;

                case Key.Down:
                    if (DropDownListBox.SelectedIndex == DropDownListBox.Items.Count - 1)
                        DropDownListBox.SelectedIndex = 0;
                    else
                        DropDownListBox.SelectedIndex += 1;
                    break;
            }
        }

        #endregion Event Handlers
    }



    /// <summary>
    /// A type of WPF Event Arguments for events that involve choosing an item from a ChooserDropDown. The item
    /// might already have existed on the list or not.
    /// </summary>
    public class ItemChosenEventArgs : EventArgs
    {
        // The ChooserDropDown item that was selected
        public string Item;

        // Whether the item was already on the drop-down list, or if this event is creating the item
        public bool IsItemNew;

        /// <summary>
        /// Creates ItemChosenEventArgs for and event when the given string item is picked from a ChooserDropDown,
        /// either choosing an existing tag or creating a new one.
        /// </summary>
        /// <param name="item">The item chosen during this event.</param>
        /// <param name="itemNew">Whether the item is being created by this event or already existed in the list.</param>
        public ItemChosenEventArgs(string item, bool itemNew)
        {
            Item = item;
            IsItemNew = itemNew;
        }
    }

}
