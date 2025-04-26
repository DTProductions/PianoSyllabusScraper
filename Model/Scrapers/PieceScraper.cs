using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoSyllabusScraper.Model.Scrapers
{
    internal class PieceScraper
    {
        private HttpClient httpClient;

        public PieceScraper(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Piece> ScrapePiece(string url)
        {
			HttpResponseMessage pieceResponse = await httpClient.GetAsync("x-detail.php?" + url.Split("?")[1]);
			string html = await pieceResponse.Content.ReadAsStringAsync();

			HtmlDocument pieceDoc = new();
			pieceDoc.LoadHtml(html);

            Piece piece = new Piece();

			HtmlNodeCollection infoRows = pieceDoc.DocumentNode.SelectNodes("//div[@class='flexbox1']/table/tr");
            foreach (HtmlNode row in infoRows) {

                HtmlNode firstCol = row.GetElementNodes()[0];
				HtmlNode secondCol = row.GetElementNodes()[1];

                switch (firstCol.InnerText) {
                    case "Composer":
						piece.ComposerName = secondCol.InnerText;
                        break;
                    case "Arranger":
						piece.ArrangerName = secondCol.InnerText;
                        break;
                    case "Title":
                        piece.Title = secondCol.InnerText;
                        break;
                }
			}

			piece.Grades = ScrapeGrades(pieceDoc);

            return piece;
		}

        public async Task<List<Piece>> ScrapeAllPiecesBy(string abbrComposerName) {
			MultipartFormDataContent form = new();
			form.Add(new StringContent(abbrComposerName), "composer");

			HttpResponseMessage composerSearchResponse = await httpClient.PostAsync("x-default.php", form);
			string html = await composerSearchResponse.Content.ReadAsStringAsync();

			HtmlDocument doc = new();
			doc.LoadHtml(html);

			List<Piece> pieces = new List<Piece>();

			HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//tr[@class='evenrows'] | //tr[@class='oddrows']");
			foreach(HtmlNode row in rows) {
                string pieceUrl = row.FirstChild.FirstChild.Attributes["href"].Value;
				pieces.Add(await ScrapePiece(pieceUrl));
			}

			return pieces;
		}

        /// <param name="doc">Loaded Html Document</param>
        private Dictionary<string, List<string>> ScrapeGrades(HtmlDocument pieceDoc) {
            Dictionary<string, List<string>> grades = new();

			HtmlNode gradesDiv = pieceDoc.DocumentNode.SelectSingleNode("//div[@class='entries']");

			HtmlNodeCollection gradeRows = gradesDiv.SelectNodes("./table[position() > 1]/tr");
			foreach (HtmlNode gradeRow in gradeRows) {
                string gradingSystem = gradeRow.GetElementNodes()[1].InnerText;
                string grade = gradeRow.GetElementNodes()[2].InnerText;

                if (!grades.ContainsKey(gradingSystem)) {
					grades.Add(gradingSystem, new List<string> { grade });
                }
                else {
                    grades[gradingSystem].Add(grade);
                }
			}

            return grades;
		}
	}
}
