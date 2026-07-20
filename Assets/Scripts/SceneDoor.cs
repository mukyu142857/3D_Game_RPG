using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneDoor : MonoBehaviour
{

    public string targetScene;


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(targetScene);
        }

    }

}