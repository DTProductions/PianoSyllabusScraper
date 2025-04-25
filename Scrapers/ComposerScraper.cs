using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PianoSyllabusScraper.Scrapers
{
    internal class ComposerScraper {
		private HttpClient httpClient;

		public ComposerScraper(HttpClient httpClient) {
			this.httpClient = httpClient;
		}

		public async Task<List<string>> scrapeAllNationalities() {
			List<string> nationalities = new List<string>();

			HttpResponseMessage response = await httpClient.GetAsync("x-composers.php");
			string html = await response.Content.ReadAsStringAsync();

			HtmlDocument doc = new();
			doc.LoadHtml(html);

			HtmlNode selectedNode = doc.DocumentNode.SelectSingleNode("//select[@name='nation']");

            foreach (HtmlNode option in selectedNode.ChildNodes) {
				if(option.NodeType == HtmlNodeType.Element && option.InnerText != "select nationality") {
					nationalities.Add(option.Attributes["value"].Value);
				}
			}

			return nationalities;
		}

		public async Task scrapeAllComposersAsync(string basePath) {
		}
	}
}
