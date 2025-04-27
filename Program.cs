using PianoSyllabusScraper.Model;
using PianoSyllabusScraper.Model.Scrapers;
using System.Net.Sockets;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PianoSyllabusScraper
{
    internal class Program {
        static async Task Main(string[] args) {
            HttpClient httpClient = new(
                    new SocketsHttpHandler() {
                        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
                    }
                );
            httpClient.BaseAddress = new Uri("https://pianosyllabus.com");

			PieceScraper pieceScraper = new(httpClient);
			ComposerScraper composerScraper = new(httpClient, pieceScraper);

            await composerScraper.ScrapeAllComposersAsync(@"C:\piano_syllabus_test");

            Console.WriteLine("Data scraped");
			Console.ReadLine();
        }
    }
}
