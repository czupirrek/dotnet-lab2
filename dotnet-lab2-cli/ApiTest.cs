using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Text.Json;


namespace dotnet_lab2_cli
{
    class ApiTest
    {
        string ApiKey = "039b74b22e1ed85f28229cae448df8f7";
        public HttpClient client;
        public async Task GetRecentTracks()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            client = new HttpClient();
            string RootURL = "http://ws.audioscrobbler.com/2.0/";
            string Method = "?method=user.getrecenttracks";
            string Username = "czupirrek";
            string CallURL = RootURL + Method + "&user=" + Username + "&api_key=" + ApiKey + "&format=json";
            string response = await client.GetStringAsync(CallURL);
            Console.WriteLine(response);


            RecentTracksRoot ApiResponse = JsonSerializer.Deserialize<RecentTracksRoot>(response);


            foreach (Track track in ApiResponse.recenttracks.track)
            {
                //string r = "track name: " +  track.name + "\t by artist: " + track.artist.text + "\t from album: " + track.album.text + "\t at time: " + track.date.text;
                Console.WriteLine(track);
                //Console.WriteLine(track);
            }

            //Console.WriteLine("moze cos sie stalo");
        }
    }

  
}
