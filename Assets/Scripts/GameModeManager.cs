using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class GameModeManager : MonoBehaviour
{
    [Header("Player Rigs")]
    public GameObject vrPlayerRig;
    public GameObject desktopPlayerRig;

    private void Start()
    {
        if (MainMenuManager.launchInVR)
        {
            Debug.Log("VR Mode Selected");
            vrPlayerRig.SetActive(true);
            desktopPlayerRig.SetActive(false);
            
            StartCoroutine(StartXR());
        }
        else
        {
            Debug.Log("Desktop Mode Selected");
            desktopPlayerRig.SetActive(true);
            vrPlayerRig.SetActive(false);
        }
    }

    private IEnumerator StartXR()
    {
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            yield break;
        }

        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed");
            yield break;
        }

        XRGeneralSettings.Instance.Manager.StartSubsystems();
    }

    private void OnDestroy()
    {
        if (MainMenuManager.launchInVR && XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}