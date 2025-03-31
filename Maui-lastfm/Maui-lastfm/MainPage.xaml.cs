using dotnet_lab2_cli;
namespace Maui_lastfm
{
    public partial class MainPage : ContentPage
    {
        string User;
        DateTime From;
        DateTime To;
        ApiTest Api;
        public MainPage()
        {
            Api = new ApiTest();
            InitializeComponent();
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
            LabelUsername.Text = "DONE...";



        }
    }

}
