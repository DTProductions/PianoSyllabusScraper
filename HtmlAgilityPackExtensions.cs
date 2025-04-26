using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoSyllabusScraper {
	internal static class HtmlAgilityPackExtensions {
		/// <summary>
		/// Retrieves only the element nodes for this root node
		/// </summary>
		public static List<HtmlNode> GetElementNodes(this HtmlNode rootNode) {
			return rootNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element).ToList();
		}

		/// <summary>
		/// Retrieves only the element nodes with the specified name for this root node
		/// </summary>
		public static List<HtmlNode> GetElementNodes(this HtmlNode rootNode, string nodeName) {
			return rootNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element && n.Name == nodeName).ToList();
		}
	}
}
