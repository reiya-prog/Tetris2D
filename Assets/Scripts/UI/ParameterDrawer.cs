using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Time, Level, Linesの描画を担当
namespace Tetris
{
    namespace UI
    {
        public class ParameterDrawer : MonoBehaviour
        {
            // 編集するTextMeshProオブジェクト
            // Timeを表示するテキスト
            [SerializeField]
            private TextMeshProUGUI _timeText;
            // Levelを表示するテキスト
            [SerializeField]
            private TextMeshProUGUI _levelText;
            // Linesを表示するテキスト
            [SerializeField]
            private TextMeshProUGUI _linesText;

            // 出力するフォーマット
            // timeのフォーマット。時:分:秒:ミリ秒で表示
            private const string kTimeFormat = "{0:D2}:{1:D2}:{2:D2}:{3:D2}";
            // levelのフォーマット。2桁0埋
            private const string kLevelFormat = "{0:D2}";
            // linesのフォーマット。4桁0埋
            private const string kLinesFormat = "{0:D4}";

            public void timeDraw(float deltaTimeFromStart)
            {
                _timeText.text = System.String.Format(kTimeFormat, deltaTimeFromStart);
            }

            public void levelDraw(int level)
            {
                _levelText.text = System.String.Format(kLevelFormat, level);
            }

            public void linesDraw(int lines)
            {
                _linesText.text = System.String.Format(kLinesFormat, lines);
            }
        }
    }
}