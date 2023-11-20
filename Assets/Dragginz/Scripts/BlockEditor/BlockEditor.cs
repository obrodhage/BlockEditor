//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Dragginz.Scripts.Cams;
using Dragginz.Scripts.Tools;
using Dragginz.Scripts.ShapeFactory;

namespace Dragginz.Scripts.BlockEditor
{
    // --------------------------------------------------------------------------------------------
    public struct BlockElement
    {
        public string Id;
        public GameObject Go;
        public int IndexSet;
        public int IndexType;
        public int IndexShape;
        public int IndexSize;
        public int IndexMaterial;
        public float Size;
        public Vector3 Pos;
    }
    
    // --------------------------------------------------------------------------------------------
    public class BlockEditor : MonoBehaviour
    {
        public Camera cam;
        public EditorCam editorCam;
        public Transform fpsController;
        public MeshRenderer gridRenderer;
        
        public ShapesFactory shapesFactory;
        public ExtendGizmo.Scripts.ExtendGizmo extendGizmo;
        
        public Text textPos;
        public Text textHelp;
        
        public Button buttonBuild;
        public Button buttonDraw;
        public Button buttonExtend;
        public Button buttonColour;
        public Button buttonCopy;
        public Button buttonDelete;
        
        public Button buttonUndo;
        public Text captionUndo;

        public Dropdown dropdownSets;
        public Dropdown dropdownTypes;
        public Dropdown dropdownShapes;
        public Dropdown dropdownSizes;
        public Dropdown dropdownMaterials;

        public LineRenderer raycastLineRenderer;
        public BoundsLineRenderer boundsRendererRaycastElement;
        
        public Transform containerBlocks;
        public Transform containerDrawBlocks;
        public Transform containerDeletedBlocks;
        
        //

        private enum EditMode
        {
            Null,
            Buidl,
            Draw,
            Extend,
            Colour,
            Copy,
            Delete
        }

        private struct UndoStep
        {
            public EditMode Mode;
            public List<string> BlockIds;
            public int MaterialIndex;
        }
        private List<UndoStep> _undoSteps;
        
        private EditMode _editMode;
        
        private int _indexSet;
        private int _indexType;
        private int _indexShape;
        private int _indexSize;
        private int _indexMaterial;

        private List<Material> _materials;
        
        private GameObject _goEditBlock;
        private Vector3 _v3Offset;

        private int _blockCounter;
        private Dictionary<string, BlockElement> _blockElements;
        private Dictionary<string, BlockElement> _drawBlockElements;
        private Dictionary<string, BlockElement> _deletedElements;

        //private List<string> _undoBlockIds;
        
        //
        
        private float _mousewheel;
        private float _timer;
        private float _lastMouseWheelUpdate;
        
        private int _curAlphaPressed;
        private int _curExtrudeStep;
        
        private Vector3 _v3EditPos;
        private Ray _ray;
        private RaycastHit _hit;
        private GameObject _goHit;
        private GameObject _goLastHit;

        private Bounds _collBounds;
        private Vector3 _v3CollCenter;
        private Vector3 _v3CollExtents;
        private Collider[] _colliders;
        private bool _collision;

        private BlockElement _selectedElement;
        private BlockElement _extrudeFromElement;
        private Material _selectedMaterial;
        private Material _drawMaterial;

        private bool _updateAfterDropdownValueChange;
        
        // ----------------------------------------------------------------------------------------
        private void Awake()
        {
            Application.targetFrameRate = 60;
        }
        
        // ----------------------------------------------------------------------------------------
        private void Start()
        {
            _editMode = EditMode.Null;

            SetTransformGizmo(false);

            _curAlphaPressed = -1;
            _curExtrudeStep = 0;
                
            _indexSet      = 0;
            _indexType     = 0;
            _indexShape    = 0;
            _indexSize     = 1;
            _indexMaterial = 0;
            
            _blockCounter  = 0;
            _blockElements     = new Dictionary<string, BlockElement>();
            _drawBlockElements = new Dictionary<string, BlockElement>();
            _deletedElements   = new Dictionary<string, BlockElement>();
            
            _undoSteps = new List<UndoStep>();
            //_undoBlockIds = new List<string>();
            
            _selectedElement = new BlockElement();
            _extrudeFromElement = new BlockElement();
            
            _v3EditPos = Vector3.zero;
            
            _collBounds = new Bounds();
            _v3CollCenter = Vector3.zero;
            _colliders = new Collider[64];
            
            shapesFactory.Init();
            
            shapesFactory.PopulateSetsDropdown(dropdownSets);
            shapesFactory.PopulateTypesDropdown(dropdownTypes, _indexSet);
            shapesFactory.PopulateShapesDropdown(dropdownShapes, _indexSet, _indexType);
            shapesFactory.PopulateSizesDropdown(dropdownSizes);

            LoadMaterials();
            
            raycastLineRenderer.startWidth    = 0.01f;
            raycastLineRenderer.endWidth      = 0.01f;
            raycastLineRenderer.positionCount = 2;

            _updateAfterDropdownValueChange = true;
            
            buttonUndo.interactable = false;
            
            GetNewEditBlock();
            SetGridSize();
            SetEditMode(EditMode.Buidl);
        }

        // ----------------------------------------------------------------------------------------
        private void Update()
        {
            _timer = Time.realtimeSinceStartup;
            _mousewheel = Input.GetAxis ("Mouse ScrollWheel");

            if (_mousewheel != 0) {
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    editorCam.MouseWheelMove (_mousewheel);
                }
            }

            if (ShortCutCheck()) return;
            
            if (_editMode == EditMode.Null) return;
            
            DoRayCast(cam);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetEditMode(EditMode.Null);
                return;
            }

            if (_editMode == EditMode.Buidl || _editMode == EditMode.Draw)
            {
                UpdateBuidlMode();
            }
            else if (_editMode == EditMode.Extend)
            {
                DoAlphaInputCheck();
                UpdateExtendMode();
            }
            else if (_editMode == EditMode.Colour)
            {
                UpdateColourMode();
            }
            else if (_editMode == EditMode.Copy)
            {
                UpdateCopyMode();
            }
            else if (_editMode == EditMode.Delete)
            {
                UpdateDeleteMode();
            }
        }

        // ------------------------------------------------------------------------
        private bool ShortCutCheck()
        {
            if (Input.GetKeyDown(KeyCode.B)) {
                SetEditMode(EditMode.Buidl);
            }
            else if (Input.GetKeyDown(KeyCode.R)) {
                SetEditMode(EditMode.Draw);
            }
            else if (Input.GetKeyDown(KeyCode.X)) {
                SetEditMode(EditMode.Extend);
            }
            else if (Input.GetKeyDown(KeyCode.C)) {
                SetEditMode(EditMode.Colour);
            }
            else if (Input.GetKeyDown(KeyCode.P)) {
                SetEditMode(EditMode.Copy);
            }
            else if (Input.GetKeyDown(KeyCode.L)) {
                SetEditMode(EditMode.Delete);
            }
            else {
                return false;
            }
            
            return true;
        }
        
        // ------------------------------------------------------------------------
        private void DoAlphaInputCheck()
        {
            if (_selectedElement.Go == null)
            {
                extendGizmo.gameObject.SetActive(false);
                return;
            }

            if (_curAlphaPressed == -1)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) {
                    _curAlphaPressed = 1;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                    _curAlphaPressed = 2;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                    _curAlphaPressed = 3;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                    _curAlphaPressed = 4;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5)) {
                    _curAlphaPressed = 5;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6)) {
                    _curAlphaPressed = 6;
                }

                //
                if (_curAlphaPressed != -1)
                {
                    _curExtrudeStep = 0;
                    
                    extendGizmo.gameObject.SetActive(true);
                    extendGizmo.transform.position = _selectedElement.Pos + 
                                                        new Vector3(
                                                            _selectedElement.Size * .5f,
                                                            _selectedElement.Size * .5f,
                                                            _selectedElement.Size * -.5f);
                    extendGizmo.SetAxis(_curAlphaPressed);
                }
            }
            else
            {
                if (Input.GetKeyUp(KeyCode.Alpha1)) {
                    if (_curAlphaPressed == 1) _curAlphaPressed = -1;
                }
                else if (Input.GetKeyUp(KeyCode.Alpha2)) {
                    if (_curAlphaPressed == 2) _curAlphaPressed = -1;
                }
                else if (Input.GetKeyUp(KeyCode.Alpha3)) {
                    if (_curAlphaPressed == 3) _curAlphaPressed = -1;
                }
                else if (Input.GetKeyUp(KeyCode.Alpha4)) {
                    if (_curAlphaPressed == 4) _curAlphaPressed = -1;
                }
                else if (Input.GetKeyUp(KeyCode.Alpha5)) {
                    if (_curAlphaPressed == 5) _curAlphaPressed = -1;
                }
                else if (Input.GetKeyUp(KeyCode.Alpha6)) {
                    if (_curAlphaPressed == 6) _curAlphaPressed = -1;
                }

                //
                if (_curAlphaPressed == -1) extendGizmo.gameObject.SetActive(false);
            }
        }
        
        // ------------------------------------------------------------------------
        private void UpdateBuidlMode()
        {
            if (_goHit != null)
            {
                var v3Normals = _hit.normal;
                
                if (_goHit.CompareTag("Block"))
                {
                    if (v3Normals.x > 0) {
                        v3Normals.y = v3Normals.z = 0.5f;
                    }
                    else if (v3Normals.x < 0) {
                        v3Normals.x = -.49f;
                        v3Normals.y = v3Normals.z = 0.5f;
                    }
                    else if (v3Normals.y > 0) {
                        v3Normals.x = v3Normals.z = 0.5f;
                    }
                    else if (v3Normals.y < 0) {
                        v3Normals.y = -.49f;
                        v3Normals.x = v3Normals.z = 0.5f;
                    }
                    else if (v3Normals.z > 0) {
                        v3Normals.x = v3Normals.y = 0.5f;
                    }
                    else if (v3Normals.z < 0) {
                        v3Normals.z = -.49f;
                        v3Normals.x = v3Normals.y = 0.5f;
                    }
                    
                    if (_selectedElement.Go != null)
                    {
                        _v3EditPos = _hit.point + v3Normals * shapesFactory.currentSize;//_selectedElement.Size;
                    }
                    /*else
                    {
                        Debug.Log("FUCK!");
                        _v3EditPos = _hit.point + v3Normals * shapesFactory.currentSize;
                    }*/
                }
                else if (_goHit.CompareTag("Grid"))
                {
                    if (v3Normals.y > 0) {
                        v3Normals.x = v3Normals.z = 0.5f;
                    }

                    _v3EditPos = _hit.point + v3Normals * shapesFactory.currentSize;
                }
            }
            else
            {
                return;
            }
            
            _v3EditPos = Snap(_v3EditPos, shapesFactory.currentSize);
            _goEditBlock.transform.position = _v3EditPos;
            textPos.text = _v3EditPos.ToString();
            
            if (_mousewheel != 0.0f && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                if (_timer > _lastMouseWheelUpdate)
                {
                    _lastMouseWheelUpdate = _timer + 0.2f;
                    var dir = _mousewheel > 0 ? 1 : -1;
                    ChangeDropdownValue(dropdownShapes, dir);
                    return;
                }
            }

            if (EventSystem.current.IsPointerOverGameObject()) return;

            // Collision check

            //_v3CollCenter.x = _v3EditPos.x;// - shapesFactory.currentSize * 0.5f;
            //_v3CollCenter.y = _v3EditPos.y;// - shapesFactory.currentSize * 0.5f;
            //_v3CollCenter.z = _v3EditPos.z;// - shapesFactory.currentSize * 0.5f;
            _collBounds.center = _v3EditPos;//_v3CollCenter;
            
            boundsRendererRaycastElement.UpdateBounds(_collBounds);

            //Debug.Log("1:"+Physics.OverlapBox(_collisionBounds.center, _v3CollisionExtents, Quaternion.identity).Length);
            //Debug.Log("2:"+Physics.OverlapBoxNonAlloc(_collisionBounds.center, _v3CollisionExtents, _colliders, Quaternion.identity));
            _collision = Physics.OverlapBoxNonAlloc(_collBounds.center, _v3CollExtents, _colliders, Quaternion.identity) > 0;
            //_collision = _colliders.Length > 0;
                         
            boundsRendererRaycastElement.gameObject.SetActive (_collision);

            if (_editMode == EditMode.Buidl)
            {
                if (_collision) return;
                if (!Input.GetMouseButtonDown(0)) return;

                PlaceBlock();
            }
            else
            {
                if (Input.GetMouseButton(0) && !_collision)
                {
                    CreateDrawBlockElement();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    PlaceDrawBlocks();
                    RemoveDrawBlocks();
                }
            }            
        }
        
        // ----------------------------------------------------------------------------------------
        private static Vector3 Snap(Vector3 vector3, float gridSize = 1.0f)
        {
            var gridHalf = gridSize * .5f;
            return new Vector3(
                Mathf.Round(vector3.x / gridSize) * gridSize - gridHalf,
                Mathf.Round(vector3.y / gridSize) * gridSize - gridHalf,
                Mathf.Round(vector3.z / gridSize) * gridSize - gridHalf);
        }

        // ------------------------------------------------------------------------
        private void UpdateExtendMode()
        {
            if (_selectedElement.Go != null) //_goHit != null && _goHit.CompareTag("Block"))
            {
                boundsRendererRaycastElement.gameObject.SetActive (true);
                boundsRendererRaycastElement.UpdateBounds(_goHit.GetComponent<Collider>().bounds);

                if (!Input.GetMouseButtonDown(0)) return;
                
                if (_curAlphaPressed != -1)
                {
                    if (_curExtrudeStep == 0) {
                        _extrudeFromElement = _selectedElement;
                    }
                        
                    ExtrudeSelectedElement();
                }
            }
            else
            {
                boundsRendererRaycastElement.gameObject.SetActive (false);
            }
        }
        
        // ------------------------------------------------------------------------
        private void UpdateDeleteMode()
        {
            if (_selectedElement.Go != null) //_goHit != null && _goHit.CompareTag("Block"))
            {
                boundsRendererRaycastElement.gameObject.SetActive (true);
                boundsRendererRaycastElement.UpdateBounds(_goHit.GetComponent<Collider>().bounds);

                if (Input.GetMouseButtonDown(0)) {
                    DeleteBlockElement(_goHit.name);
                }
            }
            else
            {
                boundsRendererRaycastElement.gameObject.SetActive (false);
            }
        }

        // ------------------------------------------------------------------------
        private void ExtrudeSelectedElement()
        {
            var pos = _extrudeFromElement.Pos;
            if (_curAlphaPressed == 1) {
                pos.x += _extrudeFromElement.Size * (_curExtrudeStep + 1);
            }
            else if (_curAlphaPressed == 2) {
                pos.z += _extrudeFromElement.Size * (_curExtrudeStep + 1);
            }
            else if (_curAlphaPressed == 3) {
                pos.x -= _extrudeFromElement.Size * (_curExtrudeStep + 1);
            }
            else if (_curAlphaPressed == 4) {
                pos.z -= _extrudeFromElement.Size * (_curExtrudeStep + 1);
            }
            else if (_curAlphaPressed == 5) {
                pos.y += _extrudeFromElement.Size * (_curExtrudeStep + 1);
            }
            else if (_curAlphaPressed == 6) {
                pos.y -= _extrudeFromElement.Size * (_curExtrudeStep + 1);
            }

            _v3CollCenter.x = pos.x + _extrudeFromElement.Size * 0.5f;
            _v3CollCenter.y = pos.y + _extrudeFromElement.Size * 0.5f;
            _v3CollCenter.z = pos.z - _extrudeFromElement.Size * 0.5f;
            
            var extent = 0.5f * (_extrudeFromElement.Size - 0.01f);
            var v3Extents = new Vector3(extent, extent, extent);
            var size = Physics.OverlapBoxNonAlloc(_v3CollCenter, v3Extents, _colliders, Quaternion.identity);
            if (size <= 0)
            {
                var go = shapesFactory.GetBlockGameObject(
                    _extrudeFromElement.IndexSet,
                    _extrudeFromElement.IndexType, 
                    _extrudeFromElement.IndexShape,
                    _extrudeFromElement.IndexSize);
                go.name = "block_"+_blockCounter++;
                go.transform.SetParent(containerBlocks);
                go.transform.position = pos;
                go.GetComponent<Renderer>().sharedMaterial = _materials[_extrudeFromElement.IndexMaterial]; 
                
                _blockElements.Add(go.name, new BlockElement {
                    Id = _extrudeFromElement.Id,
                    IndexSet = _extrudeFromElement.IndexSet,
                    IndexType = _extrudeFromElement.IndexType,
                    IndexShape = _extrudeFromElement.IndexShape,
                    IndexSize = _extrudeFromElement.IndexSize,
                    IndexMaterial = _extrudeFromElement.IndexMaterial,
                    Size = _extrudeFromElement.Size,
                    Go = go,
                    Pos = pos}
                );

                if (_curExtrudeStep == 0) {
                    AddNewUndoStep(_editMode, new List<string>());
                }
                _undoSteps[_undoSteps.Count-1].BlockIds.Add(go.name);
                
                _curExtrudeStep++;
            }
            else
            {
                Debug.Log("collision: "+_colliders[0].gameObject.name);
            }
        }

        // ------------------------------------------------------------------------
        private void UpdateColourMode()
        {
            if (_selectedElement.Go != null) //_goHit != null && _goHit.CompareTag("Block"))
            {
                boundsRendererRaycastElement.gameObject.SetActive (true);
                boundsRendererRaycastElement.UpdateBounds(_goHit.GetComponent<Collider>().bounds);

                if (Input.GetMouseButtonDown(0)) {
                    ColourBlockElement(_goHit.name);
                }
            }
            else
            {
                boundsRendererRaycastElement.gameObject.SetActive (false);
            }
        }
        
        // ------------------------------------------------------------------------
        private void UpdateCopyMode()
        {
            if (_selectedElement.Go != null) //_goHit != null && _goHit.CompareTag("Block"))
            {
                boundsRendererRaycastElement.gameObject.SetActive (true);
                boundsRendererRaycastElement.UpdateBounds(_goHit.GetComponent<Collider>().bounds);

                if (Input.GetMouseButtonDown(0))
                {
                    _updateAfterDropdownValueChange = false;
                    dropdownSets.value      = _selectedElement.IndexSet;
                    dropdownTypes.value     = _selectedElement.IndexType;
                    dropdownShapes.value    = _selectedElement.IndexShape;
                    dropdownSizes.value     = _selectedElement.IndexSize;
                    dropdownMaterials.value = _selectedElement.IndexMaterial;
                    _updateAfterDropdownValueChange = true;
                    
                    SetEditMode(EditMode.Buidl);
                    GetNewEditBlock();
                }
            }
            else
            {
                boundsRendererRaycastElement.gameObject.SetActive (false);
            }
        }
        
        // ------------------------------------------------------------------------
        private void SetEditMode(EditMode mode)
        {
            if (_editMode == mode) return;

            _editMode = mode;

            textPos.text = "";
            
            if (_editMode == EditMode.Delete) {
                SetHelpText("Hover mouse over block to select\nLeft mouse: remove block");
            } else if (_editMode == EditMode.Copy) {
                SetHelpText("Hover mouse over block to select\nLeft mouse: copy selected block"); 
            } else if (_editMode == EditMode.Colour) {
                SetHelpText("Hover mouse over block to select\nLeft mouse: change block material"); 
            } else if (_editMode == EditMode.Extend) {
                SetHelpText("Hover mouse over block to select\nKeys 1-6 + left mouse: Extend selected block");
            } else if (_editMode == EditMode.Draw) {
                SetHelpText("Left mouse down: start drawing\nleft mouse up: end drawing");
            } else if (_editMode == EditMode.Buidl) {
                SetHelpText("Left mouse: place block\nShift+Mousewheel: Toggle shape");
            } else {
                SetHelpText("Mousewheel: zoom in/out\nRight mouse: rotate view\nCenter mouse: move view");
            }
            
            buttonBuild.interactable  = _editMode != EditMode.Buidl;
            buttonDraw.interactable   = _editMode != EditMode.Draw;
            buttonExtend.interactable = _editMode != EditMode.Extend;
            buttonColour.interactable = _editMode != EditMode.Colour;
            buttonCopy.interactable   = _editMode != EditMode.Copy;
            buttonDelete.interactable = _editMode != EditMode.Delete;
            
            dropdownSets.interactable      = _editMode == EditMode.Buidl || _editMode == EditMode.Draw;
            dropdownTypes.interactable     = _editMode == EditMode.Buidl || _editMode == EditMode.Draw;
            dropdownShapes.interactable    = _editMode == EditMode.Buidl || _editMode == EditMode.Draw;
            dropdownSizes.interactable     = _editMode == EditMode.Buidl || _editMode == EditMode.Draw;
            
            dropdownMaterials.interactable = _editMode == EditMode.Buidl || _editMode == EditMode.Draw || _editMode == EditMode.Colour;
            
            _goEditBlock.SetActive(_editMode == EditMode.Buidl || _editMode == EditMode.Draw);
            
            _goEditBlock.GetComponent<Renderer>().sharedMaterial = _editMode == EditMode.Draw ? _drawMaterial : _selectedMaterial;

            raycastLineRenderer.gameObject.SetActive(_editMode != EditMode.Null);
            boundsRendererRaycastElement.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------------
        private void SetGridSize()
        {
            var scale = 36f / shapesFactory.currentSize;
            var matScale = new Vector3(scale, scale, scale);
            gridRenderer.material.mainTextureScale = matScale;   
        }
        
        // ------------------------------------------------------------------------
        private void DoRayCast(Camera raycam)
        {
            _goHit = null;
            _ray = raycam.ScreenPointToRay (Input.mousePosition);

            _selectedElement.Go = null;
            
            raycastLineRenderer.SetPosition(0, fpsController.position);

            var hitSomething = Physics.Raycast(_ray, out _hit, 40);
            if (!hitSomething)
            {
                raycastLineRenderer.SetPosition(1, _ray.GetPoint(40));
                return;// false;
            }
			
            raycastLineRenderer.SetPosition(1, _hit.point);
            _goHit = _hit.collider.gameObject;
			
            if (_blockElements.ContainsKey(_goHit.name)) {
                _selectedElement = _blockElements[_goHit.name];
            }
        }
        
        // ----------------------------------------------------------------------------------------
        private void GetNewEditBlock()
        {
            if (_goEditBlock != null) Destroy(_goEditBlock);
            
            _v3Offset = new Vector3(
                shapesFactory.currentSize * 0.5f,
                shapesFactory.currentSize * 0.5f,
                shapesFactory.currentSize * 0.5f);
                
            _goEditBlock = shapesFactory.GetBlockGameObject(_indexSet, _indexType, _indexShape);
            _goEditBlock.transform.SetParent(containerBlocks);
            _goEditBlock.transform.position = _v3EditPos;

            _goEditBlock.GetComponent<Renderer>().sharedMaterial = _editMode == EditMode.Draw ? _drawMaterial : _selectedMaterial;

            _collBounds.size = Vector3.one * (shapesFactory.currentSize + 0.01f);
            var extent = 0.5f * (shapesFactory.currentSize - 0.01f);
            _v3CollExtents = new Vector3(extent, extent, extent);
            
            _goEditBlock.GetComponent<Collider>().enabled = false;
        }

        // ----------------------------------------------------------------------------------------
        private void PlaceBlock()
        {
            _goEditBlock.name = "block_"+_blockCounter++;
            _goEditBlock.GetComponent<Collider>().enabled = true;
                
            _blockElements.Add(_goEditBlock.name, new BlockElement {
                Id = shapesFactory.currentId,
                IndexSet = _indexSet,
                IndexType = _indexType,
                IndexShape = _indexShape,
                IndexSize = _indexSize,
                IndexMaterial = _indexMaterial,
                Size = shapesFactory.currentSize,
                Go = _goEditBlock,
                Pos = _goEditBlock.transform.position}
            );

            AddNewUndoStep(_editMode, new List<string> {_goEditBlock.name});
            
            _goEditBlock = null;
            GetNewEditBlock();
        }
        
        // ----------------------------------------------------------------------------------------
        private void DeleteBlockElement(string elementName, bool allowUndo = true)
        {
            if (!_blockElements.ContainsKey(elementName)) return;

            _blockElements[elementName].Go.transform.SetParent(containerDeletedBlocks);
            //Destroy(_blockElements[elementName].Go);
            _deletedElements.Add(elementName, _blockElements[elementName]);
            _blockElements.Remove(elementName);

            if (allowUndo) {
                AddNewUndoStep(_editMode, new List<string> {elementName});
            }
        }

        // ----------------------------------------------------------------------------------------
        private void CreateDrawBlockElement()
        {
            var okayToPlace = true;
            foreach (var block in _drawBlockElements)
            {
                if (block.Value.Pos == _v3EditPos) {
                    okayToPlace = false;
                    break;
                }
            }

            if (!okayToPlace) return;
            
            _goEditBlock.name = "drawBlock_"+_timer.ToString(CultureInfo.InvariantCulture);
            _goEditBlock.GetComponent<Collider>().enabled = false;
            _goEditBlock.transform.SetParent(containerDrawBlocks);
            
            _drawBlockElements.Add(_goEditBlock.name, new BlockElement {
                Id = shapesFactory.currentId,
                IndexSet = _indexSet,
                IndexType = _indexType,
                IndexShape = _indexShape,
                IndexSize = _indexSize,
                IndexMaterial = _indexMaterial,
                Size = shapesFactory.currentSize,
                Go = _goEditBlock,
                Pos = _goEditBlock.transform.position}
            );
            
            _goEditBlock = null;
            GetNewEditBlock();
        }
        
        // ----------------------------------------------------------------------------------------
        private void PlaceDrawBlocks()
        {
            var undoBlockIds = new List<string>();

            foreach (var block in _drawBlockElements)
            {
                var go = block.Value.Go;
                
                go.name = "block_"+_blockCounter++;
                go.GetComponent<Collider>().enabled = true;
                go.transform.SetParent(containerBlocks);
                go.GetComponent<Renderer>().material = _selectedMaterial;

                _blockElements.Add(go.name, block.Value);
                
                undoBlockIds.Add(go.name);
            }

            if (undoBlockIds.Count > 0) {
                AddNewUndoStep(_editMode, undoBlockIds);
            }
            
            GetNewEditBlock();
        }
        
        // ----------------------------------------------------------------------------------------
        private void RemoveDrawBlocks()
        {
            _drawBlockElements.Clear();
            
            foreach (Transform child in containerDrawBlocks.transform) {
                Destroy(child.gameObject);
            }
        }
        
        // ----------------------------------------------------------------------------------------
        private void ColourBlockElement(string elementName)
        {
            if (!_blockElements.ContainsKey(elementName)) return;

            var element = _blockElements[elementName];

            AddNewUndoStep(_editMode, new List<string>{elementName}, element.IndexMaterial);
            
            element.IndexMaterial = _indexMaterial;
            element.Go.GetComponent<Renderer>().material = _selectedMaterial;
            _blockElements[elementName] = element;
        }
        
        // ----------------------------------------------------------------------------------------
        private void AddNewUndoStep(EditMode mode, List<string> blockIds, int materialId = -1)
        {
            var step = new UndoStep {Mode = mode, BlockIds = blockIds, MaterialIndex = materialId};
            _undoSteps.Add(step);

            buttonUndo.interactable = true;
            captionUndo.text = "UNDO (" + _undoSteps.Count + ")";
        }

        // ----------------------------------------------------------------------------------------
        private void UndoLastStep()
        {
            var index = _undoSteps.Count - 1;
            if (index < 0) return;
            
            //Debug.Log("UndoLastStep "+index);
            var step  = _undoSteps[index];
            foreach (var blockId in step.BlockIds)
            {
                if (step.Mode == EditMode.Delete)
                {
                    _deletedElements[blockId].Go.transform.SetParent(containerBlocks);
                    _blockElements.Add(blockId, _deletedElements[blockId]);
                    _deletedElements.Remove(blockId);
                }
                else if (step.Mode == EditMode.Colour)
                {
                    _blockElements[blockId].Go.GetComponent<Renderer>().material = _materials[step.MaterialIndex];
                }
                else
                {
                    DeleteBlockElement(blockId, false);
                }
            }
            _undoSteps.RemoveAt(index);
            
            buttonUndo.interactable = _undoSteps.Count > 0;
            captionUndo.text = _undoSteps.Count > 0 ? "UNDO (" + _undoSteps.Count + ")" : "UNDO";
        }
        
        // ----------------------------------------------------------------------------------------
        private static void ChangeDropdownValue(Dropdown dd, int dir)
        {
            var newValue = dd.value + dir;
            if (newValue < 0) newValue = dd.options.Count - 1;
            else if (newValue >= dd.options.Count) newValue = 0;

            dd.value = newValue;
        }

        // ----------------------------------------------------------------------------------------
        private void LoadMaterials()
        {
            dropdownMaterials.ClearOptions();
            
            _materials = new List<Material>();
            string[] resources = {"Grass", "Dirt", "Rock", "Stone", "Carpet"};
            foreach (var r in resources)
            {
                var mat = Resources.Load<Material>("Materials/" + r);
                _materials.Add(mat);

                dropdownMaterials.options.Add(new Dropdown.OptionData {text = r});
            }

            dropdownMaterials.captionText.text = resources[_indexMaterial];
            dropdownMaterials.value = _indexMaterial;

            _selectedMaterial = _materials[_indexMaterial];
            
            _drawMaterial = Resources.Load<Material>("Materials/DrawMaterial");
        }

        // ----------------------------------------------------------------------------------------
        private void SetTransformGizmo(bool active)
        {
            extendGizmo.gameObject.SetActive(active);
        }
        
        // ----------------------------------------------------------------------------------------
        private void SetHelpText(string s)
        {
            textHelp.text = s;
            textHelp.gameObject.SetActive(!string.IsNullOrEmpty(s));
        }
        
        // ----------------------------------------------------------------------------------------
        // UI Event Handlers
        // ----------------------------------------------------------------------------------------
        public void OnBuidlClick()
        {
            SetEditMode(EditMode.Buidl);
        }
        
        // ----------------------------------------------------------------------------------------
        public void OnDrawClick()
        {
            SetEditMode(EditMode.Draw);
        }
        
        // ----------------------------------------------------------------------------------------
        public void OnExtendClick()
        {
            SetEditMode(EditMode.Extend);
        }
        
        // ----------------------------------------------------------------------------------------
        public void OnColourClick()
        {
            SetEditMode(EditMode.Colour);
        }

        // ----------------------------------------------------------------------------------------
        public void OnCopyClick()
        {
            SetEditMode(EditMode.Copy);
        }
        
        // ----------------------------------------------------------------------------------------
        public void OnDeleteClick()
        {
            SetEditMode(EditMode.Delete);
        }
        
        // ----------------------------------------------------------------------------------------
        public void OnUndoClick()
        {
            UndoLastStep();
        }
        
        // ----------------------------------------------------------------------------------------
        //
        // ----------------------------------------------------------------------------------------
        public void OnSetValueChange(int value)
        {
            _indexSet = value;
            shapesFactory.PopulateTypesDropdown(dropdownTypes, _indexSet);
        }

        // ----------------------------------------------------------------------------------------
        public void OnTypeValueChange(int value)
        {
            _indexType = value;
            shapesFactory.PopulateShapesDropdown(dropdownShapes, _indexSet, _indexType);
            
            if (!_updateAfterDropdownValueChange) return;
            
            GetNewEditBlock();
        }

        // ----------------------------------------------------------------------------------------
        public void OnShapeValueChange(int value)
        {
            _indexShape = value;
            
            if (!_updateAfterDropdownValueChange) return;
            
            GetNewEditBlock();
        }        
        
        // ----------------------------------------------------------------------------------------
        public void OnSizeValueChange(int value)
        {
            _indexSize = value;
            shapesFactory.SetSize(value);
            
            if (!_updateAfterDropdownValueChange) return;
            
            GetNewEditBlock();
            SetGridSize();
        }

        // ----------------------------------------------------------------------------------------
        public void OnMaterialValueChange(int value)
        {
            _indexMaterial = value;
            _selectedMaterial = _materials[_indexMaterial];

            if (!_updateAfterDropdownValueChange) return;
            
            if (_goEditBlock != null) {
                _goEditBlock.GetComponent<Renderer>().sharedMaterial = _editMode == EditMode.Draw ? _drawMaterial : _selectedMaterial;
            }
        }
        
        // ------------------------------------------------------------------------
        /*private void OnDrawGizmos() {
            var size = Vector3.one * (ShapesFactory.currentSize + 0.01f);
            if (_collision) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(_collisionBounds.center, size);
            }
            else {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_collisionBounds.center, size);
            }
        }*/
    }
}