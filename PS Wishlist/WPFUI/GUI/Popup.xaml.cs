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
using System.Windows.Shapes;

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
