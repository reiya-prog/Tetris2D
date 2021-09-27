using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Tetris
{
    namespace Mino
    {
        public class MinoGenerater : MonoBehaviour
        {
            // ミノの座標周りの定数
            // ミノの生成位置。中心を中央から1マス左の位置になるように生成する。
            private readonly Vector3 kStartingMinoPosition = new Vector3(-0.5f, 8.5f, 0f);
            // ネクストミノの座標。この値にOffsetY * 順番を引いた値が最終的な座標になる。
            private readonly Vector3 kNextMinoPosition = new Vector3(7.5f, 7.5f, 0f);
            // ネクストミノのy座標オフセット。
            private const float kNextMinoOffsetY = 1.5f;


            // ミノのスケール。生成時は全て1、ネクストミノは全て0.5にする。
            private readonly Vector3 kStartingMinoScale = new Vector3(1.0f, 1.0f, 1.0f);
            private readonly Vector3 kNextMinoScale = new Vector3(0.5f, 0.5f, 0.5f);

            // ミノオブジェクト周りの変数
            // ネクストミノの個数
            private const int kMaxNextMinoSize = 6;
            // ネクストミノオブジェクトを保持するキュー。常に6種類のミノを保持する。
            private Queue<GameObject> _nextMinos;
            // ネクストミノタイプを保持するキュー。ネクストミノは7種類のミノをランダムに並べたセットを繰り返す。
            private Queue<Tetromino> _nextMinosType;
            // テトリミノの種類一覧
            private Tetromino[] _tetrominos = new Tetromino[] { Tetromino.IMino, Tetromino.JMino, Tetromino.LMino, Tetromino.OMino, Tetromino.SMino, Tetromino.TMino, Tetromino.ZMino };
            // Resourcesフォルダにあるミノプレハブを所持するための配列
            private GameObject[] _minoObjects;

            private void Awake()
            {
                _nextMinos = new Queue<GameObject>();
                _nextMinosType = new Queue<Tetromino>();
            }

            private void Start()
            {
                _minoObjects = Resources.LoadAll<GameObject>("Tetromino");

                SetNextMinosType();
                for (int i = 0; i < kMaxNextMinoSize; ++i)
                {
                    GenerateNextMino();
                }
                this.gameObject.GetComponent<MinoController>().enabled = true;
            }

            // 7種類のミノをシャッフルしてネクストミノの順番として保存。
            private void SetNextMinosType()
            {
                Tetromino[] shuffledTetrominos = _tetrominos.OrderBy(i => System.Guid.NewGuid()).ToArray();
                foreach (Tetromino item in shuffledTetrominos)
                {
                    _nextMinosType.Enqueue(item);
                }
            }

            // _nextMinosTypeをもとにネクストミノオブジェクトを生成。
            private void GenerateNextMino()
            {
                Tetromino nextMinoType = _nextMinosType.Dequeue();
                GameObject generatedMino = Instantiate(_minoObjects[(int)nextMinoType]);
                _nextMinos.Enqueue(generatedMino);
                SetNextMinosPosition();
            }

            // ネクストミノの座標を調整
            private void SetNextMinosPosition()
            {
                foreach (var (item, index) in _nextMinos.Select((item, index) => (item, index)))
                {
                    item.transform.position = kNextMinoPosition - new Vector3(0f, kNextMinoOffsetY * index, 0f);
                    item.transform.localScale = kNextMinoScale;
                }
            }

            // ネクストミノの先頭を取得し、次の操作ミノにする。
            public GameObject GetNextMino()
            {
                // ネクストミノが6つ(次に呼び出すミノ+ネクストミノに残る5つ)まで減っていた場合は後ろに7つ追加する
                if (_nextMinosType.Count <= kMaxNextMinoSize)
                {
                    SetNextMinosType();
                }
                // ネクストミノの先頭を取得し、末尾に一つ追加する。
                GameObject nextMino = _nextMinos.Dequeue();
                GenerateNextMino();

                // 取得したミノを操作ミノの位置にセット
                nextMino.transform.position = kStartingMinoPosition;
                nextMino.transform.localScale = kStartingMinoScale;
                return nextMino;
            }
        }
    }
}