using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;


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

                    // Sprawdź, czy artysta już istnieje w bazie danych
                    var dbArtist = await context.Artists.FirstOrDefaultAsync(a => a.ArtistName == track.artist.text);
                    if (dbArtist == null)
                    {
                        dbArtist = new DbArtist
                        {
                            ArtistName = track.artist.text,
                            ImageUrl = track.image?[3].text // [3] - indeks obrazka w najwiekszym rozmiarze
                        };
                        context.Artists.Add(dbArtist);
                        await context.SaveChangesAsync();
                    }

                    // Sprawdź, czy album już istnieje w bazie danych
                    var dbAlbum = await context.Albums.FirstOrDefaultAsync(a =>
                        a.AlbumName == track.album.text &&
                        a.Artist == track.artist.text);

                    if (dbAlbum == null)
                    {
                        // Jeśli album nie istnieje, utwórz nowy
                        dbAlbum = new DbAlbum
                        {
                            AlbumName = track.album.text,
                            Artist = track.artist.text,
                            AlbumMbid = track.album.mbid,
                            ImageUrl = track.image?[3].text, // Największy dostępny rozmiar obrazka
                        };
                        context.Albums.Add(dbAlbum);
                        await context.SaveChangesAsync();
                    }

                    var dbTrack = new DbTrack
                    {
                        ArtistName = track.artist.text,
                        TrackName = track.name,
                        Album = track.album.text,
                        Date = track.date?.text,
                        Timestamp = timestamp,
                        AlbumMbid = track.album.mbid,
                        ArtistId = dbArtist.Id,
                        AlbumId = dbAlbum.Id  // Ustawienie relacji z albumem
                    };

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
        public async Task<List<Tuple<int, string, string>>> GetTopArtists(int Limit, DateTime From, DateTime To)
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
                    .Select(g => new
                    {
                        Count = g.Count(),
                        ArtistName = g.Key,
                        ImageUrl = g.First().Artist.ImageUrl
                    })
                    .ToListAsync();
                return topArtists.Select(a => new Tuple<int, string, string>(a.Count, a.ArtistName, a.ImageUrl)).ToList();

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
                    .Select(g => new
                    {
                        Count = g.Count(),
                        AlbumName = g.Key,
                        ImageUrl = g.First().DbAlbum.ImageUrl
                    })
                    .ToListAsync();
                return topAlbums.Select(a => new Tuple<int, string, string>(a.Count, a.AlbumName, a.ImageUrl)).ToList();
            }
        }

        // to sie okazalo totalnie niepotrzebne bo maui moze pobierac obrazki z url ....
        public async Task<List<KeyValuePair<string, string>>> DownloadAlbumCovers(List<Tuple<int, string, string>> TopAlbums)
        {
            string BasePath = "C:\\Users\\czupi\\source\\repos\\dotnet-lab2-cli\\Maui-lastfm\\Maui-lastfm\\Resources\\Images";
            List<KeyValuePair<string, string>> AlbumPath = new List<KeyValuePair<string, string>>();
            foreach (var album in TopAlbums)
            {
                string OutFilename = MD5String(album.Item2 + album.Item3) + ".png";
                string FullPath = System.IO.Path.Combine(BasePath, OutFilename);
                if (!System.IO.File.Exists(FullPath))
                {
                    string Url = album.Item3;

                    Console.WriteLine($"Pobieram {Url} to {FullPath}");
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] fileBytes = await client.GetByteArrayAsync(Url);
                        await System.IO.File.WriteAllBytesAsync(FullPath, fileBytes);
                    }
                }

                AlbumPath.Add(new KeyValuePair<string, string>(album.Item2, FullPath));
            }
            return AlbumPath;
        }



        private string MD5String(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

    }


    

}
    


