using dotnet_lab2_cli;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
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
        bool userOk = false;
        bool dateOk = false;
        int n = 10;
        public MainPage()
        {
            this.Api = new ApiTest();
            InitializeComponent();
            ImageCollectionView.ItemsSource = _imageItems; 



            User = "";




        }

        private void OnDataEntered(object sender, EventArgs e) //wcisniecie guzika walidacji danych
        {
            LabelUsername.Text = this.User;
            //DatesToFrom.Text = this.From.ToString() + " " + this.To.ToString();
            DateTime year2k = new DateTime(2000, 1, 1);

            int compare = DateTime.Compare(From, To);
            int compare2kFrom = DateTime.Compare(From, year2k);
            int compare2kTo = DateTime.Compare(To, year2k);
            if (compare > 0)
            {
                DatesToFrom.Text = "'from' cant be later than 'to'!";
                dateOk = false;
                return;
            }
            else if (compare2kFrom < 0 || compare2kTo < 0)
            {
                DatesToFrom.Text = "date must be after 2000!";
                dateOk = false;
                return;
            } else
            {
                DatesToFrom.Text = $"date ok! from {From.Day}.{From.Month}.{From.Year} to {To.Day}.{To.Month}.{To.Year} ";
                dateOk = true;
            }

            if (User == "")
            {
                LabelUsername.Text = "enter some username!";
                userOk = false;
                return;
            }
            else
            {
                LabelUsername.Text = User + "\t [user ok!]";
                userOk = true;
            }

        }

        private void OnTextEntered(object sender, EventArgs e)
        {
            
            this.User = TextEntry.Text;
 
        }

        private void OnDateSelectedFrom(object sender, DateChangedEventArgs e)
        {
            From = DatePickerFrom.Date; // zaczynamy od pierwszej sekundy dnia
        }
        private void OnDateSelectedTo(object sender, DateChangedEventArgs e)
        {
            To = DatePickerTo.Date.Add(new TimeSpan(23, 59, 59)); // konczymy na ostatniej sekundzie dnia

        }

        private async void OnProgramRun(object sender, EventArgs e)
        {
            try
            {
                if (!userOk || !dateOk)
                {
                    LabelUsername.Text = "data not validated, try again!";
                    return;
                }
                LabelUsername.Text = "button pressed";

                //if (User == "")
                //{
                //    LabelUsername.Text = "enter some username!";
                //    return;
                //} else
                //{
                //    // nic
                //}


                LabelUsername.Text = "running...";



                await GetRecentTracksByDateSpan(Api, User, From, To);

                LabelUsername.Text = "fetching top albums...";

                TopAlbums = await Api.GetTopAlbums(n, From, To);
                LabelUsername.Text = $"top {n} albums fetched!";

                for (int i = 0; i < TopAlbums.Count; i++)
                {
                    string description = $"{TopAlbums[i].Item1} repeats \r\n {TopAlbums[i].Item2}";
                    _imageItems[i] = new ImageItem { ImageUrl = TopAlbums[i].Item3, Text = description };
                }
                LabelUsername.Text = "fetching top artists...";

                var TopArtists = await Api.GetTopArtists(n, From, To);
                LabelUsername.Text = $"top {n} albums fetched!";

                string Artists = "";
                foreach (var artist in TopArtists)
                {
                    Artists += ($"({artist.Item1}) {artist.Item2}\r\n");
                }
                TextBoxArtists.Text = Artists;



                LabelUsername.Text = "All done!";
            }
            catch (Exception ex)
            {
                LabelUsername.Text = ex.Message;
            }



        }


        public async Task GetRecentTracksByDateSpan(ApiTest a, string User, DateTime From, DateTime To)
        {
            int Year, Month, Day;
            DateTime currentDate = From;
            int AllDays = (int)(To - From).TotalDays + 1;
            int DayCounter = 1;
            while (currentDate <= To)
            {
                Year = currentDate.Year;
                Month = currentDate.Month;
                Day = currentDate.Day;

                LabelUsername.Text = $"fetching {currentDate.Day}.{currentDate.Month}.{currentDate.Year} \t {DayCounter}/{AllDays} ...";
                await a.GetRecentTracksByDay(User, Year, Month, Day);

                currentDate = currentDate.AddDays(1);
                DayCounter++;
            }
            LabelUsername.Text = "data saved to database";
        }

    }




    public class ImageItem
    {
        public string ImageUrl { get; set; }
        public string Text { get; set; }
    }


}


