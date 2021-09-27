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
            private GameObject[] _componentBlock = new GameObject[4];

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
            private bool _isHold = false;

            // 設置済みかどうか
            private bool _isStable = false;

            // このミノの一番左の座標
            private float _currentLeftPosition;
            // このミノの一番右の座標
            private float _currentRightPosition;

            // 座標周りの定数
            // 左端の座標
            private const float kMinPositionX = -4.5f;
            // 右端の座標
            private const float kMaxPositionX = 4.5f;
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
                _componentBlock[0] = _rotationPivot.transform.GetChild(0).gameObject;
                _componentBlock[1] = _rotationPivot.transform.GetChild(1).gameObject;
                _componentBlock[2] = _rotationPivot.transform.GetChild(2).gameObject;
                _componentBlock[3] = _rotationPivot.transform.GetChild(3).gameObject;
            }

            private void Start()
            {
                // インタラクションの設定
                // Wキーor↑キー。現在のミノをすぐに真下に移動（ハードドロップ）
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.W)))
                    .BatchFrame(0, FrameCountType.FixedUpdate)
                    .Subscribe(_ => MoveHardDrop());

                // Aキーor←キー。現在のミノを左に移動。
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.A)))
                    .BatchFrame(0, FrameCountType.FixedUpdate)
                    .Subscribe(_ => MoveLeft());

                // Sキーor↓キー。現在のミノを下に移動。
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.S)))
                    .BatchFrame(0, FrameCountType.FixedUpdate)
                    .Subscribe(_ => MoveDown());

                // Dキーor→キー。現在のミノを右に移動
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.D)))
                    .BatchFrame(0, FrameCountType.FixedUpdate)
                    .Subscribe(_ => MoveRight());

                // Qキーor。左回転
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.Q)))
                    .BatchFrame(0, FrameCountType.FixedUpdate)
                    .Subscribe(_ => RotateLeft());

                // Eキーor。右回転
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.E)))
                    .BatchFrame(0, FrameCountType.FixedUpdate)
                    .Subscribe(_ => RotateRight());

                // LControlキーor。ホールド
                this.UpdateAsObservable()
                    .Where(_ => (Input.GetKeyDown(KeyCode.LeftControl)))
                    .BatchFrame(0, FrameCountType.FixedUpdate)
                    .Subscribe(_ => HoldMino());
            }

            private void SetLeftRightPosition()
            {
                // 左端は最大値で初期化、右端は最小値で初期化しておく。
                float minLeft = kMaxPositionX;
                float maxRight = kMinPositionX;
                for (int i = 0; i < _componentBlock.Length; ++i)
                {
                    minLeft = Mathf.Min(minLeft, _componentBlock[i].transform.position.x);
                    maxRight = Mathf.Max(maxRight, _componentBlock[i].transform.position.x);
                }
                _currentLeftPosition = minLeft;
                _currentRightPosition = maxRight;
            }

            // 現在のレベルを基に落下速度用の値に変換
            // 具体的には_levels^(3/2)の値を返し、その値をもとに落下速度を算出する
            private float FixedLevel()
            {
                return _levels * Mathf.Sqrt(_levels);
            }

            public void StartMoveMino()
            {
                SetLeftRightPosition();
                // this._levels = .GetComponent<>().GetCurrentLevels();
                // levelsに合わせて落下時間を設定。最高速度は1fに1マス落下
                float repeatRate = _maxFlameRate / FixedLevel();
                repeatRate = Mathf.Max(repeatRate, 1.0f);
                Invoke("MoveDownInvoke", repeatRate / _maxFlameRate);
            }

            public void HoldMino()
            {
                if (!_minoManager.GetComponent<MinoController>().canHoldMino()) return;
                _isHold = true;
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
                for (int i = 0; i < _componentBlock.Length; ++i)
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
                if (_isHold || _isStable) return;
                // 左に壁かミノがある場合移動できない
                if (CheckMovable(kMoveLeftDirection)) return;
                Vector3 newMinoPosition = this.gameObject.transform.position + kMoveLeftDirection;
                this.gameObject.transform.position = newMinoPosition;
                SetLeftRightPosition();
            }

            private void MoveRight()
            {
                // 移動ができるのはホールド状態でなく設置状態でもない時
                if (_isHold || _isStable) return;
                // 右に壁かミノがある場合移動できない
                if (CheckMovable(kMoveRightDirection)) return;
                Vector3 newMinoPosition = this.gameObject.transform.position + kMoveRightDirection;
                this.gameObject.transform.position = newMinoPosition;
                SetLeftRightPosition();
            }

            private void MoveDown()
            {
                // 自動落下を一時停止
                CancelInvoke("MoveDownInvoke");
                // 移動ができるのはホールド状態でなく設置状態でもない時
                if (_isHold || _isStable) return;
                // これ以上下に移動できない場合、その位置にミノを固定する
                if(CheckMovable(kMoveDownDirection)){
                    return;
                }
                // ミノを1マス落とす
                Vector3 newMinoPosition = this.gameObject.transform.position + kMoveDownDirection;
                // 自動落下を再開
                this.gameObject.transform.position = newMinoPosition;
                float repeatRate = _maxFlameRate / FixedLevel();
                repeatRate = Mathf.Max(repeatRate, 1.0f);
                Invoke("MoveDownInvoke", repeatRate / _maxFlameRate);
            }

            // 時間経過でミノを落下させる
            private void MoveDownInvoke()
            {
                // これ以上下に移動できない場合、その位置にミノを固定する
                if(CheckMovable(kMoveDownDirection)){
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

            }

            private void RotateLeft()
            {
                // 回転ができるのはホールド状態でなく設置状態でもない時
                if (_isHold || _isStable) return;
                // 回転中心オブジェクトをもとにz軸に90°回転
                _rotationPivot.transform.Rotate(transform.forward, 90);
                SetLeftRightPosition();
                // もし回転で左右の壁を越えてしまった場合はぶつからない位置まで横移動

            }

            // z軸に-90°
            private void RotateRight()
            {
                // 回転ができるのはホールド状態でなく設置状態でもない時
                if (_isHold || _isStable) return;
                // 回転中心オブジェクトをもとにz軸に-90°回転
                _rotationPivot.transform.Rotate(transform.forward, -90);
                SetLeftRightPosition();
                // もし回転で左右の壁を越えてしまった場合はぶつからない位置まで横移動

            }

            public void SetMinoManager(GameObject minoManager)
            {
                this._minoManager = minoManager;
            }
        }
    }
}