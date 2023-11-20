//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;

using Dragginz.Scripts.JSON;

namespace Dragginz.Scripts.Data
{
	[Serializable]
	public class ShapeObject
	{
		private const string Verts   = "v";
		private const string Tris    = "t";
		private const string Uvs     = "u";
		private const string Normals = "n";
		
		public string id { get; set; }
		
		public string vertices { get; set; }
		public string triangles { get; set; }
		public string uvs { get; set; }
		public string normals { get; set; }
		
        //
        // Parse JSON data
        //
        public void ParseJson(JSONNode data)
		{
            id = "";
            if (data["id"] != null) {
	            id = data["id"];
            }
            
            vertices = "";
            if (data[Verts] != null) {
	            vertices = data[Verts];
            }
            
            triangles = "";
            if (data[Tris] != null) {
	            triangles = data[Tris];
            }
            
            uvs = "";
            if (data[Uvs] != null) {
	            uvs = data[Uvs];
            }
            
            normals = "";
            if (data[Normals] != null) {
	            normals = data[Normals];
            }
		}

        //
        // Create JSON string
        //
        public string GetJsonString()
		{
			var s = "{";

			s += "\"id\":" + "\"" + id + "\"";
			s += ","+Verts+":\"" + vertices + "\"";
			s += ","+Tris+":\"" + triangles + "\"";
			s += ","+Uvs+":\"" + uvs + "\"";
			s += ","+Normals+":\"" + normals + "\"";
			
            s += "}";

			return s;
		}
	}
}