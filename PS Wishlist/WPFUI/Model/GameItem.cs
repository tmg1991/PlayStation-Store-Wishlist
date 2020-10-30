using Newtonsoft.Json;
using System.Windows.Media;

namespace WPFUI
{
    public class GameItem
    {
        public string Title { get; set; }
        public string CoverImagePath { get; set; }

        [JsonIgnore]
        public ImageSource ImageSource { get; set; }
        public string FinalPrice { get; set; }
        public string OriginalPrice { get; set; }

        public string URL{ get; set; }

    }
}
