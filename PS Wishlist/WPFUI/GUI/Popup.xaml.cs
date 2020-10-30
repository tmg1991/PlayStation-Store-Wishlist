using System.Windows;

namespace WPFUI
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    { 

        public Popup()
        {
            InitializeComponent();
            DataContext = this;
            
        }
        internal void SetMessage(string message, MessageType messageType)
        {
            if(messageType == MessageType.Error)
            {
                expanderError.Visibility = Visibility.Visible;
                txtHeader.Text = "An error occured! 😕";
                TxtMessage.Text = message;
            }
            else
            {
                expanderError.Visibility = Visibility.Collapsed;
                txtHeader.Text = message;
            }
        }

        
    }
}
