using System;
using System.IO;
using System.Threading.Tasks;
using Storage.Net;
using Storage.Net.Blob;

namespace Storage.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IBlobStorage storage = StorageFactory.Blobs.AzureBlobStorage("tredev01", "7J74uKytJJCXUTwO918xilFbU6Ej6nOU4fS90aGlGxIM7W/oNGK4Dzv4FMy6Fl486BCAQ3qURUkst1rUyz1vag==", "tredev01");
            IBlobStorage storage2 = StorageFactory.Blobs.DirectoryFiles(new DirectoryInfo("c:/logs"));
            var id = Guid.NewGuid();

            FileStream stream = File.OpenRead("C:\\logs\\a.log");
//            await storage.WriteAsync("cli1/test1/a.log", stream);
            await storage2.WriteAsync("cli1/test1/a.log", stream);
            System.Console.ReadLine();
        }
    }
}