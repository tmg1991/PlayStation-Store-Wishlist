using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPFUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WishlistViewModel _viewModel;
        delegate void Del();
        Del _handler;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new WishlistViewModel();
            DataContext = _viewModel;
            var task = Task.Run(() =>
            {
                _viewModel?.UpdateGamePrices();
            });

            task.ContinueWith((t) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ListboxGames.Items.Refresh();
                });
            });

        }

       


        private void InputField_GotFocus(object sender, RoutedEventArgs e)
        {
            InputField.Text = string.Empty;
            InputField.FontStyle = FontStyles.Normal;
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string url = InputField.Text;
            InputField.Text = "copy game URL here";
            InputField.FontStyle = FontStyles.Italic;
            var task = Task.Run(() =>
            {
                _viewModel?.AddGameFromUrl(url);
            });

            task.ContinueWith((t) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ListboxGames.Items.Refresh();
                });
            });


        }

        private void ListboxGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e?.AddedItems == null || e.AddedItems.Count < 1)
            {
                return;
            }
            var item = e.AddedItems[0];
            _viewModel?.OpenGameUrl(item);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {

            var task = Task.Run(() =>
            {
                _viewModel?.UpdateGamePrices();
            });

            task.ContinueWith((t) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ListboxGames.Items.Refresh();
                });
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var item = (Button)e.Source;
            var game = item.DataContext;
            _viewModel?.RemoveGameFromList(game);
            ListboxGames.Items.Refresh();
        }
    }
}
