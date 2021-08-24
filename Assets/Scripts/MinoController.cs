using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tetris
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
        // ミノの座標周りのパラメータ
        // J, L, S, T, Zミノの生成位置。中央から1マス左に生成する。
        private readonly Vector3 kStartingJLSTZMinoPosition = new Vector3(-0.5f, 8.5f, 0f);
        // I, Oミノの生成位置。中央に生成する。
        private readonly Vector3 kStartingIOMinoPosition = new Vector3(0f, 9f, 0f);
        // ホールドミノの座標。
        private readonly Vector3 kHoldMinoPosition = new Vector3(-7.5f, -7.0f, 0.0f);
        // ネクストミノの座標。この値にOffsetY * 順番を引いた値が最終的な座標になる。
        private readonly Vector3 kNextMinoPosition = new Vector3(7.5f, 7.5f, 0f);
        private const float kNextMinoOffsetY = 1.5f;

        // ミノのスケール。生成時は1、ホールド時は0.8にする。
        private readonly Vector3 kStartingMinoScale = new Vector3(1.0f, 1.0f, 1.0f);
        private readonly Vector3 kHoldMinoScale = new Vector3(0.8f, 0.8f, 0.8f);
        private readonly Vector3 kNextMinoScale = new Vector3(0.5f, 0.5f, 0.5f);

        // ミノオブジェクト周りの変数
        // 現在フォーカスされているミノ
        private GameObject _currentMino;
        // ホールドされているミノ。最初にホールドされるまではnullオブジェクト。
        private GameObject _holdMino = null;
        // ネクストミノオブジェクトを保持するキュー。常に6種類のミノを保持する。
        private Queue<GameObject> _nextMinos;
        // ネクストミノタイプを保持するキュー。ネクストミノは7種類のミノをランダムに並べたセットを繰り返す。
        private Queue<Tetromino> _nextMinosType;

        private const int kMaxNextMinoSize = 6; // ネクストミノの個数
        // テトリミノの種類一覧
        private Tetromino[] _tetrominos = new Tetromino[] { Tetromino.IMino, Tetromino.JMino, Tetromino.LMino, Tetromino.OMino, Tetromino.SMino, Tetromino.TMino, Tetromino.ZMino };

        private GameObject[] _minoObjects;

        void Awake()
        {
            _nextMinos = new Queue<GameObject>();
            _nextMinosType = new Queue<Tetromino>();
        }

        void Start()
        {
            _minoObjects = Resources.LoadAll<GameObject>("Tetromino");
            Debug.Log(_minoObjects[0]);

            SetNextMinosType();
            for (int i = 0; i < kMaxNextMinoSize; ++i)
            {
                GenerateNextMino();
            }

            // テトリミノを取得してゲーム開始
            _currentMino = GetNextMino();
            //_currentMino.GetComponent<MinoBehavior>().StartMoveMino();
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
            Debug.Log(nextMinoType);
            GameObject generatedMino = Instantiate(_minoObjects[(int)nextMinoType]);
            Debug.Log(generatedMino);
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
            nextMino.transform.localScale = kStartingMinoScale;
            GenerateNextMino();

            // 取得したミノを操作ミノの位置にセット
            // IミノOミノは他のミノと生成位置が異なる
            switch (nextMino.GetComponent<MinoBehavior>().minoType)
            {
                case Tetromino.IMino:
                case Tetromino.OMino:
                    nextMino.transform.position = kStartingIOMinoPosition;
                    break;
                case Tetromino.JMino:
                case Tetromino.LMino:
                case Tetromino.SMino:
                case Tetromino.TMino:
                case Tetromino.ZMino:
                    nextMino.transform.position = kStartingJLSTZMinoPosition;
                    break;
            }
            return nextMino;
        }

        public void HoldMino()
        {
            if (_holdMino == null)
            {
                _holdMino = _currentMino;
                _currentMino = GetNextMino();
            }
            else
            {
                GameObject tmpMino = _currentMino;
                _currentMino = _holdMino;
                _holdMino = tmpMino;
            }
            _currentMino.GetComponent<MinoBehavior>().StartMoveMino();
            _holdMino.GetComponent<MinoBehavior>().HoldMoveMino();
        }
    }
}