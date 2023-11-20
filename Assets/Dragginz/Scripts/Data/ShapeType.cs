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
	public class ShapeType
	{
		public string id { get; set; }

		public List<ShapeObject> shapeObjects { get; set; }

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

            shapeObjects = new List<ShapeObject> ();
			if (data ["objs"] != null) {
				var elements = (JSONArray) data ["objs"];
				if (elements != null)
				{
					var len = elements.Count;
					int i;
					for (i = 0; i < len; ++i) {
						var shapeObject = new ShapeObject ();
						shapeObject.ParseJson (elements [i]);
						shapeObjects.Add (shapeObject);
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
            s += ",\"objs\":[";
			var len = shapeObjects.Count;
			for (i = 0; i < len; ++i) {
				s += i > 0 ? "," : "";
				s += shapeObjects [i].GetJsonString ();
			}
			s += "]";

			s += "}";

			return s;
		}
	}
}