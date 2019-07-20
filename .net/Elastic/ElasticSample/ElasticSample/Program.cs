using System;
using Nest;

namespace ElasticSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var tweet = new Tweet
            {
                Id = 2,
                User = "kimchy",
                PostDate = new DateTime(2009, 11, 15),
                Message = "Trying out NEST, so far so good?"
            };
            Console.WriteLine("Hello World!");
            
            var node = new Uri("http://90.110.16.73:9200");
            var settings = new ConnectionSettings(node);
            var client = new ElasticClient(settings);
            
            var response = client.Index(tweet, idx => idx.Index("mytweetindex"));
        }
    }

    internal class Tweet
    {
        public int Id { get; set; }
        public string User { get; set; }
        public DateTime PostDate { get; set; }
        public string Message { get; set; }
    }
}