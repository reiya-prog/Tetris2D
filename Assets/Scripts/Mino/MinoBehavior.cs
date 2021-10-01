using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Tetris
{
    namespace Mino
    {
        public class MinoBehavior : MonoBehaviour
        {
            // ミノマネージャーオブジェクト
            private GameObject _minoManager;

            // 子オブジェクトの参照用メンバ変数
            // 回転軸の中心オブジェクト
            private GameObject _rotationPivot;
            // ミノの構成ブロック
            // 0=upperBlock, 1=leftBlock, 2=rightBlock, 3=bottomBlock
            private List<GameObject> _componentBlock;

            // アタッチされているミノの種類
            [SerializeField]
            private Tetromino _minoType;
            // 外部アクセス用変数
            public Tetromino minoType
            {
                get { return _minoType; }
            }
            // 現在のレベル。落下速度に影響する。
            private int _levels = 1;

            // ホールド状態かどうか
            private bool _isHeld = false;

            // 設置済みかどうか
            private bool _isFixed = false;

            // このミノの一番左の座標
            private float _currentLeftPosition;
            // このミノの一番右の座標
            private float _currentRightPosition;
            // このミノの一番上の座標
            private float _currentUpperPosition;
            // このミノの一番下の座標
            private float _currentBottomPosition;

            // 座標周りの定数
            // 左端の座標
            private const float kMinPositionX = -4.5f;
            // 右端の座標
            private const float kMaxPositionX = 4.5f;
            // 上端の座標
            private const float kMaxPositionY = 10.5f;
            // 下端の座標
            private const float kMinPositionY = -9.5f;

            // 左方向の移動
            private readonly Vector3 kMoveLeftDirection = new Vector3(-1.0f, 0.0f, 0.0f);
            // 右方向の移動
            private readonly Vector3 kMoveRightDirection = new Vector3(1.0f, 0.0f, 0.0f);
            // 下方向の移動
            private readonly Vector3 kMoveDownDirection = new Vector3(0.0f, -1.0f, 0.0f);

            private const float _maxFlameRate = 60.0f;

            private void Awake()
            {
                // メンバ変数の初期化
                _rotationPivot = this.transform.GetChild(0).gameObject;
                _componentBlock = new List<GameObject>();
                _componentBlock.Add(_rotationPivot.transform.GetChild(0).gameObject);
                _componentBlock.Add(_rotationPivot.transform.GetChild(1).gameObject);
                _componentBlock.Add(_rotationPivot.transform.GetChild(2).gameObject);
                _componentBlock.Add(_rotationPivot.transform.GetChild(3).gameObject);
            }

            private void Start()
            {
                // インタラクションの設定
                // Wキーor↑キー。現在のミノをすぐに真下に移動（ハードドロップ）
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.W)))
                    .Subscribe(_ => MoveHardDrop());

                // Aキーor←キー。現在のミノを左に移動。
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.A)))
                    .Subscribe(_ => MoveLeft());

                // Sキーor↓キー。現在のミノを下に移動。
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.S)))
                    .Subscribe(_ => MoveDown());

                // Dキーor→キー。現在のミノを右に移動
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.D)))
                    .Subscribe(_ => MoveRight());

                // Qキーor。左回転
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.Q)))
                    .Subscribe(_ => RotateLeft());

                // Eキーor。右回転
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.E)))
                    .Subscribe(_ => RotateRight());

                // LControlキーor。ホールド
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.LeftControl)))
                    .Subscribe(_ => HoldMino());
            }

            private void SetMinoPosition()
            {
                // 左端と上端は最大値で初期化、右端と下端は最小値で初期化しておく。
                float minLeft = kMaxPositionX;
                float maxRight = kMinPositionX;
                float minUpper = kMaxPositionX;
                float maxBottom = kMinPositionX;
                for (int i = 0; i < _componentBlock.Count; ++i)
                {
                    minLeft = Mathf.Min(minLeft, _componentBlock[i].transform.position.x);
                    maxRight = Mathf.Max(maxRight, _componentBlock[i].transform.position.x);
                    minUpper = Mathf.Min(minUpper, _componentBlock[i].transform.position.y);
                    maxBottom = Mathf.Max(maxBottom, _componentBlock[i].transform.position.y);
                }
                _currentLeftPosition = minLeft;
                _currentRightPosition = maxRight;
                _currentUpperPosition = minUpper;
                _currentBottomPosition = maxBottom;
            }

            // 現在のレベルを基に落下速度用の値に変換
            // 具体的には_levels^(3/2)の値を返し、その値をもとに落下速度を算出する
            private float FixedLevel()
            {
                return _levels * Mathf.Sqrt(_levels);
            }

            public void StartMoveMino()
            {
                SetMinoPosition();
                // this._levels = .GetComponent<>().GetCurrentLevels();
                // levelsに合わせて落下時間を設定。最高速度は1fに1マス落下
                float repeatRate = _maxFlameRate / FixedLevel();
                repeatRate = Mathf.Max(repeatRate, 1.0f);
                Invoke("MoveDownInvoke", repeatRate / _maxFlameRate);
            }

            public void HoldMino()
            {
                if (_isHeld || _isFixed) return;
                if (!_minoManager.GetComponent<MinoController>().canHoldMino()) return;
                _isHeld = true;
                CancelInvoke("MoveDownInvoke");
                _minoManager.GetComponent<MinoController>().HoldMino();
            }

            // 移動先にミノがあるかチェックする。ある場合:true,移動できない。ない場合:false,移動できる。
            // 引数は移動方向(x==1:右方向, x==-1:右方向, y==-1:下方向, y=1 or z!=0:無効な移動)
            private bool CheckMovable(Vector3 direction)
            {
                // 無効な移動
                if (direction.y == 1.0f || direction.z != 0.0f) return true;
                bool retFlag = false;
                // どこか一か所でもミノが存在(CheckMinoPlacementがtrue)の場合は移動できない
                for (int i = 0; i < _componentBlock.Count; ++i)
                {
                    Vector3 checkPosition = _componentBlock[i].transform.position;
                    checkPosition += direction;
                    retFlag |= _minoManager.GetComponent<MinoController>().CheckMinoPlacement(checkPosition);
                }
                return retFlag;
            }

            private void MoveLeft()
            {
                // 移動ができるのはホールド状態でなく設置状態でもない時
                if (_isHeld || _isFixed) return;
                // 左に壁かミノがある場合移動できない
                if (CheckMovable(kMoveLeftDirection)) return;
                Vector3 newMinoPosition = this.gameObject.transform.position + kMoveLeftDirection;
                this.gameObject.transform.position = newMinoPosition;
                SetMinoPosition();
            }

            private void MoveRight()
            {
                // 移動ができるのはホールド状態でなく設置状態でもない時
                if (_isHeld || _isFixed) return;
                // 右に壁かミノがある場合移動できない
                if (CheckMovable(kMoveRightDirection)) return;
                Vector3 newMinoPosition = this.gameObject.transform.position + kMoveRightDirection;
                this.gameObject.transform.position = newMinoPosition;
                SetMinoPosition();
            }

            private void MoveDown()
            {
                if (_isHeld || _isFixed) return;
                // 自動落下を一時停止
                CancelInvoke("MoveDownInvoke");
                // 移動ができるのはホールド状態でなく設置状態でもない時
                if (_isHeld || _isFixed) return;
                // これ以上下に移動できない場合、その位置にミノを固定する
                if (CheckMovable(kMoveDownDirection))
                {
                    FixMino();
                    return;
                }
                // ミノを1マス落とす
                Vector3 newMinoPosition = this.gameObject.transform.position + kMoveDownDirection;
                this.gameObject.transform.position = newMinoPosition;
                // 自動落下を再開
                float repeatRate = _maxFlameRate / FixedLevel();
                repeatRate = Mathf.Max(repeatRate, 1.0f);
                Invoke("MoveDownInvoke", repeatRate / _maxFlameRate);
            }

            // 時間経過でミノを落下させる
            private void MoveDownInvoke()
            {
                // これ以上下に移動できない場合、その位置にミノを固定する
                if (CheckMovable(kMoveDownDirection))
                {
                    FixMino();
                    return;
                }
                // ミノを1マス落とす
                Vector3 newMinoPosition = this.gameObject.transform.position + kMoveDownDirection;
                // 自動落下させる
                this.gameObject.transform.position = newMinoPosition;
                float repeatRate = _maxFlameRate / FixedLevel();
                repeatRate = Mathf.Max(repeatRate, 1.0f);
                Invoke("MoveDownInvoke", repeatRate / _maxFlameRate);
            }

            private void MoveHardDrop()
            {
                if (_isHeld || _isFixed) return;
                // 自動落下を一時停止
                CancelInvoke("MoveDownInvoke");
                while (!CheckMovable(kMoveDownDirection))
                {
                    // ミノを1マス落とす
                    Vector3 newMinoPosition = this.gameObject.transform.position + kMoveDownDirection;
                    this.gameObject.transform.position = newMinoPosition;
                }
                FixMino();
            }

            // z軸中心に90°回転
            private void RotateLeft()
            {
                // 回転ができるのはホールド状態でなく設置状態でもない時
                if (_isHeld || _isFixed) return;
                // 回転中心オブジェクトをもとにz軸に90°回転
                _rotationPivot.transform.Rotate(transform.forward, 90);
                // もし回転で左右の壁を越えてしまった場合はぶつからない位置まで横移動

                SetMinoPosition();
            }

            // z軸中心に-90°回転
            private void RotateRight()
            {
                // 回転ができるのはホールド状態でなく設置状態でもない時
                if (_isHeld || _isFixed) return;
                // 回転中心オブジェクトをもとにz軸に-90°回転
                _rotationPivot.transform.Rotate(transform.forward, -90);
                SetMinoPosition();
                // もし回転で左右の壁を越えてしまった場合はぶつからない位置まで横移動

            }

            // ミノを固定する関数
            public void FixMino()
            {
                // 固定フラグを建てる
                this._isFixed = true;
                // 設置したミノの座標からPlacementフラグを建てる
                for (int i = 0; i < _componentBlock.Count; ++i)
                {
                    Vector3 fixedPosition = _componentBlock[i].transform.position;
                    _minoManager.GetComponent<MinoController>().SetMinoPlacement(fixedPosition);
                }
                _minoManager.GetComponent<MinoController>().FixMino(_currentBottomPosition, _currentUpperPosition);
            }

            public bool DestroyMino(float positionY)
            {
                for (int i = 0; i < _componentBlock.Count; ++i)
                {
                    if (_componentBlock[i] == null) continue;
                    if (_componentBlock[i].transform.position.y == positionY)
                    {
                        Destroy(_componentBlock[i]);
                        _componentBlock.RemoveAt(i);
                    }
                    else if (_componentBlock[i].transform.position.y > positionY)
                    {
                        _componentBlock[i].transform.position -= kMoveDownDirection;
                    }
                }
                return _componentBlock.Count != 0 ? false : true;
            }

            public void SetMinoManager(GameObject minoManager)
            {
                this._minoManager = minoManager;
            }
        }
    }
}