using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tetris
{
    namespace UI
    {
        public class TechnicalDrawer
        {
            // 編集するTextMeshProオブジェクト
            // LENを表示するテキスト
            [SerializeField]
            private TextMeshProUGUI _lenText;
            // TetrisとT-Spin等を表示するテキスト
            [SerializeField]
            private TextMeshProUGUI _tetrisAndSpinText;
            // Back-to-Backを表示するテキスト
            [SerializeField]
            private TextMeshProUGUI _backToBackText;

            // 出力するフォーマットとテキスト
            // Lenのフォーマット。Len数+\n+LEN
            private const string kLenFormat = "{0}\nLEN";
            // Tetrisのテキスト。Tetris!と表示する。
            private const string kTetrisText = "Tetris!";
            // Tspinのテキスト。T SPIN!の表示に続けてSpinの詳細を表示する。
            private const string kTspinText = "T SPIN!";
            private const string kTspinMiniText = "Mini"; // Tspin Mini
            private const string kTspinSingleText = "Single"; // Tspin Single
            private const string kTspinDoubleText = "Double"; // Tspin Double
            private const string kTspinTripleText = "Triple"; // Tspin Triple
            // Back-to-Backのテキスト。Back-to-Backと表示する。
            private const string kBackToBackText = "Back-to-Back";
            public void LenDraw(int lenNum)
            {
                _lenText.text = System.String.Format(kLenFormat, lenNum);
            }

            public void TetrisDraw()
            {
                _tetrisAndSpinText.text = kTetrisText;
            }

            public void SpinDraw(int spinType)
            {
                _tetrisAndSpinText.text = kTspinText;
                switch (spinType)
                {
                    case 0: // Tspin Mini
                        _tetrisAndSpinText.text += kTspinMiniText;
                        break;
                    case 1: // Tspin Single
                        _tetrisAndSpinText.text += kTspinSingleText;
                        break;
                    case 2: // Tspin Double
                        _tetrisAndSpinText.text += kTspinDoubleText;
                        break;
                    case 3: // Tspin Triple
                        _tetrisAndSpinText.text += kTspinTripleText;
                        break;
                    default:
                        break;
                }
            }

            public void BackToBackDraw()
            {
                _backToBackText.text = kBackToBackText;
            }
        }
    }
}