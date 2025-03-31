using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


namespace dotnet_lab2_cli
{
    public class ApiTest
    {
        string ApiKey = "039b74b22e1ed85f28229cae448df8f7";
        public HttpClient client;
        List<Track> AllTracks = new List<Track>();

        public async Task GetRecentTracks(string User, long From, long To)
        {
            // sprawdz czy data jest w bazie danych, jesli jest - zakoncz funkcje
            if (await IsDateInDatabase((From+To)/2))
            {
                Console.WriteLine("Data already in database.");
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            client = new HttpClient();
            string RootURL = "http://ws.audioscrobbler.com/2.0/";
            string Method = "?method=user.getrecenttracks";
            string Username = User;
            int Page = 1;
            int AllPages = 999;
            bool HasMorePages = true;

            while (HasMorePages)
            {
                string CallURL = $"{RootURL}{Method}&user={User}&api_key={ApiKey}&format=json&from={From}&to={To}&page={Page}";
                string JsonApiResponse = await client.GetStringAsync(CallURL);
                Console.WriteLine(JsonApiResponse);
                RecentTracksRoot StructuredApiResponse = JsonSerializer.Deserialize<RecentTracksRoot>(JsonApiResponse);
                AllPages = int.Parse(StructuredApiResponse.recenttracks.attr.totalPages);
                Console.WriteLine(StructuredApiResponse.recenttracks.attr);

                AllTracks.AddRange(StructuredApiResponse.recenttracks.track);

                if (Page >= AllPages)
                {
                    HasMorePages = false;
                }

                Page++;
            }

            // Tracki z atrybutem nowplaying są zwracane ZAWSZE, nawet jesli okreslono inny zakres dat.
            // Aby uniknąć nieporozumień, usuwam je z listy.
            AllTracks.RemoveAll(track => track.attr?.nowplaying == "true");

            int counter = 0;

            foreach (Track track in AllTracks)
            {
                counter++;
                Console.WriteLine(track);
            }
            Console.WriteLine("Total tracks: " + counter);
            await SaveTracksToDatabase(AllTracks);
        }
    


  

    private async Task SaveTracksToDatabase(List<Track> tracks)
        {
            using (var context = new LastfmContext())
            {
                foreach (var track in tracks)
                {
                    var dbTrack = new DbTrack
                    {
                        ArtistName = track.artist.text,
                        TrackName = track.name,
                        Album = track.album.text,
                        Date = track.date?.text,
                        AlbumMbid = track.album.mbid
                    };

                    // Sprawdź, czy artysta już istnieje w bazie danych
                    var dbArtist = await context.Artists.FirstOrDefaultAsync(a => a.ArtistName == track.artist.text);
                    if (dbArtist == null)
                    {
                        dbArtist = new DbArtist
                        {
                            ArtistName = track.artist.text,
                            ImageUrl = track.image.FirstOrDefault()?.text
                        };
                        context.Artists.Add(dbArtist);
                        await context.SaveChangesAsync();
                    }

                    dbTrack.ArtistId = dbArtist.Id;
                    context.Tracks.Add(dbTrack);
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task<bool> IsDateInDatabase(long timestamp)
        {
            using (var context = new LastfmContext())
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime.Date;

                var tracks = await context.Tracks
                    .Where(t => t.Date != null && t.Date != "") // Filtr pustych wartości
                    .ToListAsync(); // Pobierz listę asynchronicznie

                return tracks.Any(t => DateTime.TryParseExact(
                    t.Date,
                    "dd MMM yyyy, HH:mm", // Dopasowanie do twojego formatu
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate) && parsedDate.Date == date);
            }
        }




        public async Task GetRecentTracksByDay(string User, int Year, int Month, int Day)
        {
            DateTime From = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc);
            DateTime To = new DateTime(Year, Month, Day, 23, 59, 59, DateTimeKind.Utc);
            long FromUnix = ((DateTimeOffset)From).ToUnixTimeSeconds();
            long ToUnix = ((DateTimeOffset)To).ToUnixTimeSeconds();

            await GetRecentTracks(User, FromUnix, ToUnix);
        }

        public async Task GetRecentTracksByDateSpan(string User, DateTime From, DateTime To)
        {
            int Year, Month, Day;
            DateTime currentDate = From;

            while (currentDate <= To)
            {
                Year = currentDate.Year;
                Month = currentDate.Month;
                Day = currentDate.Day;


                await GetRecentTracksByDay(User, Year, Month, Day);

                currentDate = currentDate.AddDays(1);
            }
        }
    }


    

}
    


