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

        public async Task GetRecentTracks(string User, long From, long To)
        {
            List<Track> AllTracks = new List<Track>();


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
                //Console.WriteLine(JsonApiResponse);
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
                //Console.WriteLine(track);
            }
            Console.WriteLine("Total tracks: " + counter);
            Console.WriteLine($"alltracks size: {AllTracks.Count}");
            await SaveTracksToDatabase(AllTracks);
        }
    


  

    private async Task SaveTracksToDatabase(List<Track> tracks)
        {
            using (var context = new LastfmContext())
            {
                foreach (var track in tracks)
                {
                    long timestamp = 0;
                    if (track.date?.uts != null)
                    {
                        // Konwersja string na long
                        if (long.TryParse(track.date.uts, out var parsedTimestamp))
                        {
                            timestamp = parsedTimestamp;
                        }
                    }
                    var dbTrack = new DbTrack
                    {
                        ArtistName = track.artist.text,
                        TrackName = track.name,
                        Album = track.album.text,
                        Date = track.date?.text,
                        Timestamp = timestamp,
                        AlbumMbid = track.album.mbid
                    };

                    // Artysta jest dodawany do BD tylko jeśli go tam jeszcze nie ma
                    var dbArtist = await context.Artists.FirstOrDefaultAsync(a => a.ArtistName == track.artist.text);
                    if (dbArtist == null)
                    {
                        dbArtist = new DbArtist
                        {
                            ArtistName = track.artist.text,
                            ImageUrl = track.image?[2].text // [2] - indeks obrazka w rozmiarze large
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
                // Oblicz timestamp początku i końca dnia
                long startOfDayTimestamp = ((DateTimeOffset)date).ToUnixTimeSeconds();
                long endOfDayTimestamp = ((DateTimeOffset)date.AddDays(1).AddSeconds(-1)).ToUnixTimeSeconds();

                Console.WriteLine($"Sprawdzam czy data {date:dd MMM yyyy} jest już w bazie danych...");

                // Przeszukaj bazę danych używając pola Timestamp
                var tracksFromDate = await context.Tracks
                    .Where(t => t.Timestamp >= startOfDayTimestamp && t.Timestamp <= endOfDayTimestamp)
                    .FirstOrDefaultAsync();

                bool exists = tracksFromDate != null;
                Console.WriteLine(exists ?
                    $"Znaleziono utwory z dnia {date:dd MMM yyyy} w bazie danych." :
                    $"Nie znaleziono utworów z dnia{date:dd MMM yyyy} w bazie danych .");

                return exists;
            }
        }






        public async Task GetRecentTracksByDay(string User, int Year, int Month, int Day)
        {
            DateTime From = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc);
            DateTime To = new DateTime(Year, Month, Day, 23, 59, 59, DateTimeKind.Utc);
            long FromUnix = ((DateTimeOffset)From).ToUnixTimeSeconds();
            long ToUnix = ((DateTimeOffset)To).ToUnixTimeSeconds();

            if (await IsDateInDatabase(FromUnix))
            {
                Console.WriteLine("Dzień istnieje już w bazie danych.");
                return;
            } else

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
    

        // Funkcja zwraca listę najczęściej odtwarzanych artystów obecnych w >>zakresie dat<< w bazie danych
        public async Task<List<KeyValuePair<int, string>>> GetTopArtists(int Limit, DateTime From, DateTime To)
        {
            long FromUnix = ((DateTimeOffset)From).ToUnixTimeSeconds();
            long ToUnix = ((DateTimeOffset)To).ToUnixTimeSeconds();

            using (var context = new LastfmContext())
            {
                var topArtists = await context.Tracks
                    .Where(t => t.Timestamp >= FromUnix && t.Timestamp <= ToUnix)
                    .GroupBy(t => t.ArtistName)
                    .OrderByDescending(g => g.Count())
                    .Take(Limit)
                    .Select(g => new KeyValuePair<int, string>(g.Count(), g.Key))
                    .ToListAsync();
                return topArtists;

            }
        }

        public async Task<List<Tuple<int, string, string>>> GetTopAlbums(int Limit, DateTime From, DateTime To)
        {
            long FromUnix = ((DateTimeOffset)From).ToUnixTimeSeconds();
            long ToUnix = ((DateTimeOffset)To).ToUnixTimeSeconds();
            using (var context = new LastfmContext())
            {
                var topAlbums = await context.Tracks
                    .Where(t => t.Timestamp >= FromUnix && t.Timestamp <= ToUnix)
                    .GroupBy(t => t.Album)
                    .OrderByDescending(g => g.Count())
                    .Take(Limit)
                    .Select(g => new Tuple<int, string, string>(g.Count(), g.Key, g.First().ArtistName))
                    .ToListAsync();
                return topAlbums;
            }
        }

    }


    

}
    


