using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SequenceManager : MonoBehaviour
{
    [SerializeField]
    private string _initialSceneName = "Title";
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        SceneManager.LoadScene(_initialSceneName);
    }

    void Update()
    {
    }

    public void GameClear() { }
    public void GameOver() { }
    public void GamePause() { }

}
