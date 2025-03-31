namespace dotnet_lab2_cli
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ApiTest apiTest = new ApiTest();
            //apiTest.GetRecentTracks("czupirrek", 1710199047, 1710285447).Wait();
            apiTest.GetRecentTracksByDay("czupirrek", 2021, 10, 10);
            apiTest.GetRecentTracksByDay("czupirrek", 2021, 10, 11);
            apiTest.GetRecentTracksByDay("czupirrek", 2021, 10, 12);




        }
    }
}
