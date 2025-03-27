namespace dotnet_lab2_cli
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ApiTest apiTest = new ApiTest();
            apiTest.GetRecentTracks().Wait();
        }
    }
}
