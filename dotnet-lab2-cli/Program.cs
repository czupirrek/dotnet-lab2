namespace dotnet_lab2_cli
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ApiTest apiTest = new ApiTest();
            //apiTest.GetRecentTracks("czupirrek", 1710199047, 1710285447).Wait();
            //apiTest.GetRecentTracksByDay("czupirrek", 2021, 10, 10);
            //apiTest.GetRecentTracksByDay("czupirrek", 2021, 10, 11);
            //apiTest.GetRecentTracksByDay("czupirrek", 2021, 10, 12);

            DateTime From = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime To = new DateTime(2025, 1, 10, 23, 59, 59, DateTimeKind.Utc);

            apiTest.GetRecentTracksByDateSpan("czupirrek", From, To).Wait();

            int n = 10;

            var TopArtists = apiTest.GetTopArtists(n, From, To);

            var TopAlbums = apiTest.GetTopAlbums(n, From, To);

            Console.WriteLine($"Top {n} artists:");
            foreach (var artist in TopArtists.Result)
            {
                Console.WriteLine($"{artist.Item1}. {artist.Item2}. {artist.Item3}");
            }

            Console.WriteLine($"Top {n} albums:");
            foreach (var album in TopAlbums.Result)
            {
                Console.WriteLine($"{album.Item1} {album.Item2} {album.Item3}");
            }




        }
    }
}
