using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace PianoSyllabusScraper.Model.Scrapers
{
    internal class ComposerScraper
    {
        private HttpClient httpClient;
        private PieceScraper pieceScraper;

        public ComposerScraper(HttpClient httpClient, PieceScraper pieceScraper)
        {
            this.httpClient = httpClient;
            this.pieceScraper = pieceScraper;
        }

        private async Task<List<string>> ScrapeAllNationalities()
        {
            List<string> nationalities = new();

            HttpResponseMessage response = await httpClient.GetAsync("x-composers.php");
            string html = await response.Content.ReadAsStringAsync();

            HtmlDocument doc = new();
            doc.LoadHtml(html);

            HtmlNode selectedNode = doc.DocumentNode.SelectSingleNode("//select[@name='nation']");

            foreach (HtmlNode option in selectedNode.ChildNodes)
            {
                if (option.NodeType == HtmlNodeType.Element && option.InnerText != "select nationality")
                {
                    nationalities.Add(option.Attributes["value"].Value);
                }
            }

            return nationalities;
        }

        public async Task ScrapeAllComposersAsync(string basePath)
        {
            List<string> nationalities = await ScrapeAllNationalities();

            foreach (string nationality in nationalities)
            {
                MultipartFormDataContent form = new();
                form.Add(new StringContent(nationality), "nation");
                HttpResponseMessage nationalitySearchResponse = await httpClient.PostAsync("x-composers.php", form);

                string html = await nationalitySearchResponse.Content.ReadAsStringAsync();

                HtmlDocument doc = new();
                doc.LoadHtml(html);

                HtmlNodeCollection selectedNodes = doc.DocumentNode.SelectNodes("//tr[@class='evenrows'] | //tr[@class='oddrows']");
                foreach (HtmlNode composerRow in selectedNodes)
                {
                    List<HtmlNode> composerRowData = composerRow.GetElementNodes("td");

					Composer composer = new Composer();
					composer.AbbrName = composerRowData[0].FirstChild.InnerText;
                    composer.Era = composerRowData[1].InnerText;
                    composer.Nationality = nationality;

                    List<Piece> composedPieces = await pieceScraper.ScrapeAllPiecesBy(composer.AbbrName);

					composer.Name = composedPieces[0].ComposerName;

                    string composerPath = Path.Combine(basePath, composer.AbbrName);

					Directory.CreateDirectory(composerPath);

                    using(FileStream piecesStream = File.Create(Path.Combine(composerPath, "pieces.json"))) {
                        JsonSerializer.SerializeAsync(piecesStream, composedPieces);
                    }

                    using (FileStream composerStream = File.Create(Path.Combine(composerPath, composer.AbbrName + ".json"))) {
                        JsonSerializer.SerializeAsync(composerStream, composer);
                    }

					Console.WriteLine("Composer's info downloaded: " + composer.AbbrName);
				}
            }
        }
    }
}
