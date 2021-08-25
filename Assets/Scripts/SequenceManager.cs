using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SequenceManager : MonoBehaviour
{
    [SerializeField]
    private string _initialSceneName = "Title";

    private GameObject _pauseUI;
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        SceneManager.LoadScene(_initialSceneName);
        _pauseUI = GameObject.FindWithTag("PauseUI");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GamePause();
        }
    }

    public void GameClear() { }
    public void GameOver() { }
    public void GamePause()
    {
        _pauseUI.SetActive(!_pauseUI.activeSelf);
        if (_pauseUI.activeSelf)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}