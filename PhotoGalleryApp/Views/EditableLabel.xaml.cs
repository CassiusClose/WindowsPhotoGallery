using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// EditableLabel is a control which displays a label with some text, bindable to the Text DependencyProperty.
    /// Double-clicking on this control will replace the label with a TextBox, where the user can edit the label text.
    /// The Text DependencyProperty will not update with the new text until this leaves edit mode. The user can leave
    /// edit mode by pressing enter, or shifting the focus to another control.
    /// </summary>
    public partial class EditableLabel : UserControl
    {
        public EditableLabel()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableLabel));

        /// <summary>
        /// The text being displayed in the label. When editing the text, this does not update until the control leaves edit mode.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }



        private void LeaveEditMode()
        {
            Text = ContentBox.Text;
            ContentLabel.Content = Text;
            ContentLabel.Visibility = Visibility.Visible;
            ContentBox.Visibility = Visibility.Collapsed;
        }

        private void EnterEditMode()
        {
            ContentBox.Text = Text;
            ContentLabel.Visibility = Visibility.Collapsed;
            ContentBox.Visibility = Visibility.Visible;
            ContentBox.CaretIndex = Text.Length;
            ContentBox.Focus();
        }


        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EnterEditMode();
            e.Handled = true;
        }

        private void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            LeaveEditMode();
        }

        private void Box_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                LeaveEditMode();
        }
    }
}
