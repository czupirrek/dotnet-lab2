using dotnet_lab2_cli;
using System.Collections.ObjectModel;
namespace Maui_lastfm
{
    public partial class MainPage : ContentPage
    {

        private ObservableCollection<ImageItem> _imageItems = new ObservableCollection<ImageItem>
    {
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" },
        new ImageItem { ImageUrl = "", Text = "" }
    };


        string User;
        DateTime From;
        DateTime To;
        ApiTest Api;
        List<Tuple<int, string, string>> TopAlbums;
        public MainPage()
        {
            Api = new ApiTest();
            InitializeComponent();
            ImageCollectionView.ItemsSource = _imageItems; 



            User = "empty";




        }

        private void OnDataEntered(object sender, EventArgs e)
        {
            LabelUsername.Text = this.User;
            DatesToFrom.Text = this.From.ToString() + " " + this.To.ToString();
        }

        private void OnTextEntered(object sender, EventArgs e)
        {
            
            this.User = TextEntry.Text;
 
        }

        private void OnDateSelectedFrom(object sender, DateChangedEventArgs e)
        {
            From = DatePickerFrom.Date;
        }
        private void OnDateSelectedTo(object sender, DateChangedEventArgs e)
        {
            To = DatePickerTo.Date;
        }

        private async void OnDataConfirmed(object sender, EventArgs e)
        {
            LabelUsername.Text = "running...";

            await Api.GetRecentTracksByDateSpan(User, From, To);
            TopAlbums = await Api.GetTopAlbums(10, From, To);

            for (int i = 0; i < TopAlbums.Count; i++)
            {
                string description = $"{TopAlbums[i].Item1} repeats \r\n {TopAlbums[i].Item2}";
                _imageItems[i] = new ImageItem { ImageUrl = TopAlbums[i].Item3, Text = description };
            }




            LabelUsername.Text = "DONE...";



        }




    }


    public class ImageItem
    {
        public string ImageUrl { get; set; }
        public string Text { get; set; }
    }


}


