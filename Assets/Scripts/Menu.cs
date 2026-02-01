using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Menu : MonoBehaviour
{
    public void OnButtonClick()
    {
        Debug.Log("Load new scene");
        SceneManager.LoadScene("SampleScene2");
    }
}

