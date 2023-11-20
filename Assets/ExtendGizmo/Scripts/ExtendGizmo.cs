//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using UnityEngine;

namespace ExtendGizmo.Scripts
{
    public class ExtendGizmo : MonoBehaviour
    {
        public Transform axisX;
        public Transform axisY;
        public Transform axisZ;

        public void SetAxis(int alphaPressed)
        {
            axisX.gameObject.SetActive(alphaPressed == 1 || alphaPressed == 3);
            axisY.gameObject.SetActive(alphaPressed == 5 || alphaPressed == 6);
            axisZ.gameObject.SetActive(alphaPressed == 2 || alphaPressed == 4);

            if (alphaPressed == 3) {
                axisX.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            }
            else if (alphaPressed == 6) {
                axisY.transform.rotation = Quaternion.Euler(new Vector3(180, 0, 0));
            }
            else if (alphaPressed == 4) {
                axisZ.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            }
            else {
                axisX.transform.rotation = Quaternion.Euler(new Vector3(180, 90, 0));
                axisY.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                axisZ.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            }

            /*transform.rotation = Quaternion.Euler(new Vector3(
                alphaPressed == 1 ? 0 : alphaPressed == 3 ? 180 : 0,
                alphaPressed == 5 ? 0 : alphaPressed == 6 ? 180 : 0,
                alphaPressed == 2 ? 0 : alphaPressed == 4 ? 180 : 0
                ));*/
        }
    }
}