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
	public class ShapeSet
	{
		public string id { get; set; }

		public List<ShapeType> shapeTypes { get; set; }

		//
		// Parse JSON data
		//
		public void ParseJson(JSONNode data)
		{
			//var data = JSON.JSON.Parse(json);

			id = "";
			if (data ["id"] != null) {
				id = data ["id"];
			}

            shapeTypes = new List<ShapeType> ();
			if (data ["types"] != null) {
				var elements = (JSONArray) data ["types"];
				if (elements != null)
				{
					var len = elements.Count;
					int i;
					for (i = 0; i < len; ++i) {
						var shapeType = new ShapeType ();
						shapeType.ParseJson (elements [i]);
						shapeTypes.Add (shapeType);
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

			s += "\"id\":" + "\"" + id + "\"";
            s += ",\"types\":[";
			var len = shapeTypes.Count;
			for (i = 0; i < len; ++i) {
				s += i > 0 ? "," : "";
				s += shapeTypes [i].GetJsonString ();
			}
			s += "]";

			s += "}";

			return s;
		}
	}
}