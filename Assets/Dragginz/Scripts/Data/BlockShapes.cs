//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;
using System.Collections.Generic;

using Dragginz.Scripts.JSON;

namespace Dragginz.Scripts.Data
{
	[Serializable]
	public class BlockShapes
	{
		public List<ShapeSet> shapeSets { get; set; }

		//
		// Parse JSON data
		//
		public void ParseJson(string json)
		{
			var data = JSON.JSON.Parse(json);

            shapeSets = new List<ShapeSet> ();
			if (data ["sets"] != null) {
				var elements = (JSONArray) data ["sets"];
				if (elements != null)
				{
					var len = elements.Count;
					int i;
					for (i = 0; i < len; ++i) {
						var shapeSet = new ShapeSet ();
						shapeSet.ParseJson (elements [i]);
						shapeSets.Add (shapeSet);
					}
				}
			}
		}

		//
		// Create JSON string
		//
		public string GetJsonString()
		{
			int i;

			var s = "{";

            s += "\"sets\":[";
			var len = shapeSets.Count;
			for (i = 0; i < len; ++i) {
				s += i > 0 ? "," : "";
				s += shapeSets [i].GetJsonString ();
			}
			s += "]";

			s += "}";

			return s;
		}
	}
}