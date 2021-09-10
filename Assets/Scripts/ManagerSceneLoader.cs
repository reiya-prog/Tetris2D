using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerSceneLoader : MonoBehaviour
{
    private const string kManagerSceneName = "Managers";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadManagerScene()
    {
        if (!SceneManager.GetSceneByName(kManagerSceneName).IsValid())
        {
            SceneManager.LoadScene(kManagerSceneName, LoadSceneMode.Additive);
        }
    }
}