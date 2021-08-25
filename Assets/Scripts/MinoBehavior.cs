using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Tetris
{
    public class MinoBehavior : MonoBehaviour
    {
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
        private int _levels;

        // ホールド状態かどうか
        private bool _isHold;

        // このミノの一番左の座標
        private float _currentLeftPosition;
        // このミノの一番右の座標
        private float _currentRightPosition;

        // 座標周りの定数
        // 左端の座標
        private const float kMinPositionX = -4.5f;
        // 右端の座標
        private const float kMaxPositionX = 4.5f;

        private void Awake() {
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
                .Where(_ => (Input.GetKeyDown(KeyCode.R)))
                .BatchFrame(0, FrameCountType.FixedUpdate)
                .Subscribe(_ => RotateLeft());

            // Eキーor。右回転
            this.UpdateAsObservable()
                .Where(_ => (Input.GetKeyDown(KeyCode.E)))
                .BatchFrame(0, FrameCountType.FixedUpdate)
                .Subscribe(_ => RotateRight());

            // キーor。ホールド
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
        public void StartMoveMino()
        {
            SetLeftRightPosition();
            // this._levels = .GetComponent<>().GetCurrentLevels();
            // levelsに合わせて落下時間を設定。
            float repeatRate = _levels;
            //InvokeRepeating("", 0f, repeatRate);
        }

        public void HoldMino()
        {
            if(!_minoManager.GetComponent<MinoController>().canHoldMino()) return;
            _isHold = true;
        }

        private void MoveLeft()
        {
            // 既に左端の場合は移動できない
            if(_currentLeftPosition <= kMinPositionX) return;
            Vector3 newMinoPosition = this.gameObject.transform.position;
            newMinoPosition.x -= 1;
            this.gameObject.transform.position = newMinoPosition;
            SetLeftRightPosition();
        }

        private void MoveRight()
        {
            // 既に右端の場合は移動できない
            if(_currentRightPosition >= kMaxPositionX) return;
            Vector3 newMinoPosition = this.gameObject.transform.position;
            newMinoPosition.x += 1;
            this.gameObject.transform.position = newMinoPosition;
            SetLeftRightPosition();
        }

        private void MoveDown()
        {

        }

        private void MoveHardDrop()
        {

        }

        private void RotateLeft()
        {

        }

        private void RotateRight()
        {

        }

        public void SetMinoManager(GameObject minoManager){
            this._minoManager = minoManager;
        }
    }
}