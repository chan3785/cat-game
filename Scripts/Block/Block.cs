using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DIRECTION
{
    LEFT,
    RIGHT,
    UP,
    DOWN,
    NONE
};
public enum BlockType
{
    NORMAL,
    EVENT,
    BOMB
};

public enum BlockState
{
    STOP,
    MOVE
};
public class Block : MonoBehaviour
{
    public SpriteRenderer _Image;
    public int Type { set; get; }
    public BlockState State { set; get; }

    public float Width { set; get; } // 블럭 넓이 
    public float Speed { set; get; } = 0.1f;   // 속도
    private Vector3 _movePos;   // 움직일 위치
    public Vector3 MovePos
    {
        get => _movePos;
        set => _movePos = value;
    }

    private DIRECTION _direct = DIRECTION.NONE;  // 움직일 방향
    public int Column { set; get; }
    public int Row { set; get; }


    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init(int column, int row, int type, Sprite sprite)
    {
        Column = column;
        Row = row;
        Type = type;
        _Image.sprite = sprite;
    }

    public void Move(DIRECTION direct, int heightCount)
    {
        switch (direct)
        {
            case DIRECTION.LEFT:
                {
                    _direct = DIRECTION.LEFT;
                    State = BlockState.MOVE;
                }
                break;

            case DIRECTION.RIGHT:
                {
                    _direct = DIRECTION.RIGHT;
                    State = BlockState.MOVE;
                }
                break;

            case DIRECTION.UP:
                {
                    _direct = DIRECTION.UP;
                    State = BlockState.MOVE;
                }
                break;
            case DIRECTION.DOWN:
                {
                    _direct = DIRECTION.DOWN;
                    State = BlockState.MOVE;
                }
                break;
        }
    }

    public void Move(DIRECTION direct)
    {
        switch (direct)
        {
            case DIRECTION.LEFT:
                _movePos = transform.position;
                _movePos.x -= Width;
                _direct = DIRECTION.LEFT;
                State = BlockState.MOVE;
                break;

            case DIRECTION.RIGHT:
                _movePos = transform.position;
                _movePos.x += Width;
                _direct = DIRECTION.RIGHT;
                State = BlockState.MOVE;
                break;

            case DIRECTION.UP:
                _movePos = transform.position;
                _movePos.y += Width;
                _direct = DIRECTION.UP;
                State = BlockState.MOVE;
                break;

            case DIRECTION.DOWN:
                _movePos = transform.position;
                _movePos.y -= Width;
                _direct = DIRECTION.DOWN;
                State = BlockState.MOVE;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (State == BlockState.MOVE)
        {
            switch (_direct)
            {
                case DIRECTION.LEFT:
                    if (transform.position.x >= _movePos.x)
                    {
                        transform.Translate(Vector3.left * Speed);
                    }
                    else
                    {
                        transform.position = _movePos;
                        State = BlockState.STOP;
                    }
                    break;

                case DIRECTION.RIGHT:
                    if (transform.position.x <= _movePos.x)
                    {
                        transform.Translate(Vector3.right * Speed);
                    }
                    else
                    {
                        transform.position = _movePos;
                        State = BlockState.STOP;
                    }
                    break;

                case DIRECTION.UP:
                    if (transform.position.y <= _movePos.y)
                    {
                        transform.Translate(Vector3.up * Speed);
                    }
                    else
                    {
                        transform.position = _movePos;
                        State = BlockState.STOP;
                    }
                    break;

                case DIRECTION.DOWN:
                    if (transform.position.y >= _movePos.y)
                    {
                        transform.Translate(Vector3.down * Speed);
                    }
                    else
                    {
                        transform.position = _movePos;
                        State = BlockState.STOP;
                    }
                    break;
            }
        }
    }
}