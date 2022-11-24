using System;
using Flurl;
using Flurl.Http;
using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Text;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Post
    {
        public string title { get; set; }
        public string author { get; set; }
        public string link { get; set; }
        public string date { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"title:{title}");
            sb.Append($"author:{author}");
            sb.Append($"link:{link}");
            sb.Append($"date:{date}");
            return sb.ToString();
        }
    }
    internal class Program
    {
        static string ROOT = "https://www.uberpeople.net/";
        private static CookieSession session = new CookieSession(ROOT);
        static ConcurrentBag<Post> items = new System.Collections.Concurrent.ConcurrentBag<Post>();
        static async Task Main(string[] args)
        {
            Console.WriteLine("Assignment 2 VAN HOU HEI");
            // detnoe we need to get the page number
            ///forums/Chicago/page-2?sorting=latest-activity
            var isAllDone = await Task.WhenAll(Enumerable.Range(1, 404).Select(y => CrawlPage(y)));
            if (isAllDone.Any())
            {
                Console.WriteLine($"saving -- {items.Count}");
                await File.WriteAllTextAsync("log.json", JsonConvert.SerializeObject(items));
            }


        }
        static async Task<bool> CrawlPage(int page)
        {
          
            try
            {
                Console.WriteLine($"crawling---{page}");
                var response = await session.Request($"forums/Chicago/page-{page}?sorting=latest-activity").GetStringAsync();
                return await ParseHtml(response);
            }
            catch
            {
                Console.WriteLine($"error occur : {page}");
                
                return false;
            }
        }
        static async Task<bool> ParseHtml(string html)
        {
            try
            {
                var parser = new HtmlParser();
                var doc = await parser.ParseDocumentAsync(html);
                doc.QuerySelectorAll("div.california-thread-item").ToList().ForEach(y =>
                {
                    try
                    {
                        var t = y.QuerySelector("div.structItem-title");
                        var l = y.QuerySelector("div.structItem-title a.thread-title--gtm");
                        var a = y.QuerySelector("ul.structItem-parts li.structItem-username a.username.thread-details-profile-link--gtm");
                        var d = y.QuerySelector("ul.structItem-parts li.structItem-startDate a.start-date time.thread-time--gtm.u-dt");
                        var  title = t==null ?"" : t.TextContent;
                        var link = l==null ? "" :  l.GetAttribute("href");
                       var author = a==null? "" :a.TextContent;
                       var date =  d==null?"":d.TextContent;
                        var p = new Post() { title = title, link = link, author = author, date = date };
                        Console.WriteLine(p);
                        items.Add(p);
                    }
                    catch
                    {
                        
                    }
                });
                    
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}