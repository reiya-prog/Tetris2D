using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Tetris.Mino;

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
            // ミノの配置状況を保存する配列
            // [縦][横]でアクセスする
            // 壁を初期値で設定する
            // 壁はこの形→|    |
            // 　　　　　　|    |
            // 　　　　　　|____|
            private bool[,] _minoPlacementArray = new bool[22, 12];
            // 左下を(1,1)としたときの設置可能な位置のxとyの最大値
            private int kMaxX = 10;
            private int kMaxY = 21;
            // このスクリプトがアタッチされているゲームオブジェクト。MinoGeneraterもアタッチされている。
            private GameObject _minoManagerObject;
            // ScoreManagerオブジェクト
            [SerializeField]
            private GameObject ScoreManagerObject;

            // ミノの座標周りの定数
            // ホールドミノの座標。
            private readonly Vector3 kHoldMinoPosition = new Vector3(-7.5f, 7.0f, 0.0f);
            private readonly Vector3 kHoldIOMinoPosition = new Vector3(-8.0f, 7.0f, 0.0f);
            // ミノの回転。ホールドに入れる際に使用
            private readonly Vector3 kHoldMinoRotation = new Vector3(0.0f, 0.0f, 0.0f);
            // ホールド状態のミノのスケール。全て0.8。
            private readonly Vector3 kHoldMinoScale = new Vector3(0.8f, 0.8f, 0.8f);

            // ミノオブジェクト周りの変数
            // 現在フォーカスされているミノ
            private GameObject _currentMinoObject;
            // ホールドされているミノ。最初にホールドされるまではnullオブジェクト。
            private GameObject _holdMinoObject = null;
            // 設置したミノの一覧
            private List<GameObject> _fixedMinoObjects;

            // ホールドをしていいかどうか
            private bool _canHold = true;

            private void Awake()
            {
                // 変数の初期化
                _fixedMinoObjects = new List<GameObject>();

                // 壁の設定
                int leftX = 0;
                int rightX = _minoPlacementArray.GetLength(1) - 1;
                for (int xi = 0; xi < _minoPlacementArray.GetLength(1); ++xi)
                {
                    _minoPlacementArray[0, xi] = true;
                }
                for (int yi = 1; yi < _minoPlacementArray.GetLength(0); ++yi)
                {
                    _minoPlacementArray[yi, leftX] = true;
                    _minoPlacementArray[yi, rightX] = true;
                }

                // MinoManagerオブジェクトの取得
                _minoManagerObject = this.gameObject;
                this.enabled = false;
            }

            private void Start()
            {
                // テトリミノを取得してゲーム開始
                _currentMinoObject = _minoManagerObject.GetComponent<MinoGenerater>().GetNextMino();
                _currentMinoObject.GetComponent<MinoBehavior>().enabled = true;
                _currentMinoObject.GetComponent<MinoBehavior>().SetMinoManager(this.gameObject);
                _currentMinoObject.GetComponent<MinoBehavior>().StartMoveMino();
            }

            private void PlaceLog(){
                string log = "";
                for(int i=kMaxY;i>=1;--i){
                    for(int j=1;j<=kMaxX;++j){
                        log += _minoPlacementArray[i, j] ? "1":"0";
                    }
                    log += "\n";
                }
                Debug.Log(log);
            }

            // 現在のミノが下まで到着した際に呼ばれる。
            // 次のミノを取得してゲームを続ける。
            public void StartNextMino()
            {
                PlaceLog();
                // 到着したミノのMinoBehaviorを停止する。
                _currentMinoObject.GetComponent<MinoBehavior>().enabled = false;

                // ホールドが可能になる
                _canHold = true;

                // テトリミノを取得して続ける。
                _currentMinoObject = _minoManagerObject.GetComponent<MinoGenerater>().GetNextMino();
                _currentMinoObject.GetComponent<MinoBehavior>().enabled = true;
                _currentMinoObject.GetComponent<MinoBehavior>().SetMinoManager(this.gameObject);
                _currentMinoObject.GetComponent<MinoBehavior>().StartMoveMino();
            }

            // ホールドが可能かどうか
            // 一度ホールドした場合、新しいミノを設置するまではホールド禁止。
            public bool canHoldMino()
            {
                return _canHold;
            }

            public void HoldMino()
            {
                _canHold = false;
                // 現在のミノはホールドされるのでMinoBehaviorを停止する。
                _currentMinoObject.GetComponent<MinoBehavior>().enabled = false;
                // まだホールドしていない場合
                if (_holdMinoObject == null)
                {
                    _holdMinoObject = _currentMinoObject;

                    // 次のミノを取得
                    _currentMinoObject = _minoManagerObject.GetComponent<MinoGenerater>().GetNextMino();
                    _currentMinoObject.GetComponent<MinoBehavior>().SetMinoManager(this.gameObject);
                }
                else
                {
                    // 現在のミノをホールドミノと交換する
                    GameObject tmpMino = _holdMinoObject;
                    _holdMinoObject = _currentMinoObject;
                    _currentMinoObject = tmpMino;
                }
                // IミノもしくはOミノの場合ホールドする座標が違う
                if (_holdMinoObject.GetComponent<MinoBehavior>().minoType == Tetromino.IMino ||
                    _holdMinoObject.GetComponent<MinoBehavior>().minoType == Tetromino.OMino)
                {
                    _holdMinoObject.transform.position = kHoldIOMinoPosition;
                }
                else
                {
                    _holdMinoObject.transform.position = kHoldMinoPosition;
                }
                _holdMinoObject.transform.GetChild(0).transform.rotation = Quaternion.Euler(kHoldMinoRotation);
                _holdMinoObject.transform.localScale = kHoldMinoScale;

                // 次のミノのMinoBehaviorを有効にして開始。
                _currentMinoObject.GetComponent<MinoBehavior>().enabled = true;
                _currentMinoObject.GetComponent<MinoBehavior>().StartMoveMino();
                _holdMinoObject.GetComponent<MinoBehavior>().HoldMino();
            }

            // ミノが設置したときに呼ばれる関数
            // ラインの判定などを行った後新しいミノを呼び出す
            public void FixMino(float lowerYPosition, float upperYPosition)
            {
                _fixedMinoObjects.Add(_currentMinoObject);
                CheckLine(lowerYPosition, upperYPosition);
                StartNextMino();
            }

            // 設置したミノのフラグを建てる
            public void SetMinoPlacement(Vector3 fixedPosition)
            {
                // 座標をインデクスに変換する。x座標y座標の順で返ってくる
                System.Tuple<int, int> index = ConvertPosition2Index(fixedPosition.x, fixedPosition.y);
                _minoPlacementArray[index.Item2, index.Item1] = true;
            }

            // ミノのフラグが建っているかのチェック
            public bool CheckMinoPlacement(Vector3 checkPosition)
            {
                // 座標をインデクスに変換する。x座標y座標の順で返ってくる
                System.Tuple<int, int> index = ConvertPosition2Index(checkPosition.x, checkPosition.y);
                return _minoPlacementArray[index.Item2, index.Item1];
            }

            // 設置したミノの列が消せるかどうかのチェック
            // 引数は設置したミノの下端と上端(その範囲のみチェックすれば十分)
            public void CheckLine(float lowerYPosition, float upperYPosition)
            {
                System.Tuple<int, int> index = ConvertY2Index(lowerYPosition, upperYPosition);
                for (int yi = index.Item1; yi <= index.Item2; ++yi)
                {
                    Debug.Log(yi);
                    bool isDelete = true;
                    for (int xi = 1; xi <= kMaxX; ++xi)
                    {
                        // もし設置フラグが一つでもfalseの場合isDeleteがfalseになる
                        isDelete &= _minoPlacementArray[yi, xi];
                    }
                    // 消すことができる
                    if (isDelete)
                    {
                        DeleteLine(yi);
                    }
                }
            }

            // 引数の列のミノを削除する
            private void DeleteLine(int y)
            {
                for (int i = 0; i < _fixedMinoObjects.Count; ++i)
                {
                    GameObject gameObject = _fixedMinoObjects[i];
                    // 該当の列にオブジェクトがあった場合Destroyする
                    // 引数は列番号を座標に変換して渡す
                    // 戻り値はそのミノ全てをDestroyするかどうか
                    bool isEmpty = gameObject.GetComponent<MinoBehavior>().DestroyMino((float)y - kConvertOffsetY);
                    if (isEmpty)
                    {
                        Destroy(gameObject);
                        _fixedMinoObjects.RemoveAt(i);
                    }
                }
                ResetMinoPlacement(y);
                ScoreManagerObject.GetComponent<ScoreManager>().AddLineDeleteScore();
            }

            // 消したラインより上のミノが下に落ちるのでminoPlacementをずらす
            private void ResetMinoPlacement(int y)
            {
                for (int yi = y; yi < kMaxY; ++yi)
                {
                    for (int xi = 1; xi <= kMaxX; ++xi)
                    {
                        _minoPlacementArray[yi, xi] = _minoPlacementArray[yi + 1, xi];
                    }
                }
                for (int xi = 1; xi <= kMaxX; ++xi) _minoPlacementArray[kMaxY, xi] = false;
            }

            // 座標から配列のindexに変換するための値
            // 左下の座標(-4.5f, -9.5f)をindex(1, 1)にする
            private const float kConvertOffsetX = 5.5f;
            private const float kConvertOffsetY = 10.5f;
            private System.Tuple<int, int> ConvertPosition2Index(float posX, float posY)
            {
                System.Tuple<int, int> convertedIndex = new System.Tuple<int, int>((int)(posX + kConvertOffsetX), (int)(posY + kConvertOffsetY));
                return convertedIndex;
            }

            private System.Tuple<int, int> ConvertY2Index(float lowerY, float upperY)
            {
                System.Tuple<int, int> convertedIndex = new System.Tuple<int, int>((int)(lowerY + kConvertOffsetY), (int)(upperY + kConvertOffsetY));
                return convertedIndex;
            }
        }
    }
}