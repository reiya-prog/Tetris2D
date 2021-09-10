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
        public enum Tetromino
        {
            IMino,
            JMino,
            LMino,
            OMino,
            SMino,
            TMino,
            ZMino
        }

        public class MinoController : MonoBehaviour
        {
            // ミノの座標周りの定数
            // ミノの生成位置。中心を中央から1マス左の位置になるように生成する。
            private readonly Vector3 kStartingMinoPosition = new Vector3(-0.5f, 8.5f, 0f);
            // ホールドミノの座標。
            private readonly Vector3 kHoldMinoPosition = new Vector3(-7.5f, 7.0f, 0.0f);
            // ネクストミノの座標。この値にOffsetY * 順番を引いた値が最終的な座標になる。
            private readonly Vector3 kNextMinoPosition = new Vector3(7.5f, 7.5f, 0f);
            // ネクストミノのy座標オフセット。
            private const float kNextMinoOffsetY = 1.5f;

            // ミノの回転。ホールドに入れる際に使用
            private readonly Vector3 kHoldMinoRotation = new Vector3(0.0f, 0.0f, 0.0f);

            // ミノのスケール。生成時は全て1、ホールド時は全て0.8、ネクストミノは全て0.5にする。
            private readonly Vector3 kStartingMinoScale = new Vector3(1.0f, 1.0f, 1.0f);
            private readonly Vector3 kHoldMinoScale = new Vector3(0.8f, 0.8f, 0.8f);
            private readonly Vector3 kNextMinoScale = new Vector3(0.5f, 0.5f, 0.5f);

            // ミノオブジェクト周りの変数
            // ネクストミノの個数
            private const int kMaxNextMinoSize = 6;
            // 現在フォーカスされているミノ
            private GameObject _currentMino;
            // ホールドされているミノ。最初にホールドされるまではnullオブジェクト。
            private GameObject _holdMino = null;
            // ネクストミノオブジェクトを保持するキュー。常に6種類のミノを保持する。
            private Queue<GameObject> _nextMinos;
            // ネクストミノタイプを保持するキュー。ネクストミノは7種類のミノをランダムに並べたセットを繰り返す。
            private Queue<Tetromino> _nextMinosType;
            // テトリミノの種類一覧
            private Tetromino[] _tetrominos = new Tetromino[] { Tetromino.IMino, Tetromino.JMino, Tetromino.LMino, Tetromino.OMino, Tetromino.SMino, Tetromino.TMino, Tetromino.ZMino };
            // Resourcesフォルダにあるミノプレハブを所持するための配列
            private GameObject[] _minoObjects;

            // ホールドをしていいかどうか
            private bool _canHold = true;

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

                // テトリミノを取得してゲーム開始
                _currentMino = GetNextMino();
                _currentMino.GetComponent<MinoBehavior>().enabled = true;
                _currentMino.GetComponent<MinoBehavior>().SetMinoManager(this.gameObject);
                _currentMino.GetComponent<MinoBehavior>().StartMoveMino();
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
            private GameObject GetNextMino()
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

            // 現在のミノが下まで到着した際に呼ばれる。
            // 次のミノを取得してゲームを続ける。
            public void StartNextMino()
            {
                // 到着したミノのMinoBehaviorを停止する。
                _currentMino.GetComponent<MinoBehavior>().enabled = false;

                // ホールドが可能になる
                _canHold = true;

                // テトリミノを取得して続ける。
                _currentMino = GetNextMino();
                _currentMino.GetComponent<MinoBehavior>().enabled = true;
                _currentMino.GetComponent<MinoBehavior>().SetMinoManager(this.gameObject);
                _currentMino.GetComponent<MinoBehavior>().StartMoveMino();
            }

            // ホールドが可能かどうか
            // 一度ホールドした場合、新しいミノを設置するまではホールド禁止。
            public bool canHoldMino()
            {
                return _canHold || true;
            }

            public void HoldMino()
            {
                _canHold = false;
                // 現在のミノはホールドされるのでMinoBehaviorを停止する。
                _currentMino.GetComponent<MinoBehavior>().enabled = false;
                // まだホールドしていない場合
                if (_holdMino == null)
                {
                    _holdMino = _currentMino;

                    // ホールドの座標・回転・スケールをセット
                    _holdMino.transform.position = kHoldMinoPosition;
                    _holdMino.transform.GetChild(0).transform.rotation = Quaternion.Euler(kHoldMinoRotation);
                    _holdMino.transform.localScale = kHoldMinoScale;

                    // 次のミノを取得
                    _currentMino = GetNextMino();
                }
                else
                {
                    GameObject tmpMino = _holdMino;
                    _holdMino = _currentMino;

                    // ホールドの座標・回転・スケールをセット
                    _holdMino.transform.position = kHoldMinoPosition;
                    _holdMino.transform.GetChild(0).transform.rotation = Quaternion.Euler(kHoldMinoRotation);
                    _holdMino.transform.localScale = kHoldMinoScale;
                    _currentMino = tmpMino;
                }
                Debug.Log("hold : " + _holdMino + " current : " + _currentMino);
                // 次のミノのMinoBehaviorを有効にして開始。
                _currentMino.GetComponent<MinoBehavior>().enabled = true;
                _currentMino.GetComponent<MinoBehavior>().StartMoveMino();
                _holdMino.GetComponent<MinoBehavior>().HoldMino();
            }
        }
    }
}