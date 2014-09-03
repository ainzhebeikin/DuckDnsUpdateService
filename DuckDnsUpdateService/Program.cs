namespace DuckDnsUpdateService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceMainHelper.Run<Service>(args);
        }
    }
}