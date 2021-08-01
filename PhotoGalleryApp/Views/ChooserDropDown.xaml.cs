using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ChooserDropDown.xaml
    /// </summary>
    public partial class ChooserDropDown : UserControl
    {
        public ChooserDropDown()
        {
            InitializeComponent();
        }


        /**
         * Selects the first item in the dropdown list.
         */
        private void SelectFirst()
        {
            DropDownListBox.SelectedIndex = 0;
        }

        /**
         * When the textbox gains focused (i.e. is clicked on when focus was elsewhere), reset
         * the selection to the first item in the list.
         */
        private void Textbox_Focused(object sender, RoutedEventArgs e)
        {
            SelectFirst();
        }


        /**
         * Handles non-text key input to the textbox.
         */
        private void Textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
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


    }
}
