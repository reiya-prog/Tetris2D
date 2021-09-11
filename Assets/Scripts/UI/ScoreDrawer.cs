using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Score, HiScoreの描画を担当
namespace Tetris
{
    namespace UI
    {
        public class ScoreDrawer : MonoBehaviour
        {
            // 編集するTextMeshProオブジェクト
            // Scoreを表示するテキスト
            [SerializeField]
            private TextMeshProUGUI _scoreText;
            // HiScoreを表示するテキスト
            [SerializeField]
            private TextMeshProUGUI _hiScoreText;

            // スコアのフォーマット指定。9桁で左端は0埋
            private const string kScoreFormat = "{0:D9}";

            public void ScoreDraw(int score)
            {
                _scoreText.text = System.String.Format(kScoreFormat, score);
            }

            public void HiScoreDraw(int hiScore)
            {
                _hiScoreText.text = System.String.Format(kScoreFormat, hiScore);
            }
        }
    }
}