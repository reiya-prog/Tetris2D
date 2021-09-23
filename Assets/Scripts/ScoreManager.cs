using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tetris
{
    public class ScoreManager : MonoBehaviour
    {
        // UIManagerオブジェクト
        [SerializeField]
        private GameObject _UIManagerObject;
        // 現在のスコア
        private int _currentScore = 0;
        // ハイスコア
        private int _highScore;
        // ハイスコアを保存するためのキー
        private const string kHighScoreKey = "HIGH SCORE";

        // スコア周りの定数
        // 列を削除した時の加算されるスコア=1000点
        private const int kLineDeleteScore = 1000;
        // ミノを↓キーで落とした時に加算されるスコア=10点
        private const int kMinoDownScore = 10;

        private void Awake()
        {
            _highScore = PlayerPrefs.GetInt(kHighScoreKey, 0);
        }

        private void Start()
        {
            _UIManagerObject.GetComponent<UI.ScoreDrawer>().ScoreDraw(_currentScore);
            _UIManagerObject.GetComponent<UI.ScoreDrawer>().HiScoreDraw(_highScore);
        }

        // 実際にスコアを変更する関数
        private void AddScore(int adderScore)
        {
            _currentScore += adderScore;
            _UIManagerObject.GetComponent<UI.ScoreDrawer>().ScoreDraw(_currentScore);
            if (_currentScore > _highScore)
            {
                _highScore = _currentScore;
                PlayerPrefs.SetInt(kHighScoreKey, _highScore);
                _UIManagerObject.GetComponent<UI.ScoreDrawer>().HiScoreDraw(_highScore);
            }
        }

        // ライン削除時のスコア加算用
        public void AddLineDeleteScore()
        {
            AddScore(kLineDeleteScore);
        }

        // ミノを1マス落とした時のスコア加算用
        public void AddMinoDownScore()
        {
            AddScore(kMinoDownScore);
        }
    }
}