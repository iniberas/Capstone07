using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static bool launchInVR = false;
    public void PlayVR()
    {   
        launchInVR = true;
        SceneManager.LoadScene("SampleScene");
    }
    public void PlayDesktop()
    {
        launchInVR = false;
        SceneManager.LoadScene("SampleScene");
    }
}