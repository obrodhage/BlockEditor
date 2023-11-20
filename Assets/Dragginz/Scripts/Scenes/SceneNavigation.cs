//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dragginz.Scripts.Scenes
{
    public class SceneNavigation : MonoBehaviour
    {
        public int sceneId;

        public void OnClickNext()
        {
            Debug.Log("next - id "+sceneId);
            if (sceneId == 0) {
                SceneManager.LoadScene(1);
            }
            else if (sceneId == 1) {
                SceneManager.LoadScene(2);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        public void OnClickPrevious()
        {
            Debug.Log("previous - id "+sceneId);
            if (sceneId == 2) {
                SceneManager.LoadScene(1);
            }
            else if (sceneId == 1) {
                SceneManager.LoadScene(0);
            }
            else
            {
                SceneManager.LoadScene(2);
            }
        }
    }
}