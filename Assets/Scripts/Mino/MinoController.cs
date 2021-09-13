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
            // このスクリプトがアタッチされているゲームオブジェクト。MinoGeneraterもアタッチされている。
            private GameObject MinoManager;

            // ミノの座標周りの定数
            // ホールドミノの座標。
            private readonly Vector3 kHoldMinoPosition = new Vector3(-7.5f, 7.0f, 0.0f);
            // ミノの回転。ホールドに入れる際に使用
            private readonly Vector3 kHoldMinoRotation = new Vector3(0.0f, 0.0f, 0.0f);
            // ホールド状態のミノのスケール。全て0.8。
            private readonly Vector3 kHoldMinoScale = new Vector3(0.8f, 0.8f, 0.8f);

            // ミノオブジェクト周りの変数
            // 現在フォーカスされているミノ
            private GameObject _currentMino;
            // ホールドされているミノ。最初にホールドされるまではnullオブジェクト。
            private GameObject _holdMino = null;

            // ホールドをしていいかどうか
            private bool _canHold = true;

            private void Awake()
            {
                MinoManager = this.gameObject;
                this.enabled = false;
            }

            private void Start()
            {
                // テトリミノを取得してゲーム開始
                _currentMino = MinoManager.GetComponent<MinoGenerater>().GetNextMino();
                _currentMino.GetComponent<MinoBehavior>().enabled = true;
                _currentMino.GetComponent<MinoBehavior>().SetMinoManager(this.gameObject);
                _currentMino.GetComponent<MinoBehavior>().StartMoveMino();
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
                _currentMino = MinoManager.GetComponent<MinoGenerater>().GetNextMino();
                _currentMino.GetComponent<MinoBehavior>().enabled = true;
                _currentMino.GetComponent<MinoBehavior>().SetMinoManager(this.gameObject);
                _currentMino.GetComponent<MinoBehavior>().StartMoveMino();
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
                    _currentMino = MinoManager.GetComponent<MinoGenerater>().GetNextMino();
                    _currentMino.GetComponent<MinoBehavior>().SetMinoManager(this.gameObject);
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
                // 次のミノのMinoBehaviorを有効にして開始。
                _currentMino.GetComponent<MinoBehavior>().enabled = true;
                _currentMino.GetComponent<MinoBehavior>().StartMoveMino();
                _holdMino.GetComponent<MinoBehavior>().HoldMino();
            }
        }
    }
}