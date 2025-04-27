using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections;

namespace PianoSyllabusScraper.Model.Scrapers
{
    internal class ComposerScraper {
        private HttpClient httpClient;
        private PieceScraper pieceScraper;

        public ComposerScraper(HttpClient httpClient, PieceScraper pieceScraper) {
            this.httpClient = httpClient;
            this.pieceScraper = pieceScraper;
        }

        private async Task<List<string>> ScrapeAllNationalities() {
            List<string> nationalities = new();

            HttpResponseMessage response = await httpClient.GetAsync("x-composers.php");
            string html = await response.Content.ReadAsStringAsync();

            HtmlDocument doc = new();
            doc.LoadHtml(html);

            HtmlNode selectedNode = doc.DocumentNode.SelectSingleNode("//select[@name='nation']");

            foreach (HtmlNode option in selectedNode.ChildNodes) {
                if (option.NodeType == HtmlNodeType.Element && option.InnerText != "select nationality") {
                    nationalities.Add(option.Attributes["value"].Value);
                }
            }

            return nationalities;
        }

        public async Task ScrapeAllComposersAsync(string basePath) {
            List<string> nationalities = await ScrapeAllNationalities();
            
            foreach (string nationality in nationalities) {
                MultipartFormDataContent form = new();
                form.Add(new StringContent(nationality), "nation");
                HttpResponseMessage nationalitySearchResponse = await httpClient.PostAsync("x-composers.php", form);

                string html = await nationalitySearchResponse.Content.ReadAsStringAsync();

                HtmlDocument doc = new();
                doc.LoadHtml(html);

                HtmlNodeCollection selectedNodes = doc.DocumentNode.SelectNodes("//tr[@class='evenrows'] | //tr[@class='oddrows']");
                foreach (HtmlNode composerRow in selectedNodes) {
					Composer composer = LoadComposerRowInfo(composerRow);
					composer.Nationality = nationality;

					string composerPath = Path.Combine(basePath, composer.AbbrName.TrimEnd('.').Trim());

					await SaveComposerInfoToFile(composer, composerPath);
				}
            }

            await ScrapeComposersWithNoNationality(basePath);
		}

        private async Task ScrapeComposersWithNoNationality(string basePath) {
			HttpResponseMessage allComposersResponse = await httpClient.GetAsync("x-composers.php");

			string html = await allComposersResponse.Content.ReadAsStringAsync();

			HtmlDocument doc = new();
			doc.LoadHtml(html);

			HtmlNodeCollection selectedNodes = doc.DocumentNode.SelectNodes("//tr[@class='evenrows'] | //tr[@class='oddrows']");
            foreach (HtmlNode composerRow in selectedNodes) {
				Composer composer = LoadComposerRowInfo(composerRow);

				string composerPath = Path.Combine(basePath, composer.AbbrName.TrimEnd('.').Trim());
				if (!Directory.Exists(composerPath)) {
                    await SaveComposerInfoToFile(composer, composerPath);
				}
			}
		}

        private async Task SaveComposerInfoToFile(Composer composer, string composerPath) {
			List<Piece> composedPieces = await pieceScraper.ScrapeAllPiecesBy(composer.AbbrName);

			if (composedPieces != null && composedPieces.Count > 0) {
				composer.Name = composedPieces[0].ComposerName;

				Directory.CreateDirectory(composerPath);

				using (FileStream piecesStream = File.Create(Path.Combine(composerPath, "pieces.json"))) {
					await JsonSerializer.SerializeAsync(piecesStream, composedPieces);
				}

				using (FileStream composerStream = File.Create(Path.Combine(composerPath, composer.AbbrName.TrimEnd('.') + ".json"))) {
					await JsonSerializer.SerializeAsync(composerStream, composer);
				}

				Console.WriteLine("Composer's info downloaded: " + composer.AbbrName);
			}
		}

        private Composer LoadComposerRowInfo(HtmlNode composerTrNode) {
			Composer composer = new Composer();

			List<HtmlNode> composerRowData = composerTrNode.GetElementNodes("td");

			composer.AbbrName = composerRowData[0].FirstChild.InnerText;

            if (String.IsNullOrWhiteSpace(composerRowData[1].InnerText)) {
                composer.Era = null;
            } else {
                composer.Era = composerRowData[1].InnerText;
			}

            return composer;
		}
	}
}
