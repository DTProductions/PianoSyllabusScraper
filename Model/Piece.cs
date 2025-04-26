using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoSyllabusScraper.Model
{
	public class Piece {
		public string? ComposerName { get; set; }
		public string? Title { get; set;}
		public string? ArrangerName { get; set;}
		public Dictionary<string, string>? Grades { get; set;}
	}
}
