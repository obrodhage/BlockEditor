//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Dragginz.Scripts.Data;

namespace Dragginz.Scripts.Creator
{
    public class CreateShapeObjects : MonoBehaviour
    {
        public List<Transform > trfmSets;
        public InputField textOutput;
        
        private BlockShapes _blockShapes;

        private List<string> _duplicatesCheckList;
        
        // ----------------------------------------------------------------------------------------
        private void Start()
        {
            if (trfmSets == null || trfmSets.Count <= 0) return;

            _duplicatesCheckList = new List<string>();
            
            _blockShapes = new BlockShapes {shapeSets = new List<ShapeSet>()};

            var len = trfmSets.Count;
            for (var i = 0; i < len; ++i)
            {
                _blockShapes.shapeSets.Add(new ShapeSet());
                _blockShapes.shapeSets[i].id = trfmSets[i].name;

                GetTypesForSet(i, trfmSets[i]);
            }

            CheckForDuplicateShapes();
            
            textOutput.text = _blockShapes.GetJsonString();
        }
        
        // ----------------------------------------------------------------------------------------
        private void GetTypesForSet(int setIndex, Transform trfmSet)
        {
            //Debug.Log(trfmTypes.name);

            _blockShapes.shapeSets[setIndex].shapeTypes = new List<ShapeType>();
            
            var typeIndex = 0;
            foreach (Transform type in trfmSet)
            {
                _blockShapes.shapeSets[setIndex].shapeTypes.Add(new ShapeType());
                _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].id = type.name;

                GetShapesForType(setIndex, typeIndex, type);

                typeIndex++;
                
                //Debug.Log(child.name);
            }
        }
        
        // ----------------------------------------------------------------------------------------
        private void GetShapesForType(int setIndex, int typeIndex, Transform trfmType)
        {
            _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects = new List<ShapeObject>();

            var meshFilters = trfmType.GetComponentsInChildren<MeshFilter>();
            var len = meshFilters.Length;
            for (var i = 0; i < len; ++i)
            {
                _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects.Add(new ShapeObject());
                _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects[i].id = meshFilters[i].transform.name;
                
                GetShapeObject(setIndex, typeIndex, i, meshFilters[i]);
            }
        }

        // ----------------------------------------------------------------------------------------
        private void GetShapeObject(int setIndex, int typeIndex, int objIndex, MeshFilter meshFilter)
        {
            var mesh = meshFilter.mesh;

            // VERTICES
            
            var len = mesh.vertices.Length;
            
            var sVerts = "";
            var sUVs = "";
            var sNormals = "";

            Vector2 v2;
            Vector3 v3;
            
            for (var i = 0; i < len; ++i)
            {
                if (i > 0)
                {
                    sVerts += ";";
                    sUVs += ";";
                    sNormals += ";";
                }

                v3 = mesh.vertices[i];
                sVerts += v3.x + "," + v3.y + "," + v3.z;
                
                v2 = mesh.uv[i];
                sUVs += Math.Round(v2.x, 2) + "," + Math.Round(v2.y, 2);
                
                v3 = mesh.normals[i];
                sNormals += Math.Round(v3.x, 2) + "," + Math.Round(v3.y, 2) + "," + Math.Round(v3.z, 2);
            }

            _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects[objIndex].vertices = sVerts;
            _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects[objIndex].uvs = sUVs;
            _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects[objIndex].normals = sNormals;
            
            _duplicatesCheckList.Add(sVerts);

            // TRIANGLES
            
            var sTris = string.Join(",", mesh.triangles);

            _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects[objIndex].triangles = sTris;
            
            // UVS
            
            /*len = mesh.uv.Length;
            var sUVs = "";
            for (var i = 0; i < len; ++i)
            {
                if (i > 0) sUVs += ";";

                var v2 = mesh.uv[i];
                sUVs += v2.x + "," + v2.y;
            }
            
            _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects[objIndex].uvs = sUVs;
            
            // NORMALS
            
            len = mesh.normals.Length;
            var sNormals = "";
            for (var i = 0; i < len; ++i)
            {
                if (i > 0) sNormals += ";";

                var v3 = mesh.normals[i];
                sNormals += v3.x + "," + v3.y + "," + v3.z;
            }
            
            _blockShapes.shapeSets[setIndex].shapeTypes[typeIndex].shapeObjects[objIndex].normals = sNormals;
            */
        }
        
        // ----------------------------------------------------------------------------------------
        private void CheckForDuplicateShapes() 
        {
            Debug.Log("CheckForDuplicateShapes - num shapes: " + _duplicatesCheckList.Count);
            
            var checks = 0;
            var len = _duplicatesCheckList.Count;
            for (var i = 0; i < len; ++i)
            {
                for (var j = 0; j < len; ++j)
                {
                    if (i != j && _duplicatesCheckList[i].Equals(_duplicatesCheckList[j]))
                    {
                        Debug.LogWarning("duplicate found: " + i + " == " + j);
                    }

                    checks++;
                }
            }
            
            Debug.LogWarning("duplicate checks: " + checks);
        }
    }
}