//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using UnityEngine;

namespace Dragginz.Scripts.Cams
{
	public class EditorCam : MonoBehaviour
	{
		private const float MovementSpeed = 0.15f;
		private const float MouseWheelSpeed = 5.0f;

		private Vector3 _playerEuler;
		private Vector3 _camOffset;
        
        private bool _mouseRightIsDown;
        private bool _mouseWheelIsDown;

        #region Getters

        private Transform _player;

		#endregion

		private void Awake()
		{
            _player = transform;

			_playerEuler = _player.eulerAngles;

            _mouseRightIsDown = false;
            _mouseWheelIsDown = false;
        }

		//
		private void Update ()
		{
            if (!_mouseRightIsDown) {
				if (Input.GetMouseButtonDown(1)) {
					_mouseRightIsDown = true;
				}
			}
			else {
				if (Input.GetMouseButtonUp(1)) {
					_mouseRightIsDown = false;
				}
			}

            if (!_mouseWheelIsDown) {
	            if (Input.GetMouseButtonDown(2)) {
                    _mouseWheelIsDown = true;
                }
            }
            else {
	            if (Input.GetMouseButtonUp(2)) {
                    _mouseWheelIsDown = false;
                }
            }

            var h = Input.GetAxis ("Horizontal");
            var v = Input.GetAxis ("Vertical");
            var d = Input.GetAxis ("Depth");
            //Debug.Log(h+", "+v+", "+d);
            
            var mX = Input.GetAxis ("Mouse X");
            var mY = Input.GetAxis ("Mouse Y");
            
            // Looking around with the mouse
            if (_mouseRightIsDown) {
				_player.Rotate(-2f * mY, 2f * mX, 0);
				_playerEuler = _player.eulerAngles;
				_playerEuler.z = 0;
				_player.eulerAngles = _playerEuler;
			}
            else if (_mouseWheelIsDown) {
                _player.Translate (-0.2f * mX, -0.2f * mY, 0);
            }

			_player.position += (transform.right * h + transform.forward * v + transform.up * d) * MovementSpeed;
		}

		//
        public void MouseWheelMove(float value)
        {
            _player.position += transform.forward * value * MouseWheelSpeed;
        }
	}
}