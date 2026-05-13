using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMove : MonoBehaviour
{
    public void GoToNextScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
