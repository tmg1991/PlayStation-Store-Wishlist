using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFUI
{
    public class WishlistViewModel : INotifyPropertyChanged

    {
        private string _jsonFilePath;
        private object _locker = new object();


        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                if (_isBusy)
                {
                    IsGameOnSale = false;
                }
                OnPropertyChanged();
            }
        }

        private bool _isGameOnSale = false;

        public bool IsGameOnSale
        {
            get { return _isGameOnSale; }
            set
            {
                _isGameOnSale = value;
                OnPropertyChanged();
            }
        }

        private List<GameItem> _games;
        public List<GameItem> Games
        {
            get
            {
                return _games;
            }
            set
            {
                _games = value;
                OnPropertyChanged();
            }
        }

        string _dataDirectory;


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public WishlistViewModel()
        {
            CreateImageDirectory();
            Games = new List<GameItem>();
            try
            {
                lock (_locker)
                {
                    IsBusy = true;
                    Games = SaveLoadUtils.LoadFromJson(_jsonFilePath);
                    foreach (var game in Games)
                    {
                        game.ImageSource = LoadImage(game.CoverImagePath);
                    }
                    IsBusy = false;
                }
            }
            catch (Exception e)
            {
                //ignore
            }

        }

        public void OpenGameUrl(object item)
        {
            GameItem game = item as GameItem;
            if (game == null)
            {
                return;
            }
            try
            {
                Process.Start(game.URL);
            }
            catch (Exception e)
            {
                ShowMessage(e.ToString(), MessageType.Error);
            }

        }

        private void ShowMessage(string message, MessageType messageType)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

                var popup = new Popup();
                popup.SetMessage(message, messageType);
                popup.ShowDialog();
            });
        }

        public void UpdateGamePrices()
        {
            lock (_locker)
            {
                IsBusy = true;
                foreach (var game in Games)
                {
                    try
                    {
                        HtmlWeb web = new HtmlWeb();
                        HtmlDocument doc = web.Load(game.URL);
                        ScrapePrices(game, doc);
                    }
                    catch (Exception e)
                    {

                        ShowMessage(e.ToString(), MessageType.Error);
                    }

                }
                SaveLoadUtils.SaveToJson(Games, _jsonFilePath);
                CheckIfGameIsOnSale();
                IsBusy = false;
            }
        }

        private void CheckIfGameIsOnSale()
        {
            IsGameOnSale = false;
            foreach (var game in Games)
            {
                if (!string.IsNullOrEmpty(game.OriginalPrice))
                {
                    IsGameOnSale = true;
                    break;
                }
            }
        }

        public void AddGameFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (CheckIfDuplicate(url))
            {
                ShowMessage("This game is already in the list 🙂", MessageType.Info);
                return;
            }

            lock (_locker)
            {

                IsBusy = true;
                try
                {

                    DoAddGameFromUrl(url);
                }
                catch (Exception e)
                {
                    ShowMessage(e.ToString(), MessageType.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public bool CheckIfDuplicate(string url)
        {
            foreach (var game in Games)
            {
                if (game.URL == url)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveGameFromList(object game)
        {
            var gameItem = game as GameItem;
            if (gameItem == null)
            {
                return;
            }
            lock (_locker)
            {
                if (File.Exists(gameItem.CoverImagePath))
                {
                    try
                    {
                        File.Delete(gameItem.CoverImagePath);
                    }
                    catch (Exception e)
                    {
                        ShowMessage(e.ToString(), MessageType.Error);
                    }
                }


                Games.Remove(gameItem);
                SaveLoadUtils.SaveToJson(Games, _jsonFilePath);
                CheckIfGameIsOnSale();
            }
        }

        private void CreateImageDirectory()
        {
            string cwd = Directory.GetCurrentDirectory();
            _dataDirectory = Path.Combine(cwd, "Data");
            Directory.CreateDirectory(_dataDirectory);
            _jsonFilePath = Path.Combine(_dataDirectory, "Wishlist.json");

        }

        private void DoAddGameFromUrl(string url)
        {
            GameItem game = new GameItem()
            {
                Title = string.Empty,
                CoverImagePath = string.Empty,
                FinalPrice = string.Empty,
                OriginalPrice = string.Empty,
                URL = url
            };
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection imageNodes = doc.DocumentNode.SelectNodes("//img[@data-qa]");
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//h1[@data-qa]");

            string imageFullURL = string.Empty;

            game.Title = titleNodes.FirstOrDefault().InnerHtml;

            ScrapePrices(game, doc);

            foreach (var node in imageNodes)
            {
                if (node.OuterHtml.Contains("data-qa=\"gameBackgroundImage"))
                {
                    string outerHtml = node.OuterHtml;
                    string[] separator = new string[] { "src=" };
                    var arrays = outerHtml.Split(new string[] { "src=" }, StringSplitOptions.None);
                    var imageUrlArray = arrays[1].Split(' ');
                    string imageUrl = imageUrlArray[0].Split('>')[0];

                    imageFullURL = imageUrl.Trim('\"');
                    break;
                }

            }

            using (WebClient webclient = new WebClient())
            {
                byte[] data = webclient.DownloadData(imageFullURL);
                using (MemoryStream memStream = new MemoryStream(data))
                {
                    using (var myImage = Image.FromStream(memStream))
                    {
                        game.CoverImagePath = GenerateImageName(game.Title);
                        myImage.Save(game.CoverImagePath, ImageFormat.Png);
                        game.ImageSource = LoadImage(game.CoverImagePath);
                    }
                }
            }


            Games.Add(game);
            SaveLoadUtils.SaveToJson(Games, _jsonFilePath);
            CheckIfGameIsOnSale();
        }

        private void ScrapePrices(GameItem game, HtmlDocument htmlDocument)
        {
            game.OriginalPrice = string.Empty;
            game.FinalPrice = string.Empty;

            HtmlNodeCollection priceNodes = htmlDocument.DocumentNode.SelectNodes("//span[@data-qa]");
            foreach (var node in priceNodes)
            {
                if (node.OuterHtml.Contains("finalPrice"))
                {
                    game.FinalPrice = node.InnerText;
                    break;
                }
            }

            foreach (var node in priceNodes)
            {
                if (node.OuterHtml.Contains("originalPrice"))
                {
                    game.OriginalPrice = node.InnerText;
                    break;
                }
            }
        }

        private string GenerateImageName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                Random rnd = new Random();
                return rnd.Next().ToString() + ".png";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(_dataDirectory);
            sb.Append("\\");

            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            sb.Append(".png");
            return sb.ToString();
        }

        private ImageSource LoadImage(string path)
        {
            var bitmapImage = new BitmapImage();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // optional
            }

            return bitmapImage;
        }

    }

    enum MessageType
    {
        Error,
        Info
    }
}
