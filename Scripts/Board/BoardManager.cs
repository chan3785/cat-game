using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum MouseMoveDirection
{
    MOUSEMOVEUP,
    MOUSEMOVEDOWN,
    MOUSEMOVELEFT,
    MOUSEMOVERIGHT
};

public enum GamePlayState
{
    INPUTOK,
    AFTERINPUTMOVECHECK,
    MATCHCHECK,
    AFTERMATCH_MOVECHECK,
    DROPBLOCK,
    AFTERDROP_MOVECHECK,
    INPUTCANCEL
};
public class BoardManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private GameObject _BlockPrefab;

    private float _BlockWidth;  // 블록 넓이
    private Vector2 _screenPos;

    private float _screenWidth = 0.0f;  // 스크린 넓이
    private float _blockScale = 0.0f;   // 블럭의 화면 비율에 따른 스케일 값

    [SerializeField] private float _Xmargin = 0.5f;  // x축 넓이 여백
    [SerializeField] private float _Ymargin = 3.0f; // y축 넓이 여백

    private GameObject[,] _GameBoard;   // 게임 블럭보드
    [SerializeField] private Sprite[] _Sprites;     // 블럭 이미지 배열

    private bool _mouseClick = false;      // 마우스 클릭여부
    private Vector3 _startPos, _endPos;     // 마우스 클릭 시 위치값 startPos, 뗄 때의 위치값 endPos
    private GameObject _ClickedObject;      // 클릭된 블럭 저장용 변수

    private int _Column;        // 현재 게임 행 값
    private int _Row;           // 현재 게임 열 값

    private bool _IsClicked = false;    // 클릭된 블럭이 있는지 여부 저장

    private float _MoveDistance = 0.1f;     // 마우스 움직임 감도 체크 값

    private GamePlayState PlayState { set; get; }       // 현재 게임 상태값 저장

    private List<GameObject> _RemovingBlocks = new List<GameObject>();      // 터질 블럭 저장
    private List<GameObject> _RemovedBlocks = new List<GameObject>();       // 터진 블럭 저장 이후에 리필 때 사용될 블럭을 저장

    [SerializeField] private int TYPECOUNT = 4;         // 게임에 등장하는 블럭의 종류 수
    private int MATCHCOUNT = 3;         // 최소한 매치 되어야 하는 블럭 수

    [SerializeField] private int start_Column = 5;      // 게임보드 생성 시 지정할 행 값
    [SerializeField] private int start_Row = 5;         // 게임보드 생성 시 지정할 열 값


    [SerializeField] private float _Ypos = 3.0f;        // 블럭이 리필될 때 생성되는 시작점

    private bool isMatched;
    private float reverseAngle;                 // 마우스 움직임 정반대 각 저장

    private MouseMoveDirection _dir;

    #endregion

    void Start()
    {
        _screenPos = Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f));

        _screenPos.y = -_screenPos.y;

        _screenWidth = Mathf.Abs(_screenPos.x * 2); // 스크린의 가로 길이를 계산
        _screenWidth -= _Xmargin * 2;       // 가로축 기준 양쪽 여백 제거


        _BlockWidth = _BlockPrefab.GetComponent<Block>()._Image.sprite.rect.size.x / 100;
        MakeBoard(start_Column, start_Row);         // 게임 보드 생성
    }

    private void MakeBoard(int column, int row)
    {
        _Column = column;
        _Row = row;
        float blockWidth = _BlockWidth * row;
        float screenWidth = _screenWidth;

        _blockScale = screenWidth / blockWidth;

        _GameBoard = new GameObject[column, row];


        for (int col = 0; col < column; col++)
        {
            for (int ro = 0; ro < row; ro++)
            {
                _GameBoard[col, ro] = Instantiate(_BlockPrefab) as GameObject;

                _GameBoard[col, ro].transform.localScale = new Vector3(_blockScale, _blockScale, 0.0f); // 화면 사이즈 비율에 따른 브럭의 스케일값 지정


                _GameBoard[col, ro].transform.position =
                    new Vector3(_screenPos.x + _Xmargin + (ro * _BlockWidth * _blockScale) + (_BlockWidth * _blockScale) / 2,
                    _screenPos.y - _Ymargin + (-col * _BlockWidth * _blockScale) - (_BlockWidth * _blockScale) / 2, 0.0f);

                int type = UnityEngine.Random.Range(0, TYPECOUNT);

                var block = _GameBoard[col, ro].GetComponent<Block>();
                block.Init(col, ro, type, _Sprites[type]);
                block.name = string.Format("Block[{0}, {1}]", col, ro);
                block.Width = _BlockWidth * _blockScale;
            }
        }
    }

    #region MoveBlock
    public void MoveAllBlock(DIRECTION direct)
    {
        foreach (var obj in _GameBoard)
        {
            obj.GetComponent<Block>().Move(direct);
        }
    }

    public void OnClickMoveAllBlockLeft()
    {
        MoveAllBlock(DIRECTION.LEFT);
    }

    public void OnClickMoveAllBlockRight()
    {
        MoveAllBlock(DIRECTION.RIGHT);
    }

    public void OnClickMoveAllBlockUp()
    {
        MoveAllBlock(DIRECTION.UP);
    }

    public void OnClickMoveAllBlockDown()
    {
        MoveAllBlock(DIRECTION.DOWN);
    }
    #endregion

    private bool CheckBlockMove()
    {
        foreach (var obj in _GameBoard)
        {
            if (obj != null)
            {
                if (obj.GetComponent<Block>().State == BlockState.MOVE)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void DropBlocks()
    {
        int heightCount = 0;

        for (int row = 0; row < _Row; row++)
        {
            for (int col = _Column - 1; col >= 0; col--)
            {
                if (_GameBoard[col, row] == null)
                {
                    heightCount++;
                }
                else
                {
                    if (heightCount > 0)
                    {
                        var block = _GameBoard[col, row].GetComponent<Block>();
                        block.MovePos = block.transform.position;
                        block.MovePos = new Vector3(block.MovePos.x, block.MovePos.y - block.Width * heightCount, block.MovePos.z);

                        _GameBoard[col, row] = null;

                        block.Column = block.Column + heightCount;
                        block.gameObject.name = $"Bloco[{block.Column}, {block.Row}]";
                        _GameBoard[block.Column, block.Row] = block.gameObject;

                        block.Move(DIRECTION.DOWN, heightCount);
                    }
                }
            }

            heightCount = 0;
        }
    }

    private void CheckMatchBlock()
    {
        List<GameObject> matchList = new List<GameObject>();
        List<GameObject> tempMatchList = new List<GameObject>();

        int checktype = 0;

        if (_RemovingBlocks.Count > 0)
        {
            _RemovingBlocks.Clear();
        }

        for (int row = 0; row < _Row; row++)
        {
            if (_GameBoard[0, row] == null)
            {
                continue;
            }

            checktype = _GameBoard[0, row].GetComponent<Block>().Type;

            tempMatchList.Add(_GameBoard[0, row]);

            for (int col = 1; col < _Column; col++)
            {
                if (_GameBoard[col, row] == null) continue;

                if (checktype == _GameBoard[col, row].GetComponent<Block>().Type)
                {
                    tempMatchList.Add(_GameBoard[col, row]);
                }
                else
                {
                    if (tempMatchList.Count >= 3)
                    {
                        matchList.AddRange(tempMatchList);
                        tempMatchList.Clear();
                        checktype = _GameBoard[col, row].GetComponent<Block>().Type;
                        tempMatchList.Add(_GameBoard[col, row]);
                    }
                    else
                    {
                        tempMatchList.Clear();
                        checktype = _GameBoard[col, row].GetComponent<Block>().Type;
                        tempMatchList.Add(_GameBoard[col, row]);
                    }
                }
            }

            if (tempMatchList.Count >= 3)
            {
                matchList.AddRange(tempMatchList);
                tempMatchList.Clear();
                isMatched = true;
            }
            else
            {
                isMatched = false;
                tempMatchList.Clear();
            }
        }

        // 가로방향 Match 블럭 처리
        for (int col = 0; col < _Column; col++)
        {
            if (_GameBoard[col, 0] == null)
            {
                continue;
            }

            checktype = _GameBoard[col, 0].GetComponent<Block>().Type;

            tempMatchList.Add(_GameBoard[col, 0]);

            for (int row = 1; row < _Row; row++)
            {
                if (_GameBoard[col, row] == null)
                {
                    continue;
                }
                if (checktype == _GameBoard[col, row].GetComponent<Block>().Type)
                {
                    tempMatchList.Add(_GameBoard[col, row]);
                }
                else
                {
                    if (tempMatchList.Count >= 3)
                    {
                        matchList.AddRange(tempMatchList);
                        tempMatchList.Clear();
                        checktype = _GameBoard[col, row].GetComponent<Block>().Type;
                        tempMatchList.Add(_GameBoard[col, row]);
                    }
                    else
                    {
                        tempMatchList.Clear();
                        checktype = _GameBoard[col, row].GetComponent<Block>().Type;
                        tempMatchList.Add(_GameBoard[col, row]);
                    }
                }
            }

            if (tempMatchList.Count >= 3)
            {
                matchList.AddRange(tempMatchList);
                tempMatchList.Clear();
                isMatched = true;
            }
            else
            {
                tempMatchList.Clear();
                isMatched = false;
            }
        }

        // match리스트에 중복 포함된 블럭 참조를 정리
        matchList = matchList.Distinct().ToList();

        if (matchList.Count > 0)
        {
            foreach (var obj in matchList)
            {
                var block = obj.GetComponent<Block>();

                _GameBoard[block.Column, block.Row] = null;
                obj.SetActive(false);
            }

            _RemovingBlocks.AddRange(matchList);
        }
        _RemovedBlocks.AddRange(_RemovingBlocks);
        DropBlocks();
    }

    private GameObject GetNewBlock(int column, int row, int type)
    {
        if (_RemovedBlocks.Count <= 0)
        {
            return null;
        }

        GameObject obj = _RemovedBlocks[0];

        obj.GetComponent<Block>().Init(column, row, type, _Sprites[type]);
        _RemovedBlocks.Remove(obj);

        return obj;
    }


    void CreateNewBlock()
    {
        int heightCount = 0;
        for (int row = 0; row < _Row; row++)
        {
            for (int col = _Column - 1; col >= 0; col--)
            {
                if (_GameBoard[col, row] == null)
                {
                    int type = UnityEngine.Random.Range(0, TYPECOUNT);
                    _GameBoard[col, row] = GetNewBlock(col, row, type);
                    _GameBoard[col, row].name = $"Block[{col}, {row}]";
                    _GameBoard[col, row].gameObject.SetActive(true);
                    var block = _GameBoard[col, row].GetComponent<Block>();

                    _GameBoard[col, row].transform.position =
                        new Vector3(_screenPos.x + _Xmargin + (_BlockWidth * _blockScale) / 2 + row * (_BlockWidth * _blockScale),
                        _screenPos.y - _Ymargin - col * (_BlockWidth * _blockScale) - (_BlockWidth * _blockScale) / 2, 0.0f);

                    block.MovePos = block.transform.position;
                    float moveYpos = _GameBoard[col, row].GetComponent<Block>().MovePos.y + (_BlockWidth * _blockScale) * heightCount++ + _Ypos;

                    _GameBoard[col, row].transform.position =
                        new Vector3(_GameBoard[col, row].GetComponent<Block>().MovePos.x, moveYpos, _GameBoard[col, row].GetComponent<Block>().MovePos.z);

                    block.Move(DIRECTION.DOWN, heightCount);
                }
            }
            heightCount = 0;
        }
    }

    private bool CheckAllBlockInGameBoard()
    {
        foreach (var obj in _GameBoard)
        {
            if (obj == null)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckAfterMoveMatchBlock()
    {
        int checkType = -1; // 비교하는 블럭의 타입을 저장        

        // 세로 Match 블럭 check
        for (int row = 0; row < _Row; row++)
        {
            for (int col = _Column - 1; col >= (MATCHCOUNT - 1); col--)
            {
                // 하단에서 상단으로 진행
                // 상단 이동시 우측에서 이동했을때 매칭 되는 경우
                if (row >= 0 && row < (_Row - 1))
                {
                    // 하단 우측 체크
                    checkType = _GameBoard[row + 1, col].GetComponent<Block>().Type;    // 하단 우측 type값을 저장

                    if ((checkType == _GameBoard[row, col - 1].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row, col - 2].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 중단 우측 체크
                    checkType = _GameBoard[row + 1, col - 1].GetComponent<Block>().Type;    // 중단 우측 type값을 저장

                    if ((checkType == _GameBoard[row, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row, col - 2].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 상단 우측 체크
                    checkType = _GameBoard[row + 1, col - 2].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row, col - 1].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }


                // 상단 이동시 좌측에서 이동했을 때 매칭 되는 경우
                if ((row > 0) && (row <= _Row - 1))
                {
                    // 하단 좌측 체크
                    checkType = _GameBoard[row - 1, col].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row, col - 1].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row, col - 2].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 중단 좌측 체크
                    checkType = _GameBoard[row - 1, col - 1].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row, col - 2].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 상단 좌측 체크
                    checkType = _GameBoard[row - 1, col - 2].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row, col - 1].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }

                // 상단에서 이동했을 떄 매칭되는 경우
                if (col > MATCHCOUNT - 1 && col <= _Column - 1)
                {
                    // 상단 체크
                    checkType = _GameBoard[row, col].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[row, col - 2].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row, col - 3].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 상단 체크
                    checkType = _GameBoard[row, col - 3].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[row, col - 1].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }
            }
        }

        // 가로 Match 블럭 check
        for (int col = 0; col < _Column; col++)
        {
            for (int row = 0; row < (_Row - MATCHCOUNT); row++)
            {
                // 좌측에서 우측으로 진행
                // 우측 이동시 하단에서 이동했을 때 매칭되는 경우
                if (col >= 0 && col < (_Column - 1))
                {

                    // 좌측 하단 체크
                    checkType = _GameBoard[row, col + 1].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row + 1, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row + 2, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 중측 하단 체크
                    checkType = _GameBoard[row + 1, col + 1].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row + 2, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 우측 하단 체크
                    checkType = _GameBoard[row + 2, col + 1].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row + 1, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }

                // 좌측에서 우측으로 진행
                // 우측 이동시 상단에서 이동했을때 매칭되는 경우확인
                if ((col > 0) && (col <= _Column - 1))
                {
                    // 좌측 상단 체크
                    checkType = _GameBoard[row, col - 1].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row + 1, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row + 2, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 중측 상단 체크
                    checkType = _GameBoard[row + 1, col - 1].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row + 2, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }


                    // 우측 상단 체크
                    checkType = _GameBoard[row + 2, col - 1].GetComponent<Block>().Type;    // 상단 우측의  type값을 저장

                    if ((checkType == _GameBoard[row, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row + 1, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }


                if (row >= 0 && row < _Row - MATCHCOUNT)
                {
                    // 상단 체크
                    checkType = _GameBoard[row, col].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[row + 1, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row + 3, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 상단 체크
                    checkType = _GameBoard[row, col].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[row + 2, col].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[row + 3, col].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }



    private float CalculateAngle(Vector3 from, Vector3 to)
    {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
    }

    private MouseMoveDirection calculateDirection()
    {
        float angle = CalculateAngle(_startPos, _endPos);
        reverseAngle = angle + 180.0f;

        return DetermineDirection(angle);

    }

    private MouseMoveDirection DetermineDirection(float angle)
    {
        if (angle >= 315.0f && angle <= 360.0f || angle >= 0.0f && angle < 45.0f)
        {
            return MouseMoveDirection.MOUSEMOVEUP;
        }
        else if (angle >= 45.0f && angle < 135.0f)
        {
            return MouseMoveDirection.MOUSEMOVELEFT;
        }
        else if (angle >= 135.0f && angle < 225.0f)
        {
            return MouseMoveDirection.MOUSEMOVEDOWN;
        }
        else if (angle >= 225.0f && angle < 315.0f)
        {
            return MouseMoveDirection.MOUSEMOVERIGHT;
        }

        return MouseMoveDirection.MOUSEMOVEDOWN;
    }

    private void MouseMove()
    {
        float diff = Vector2.Distance(_startPos, _endPos);

        if (diff > _MoveDistance && _ClickedObject != null)
        {
            _dir = calculateDirection();

            int column = _ClickedObject.GetComponent<Block>().Column;
            int row = _ClickedObject.GetComponent<Block>().Row;

            switch (_dir)
            {
                case MouseMoveDirection.MOUSEMOVELEFT:
                    {
                        if (row > 0)
                        {
                            _GameBoard[column, row].GetComponent<Block>().Row = row - 1;
                            _GameBoard[column, row - 1].GetComponent<Block>().Row = row;

                            _GameBoard[column, row] = _GameBoard[column, row - 1];
                            _GameBoard[column, row - 1] = _ClickedObject;

                            _GameBoard[column, row].GetComponent<Block>().Move(DIRECTION.RIGHT);
                            _GameBoard[column, row - 1].GetComponent<Block>().Move(DIRECTION.LEFT);

                            PlayState = GamePlayState.AFTERINPUTMOVECHECK;
                        }
                    }

                    break;

                case MouseMoveDirection.MOUSEMOVERIGHT:
                    {
                        // 열 값이 0보다 큰 경우에 우측 이동이 가능
                        if (row < _Row - 1)
                        {
                            // 이동할 위치의 행과 열로 위치 값을 갱신
                            _GameBoard[column, row].GetComponent<Block>().Row = row + 1;
                            _GameBoard[column, row + 1].GetComponent<Block>().Row = row;

                            // 게임 보드 상의 참조 위치 값도 변경
                            _GameBoard[column, row] = _GameBoard[column, row + 1];
                            _GameBoard[column, row + 1] = _ClickedObject;

                            //움직이도록 명량
                            _GameBoard[column, row].GetComponent<Block>().Move(DIRECTION.LEFT);
                            _GameBoard[column, row + 1].GetComponent<Block>().Move(DIRECTION.RIGHT);

                            PlayState = GamePlayState.AFTERINPUTMOVECHECK;
                        }
                    }

                    break;

                case MouseMoveDirection.MOUSEMOVEUP:
                    {
                        if (column > 0)
                        {
                            _GameBoard[column, row].GetComponent<Block>().Column = column - 1;
                            _GameBoard[column - 1, row].GetComponent<Block>().Column = column;

                            _GameBoard[column, row] = _GameBoard[column - 1, row];
                            _GameBoard[column - 1, row] = _ClickedObject;

                            _GameBoard[column, row].GetComponent<Block>().Move(DIRECTION.DOWN);
                            _GameBoard[column - 1, row].GetComponent<Block>().Move(DIRECTION.UP);

                            PlayState = GamePlayState.AFTERINPUTMOVECHECK;
                        }
                    }

                    break;

                case MouseMoveDirection.MOUSEMOVEDOWN:
                    {
                        if (column < _Column - 1)
                        {
                            _GameBoard[column, row].GetComponent<Block>().Column = column + 1;
                            _GameBoard[column + 1, row].GetComponent<Block>().Column = column;

                            _GameBoard[column, row] = _GameBoard[column + 1, row];
                            _GameBoard[column + 1, row] = _ClickedObject;

                            _GameBoard[column, row].GetComponent<Block>().Move(DIRECTION.UP);
                            _GameBoard[column + 1, row].GetComponent<Block>().Move(DIRECTION.DOWN);

                            PlayState = GamePlayState.AFTERINPUTMOVECHECK;
                        }
                    }

                    break;
            }
            _startPos = _endPos = Vector3.zero;
            _ClickedObject = null;
            _mouseClick = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (PlayState)
        {
            case GamePlayState.INPUTOK:
                // 마우스 버튼 누름
                if (Input.GetMouseButtonDown(0))
                {
                    _mouseClick = true;
                    _endPos = _startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    _endPos.z = _startPos.z = 0.0f;

                    _IsClicked = false;

                    for (int col = 0; col < _Column; col++)
                    {
                        for (int row = 0; row < _Row; row++)
                        {
                            if (_GameBoard[col, row] != null)
                            {
                                // 블럭의 이미지 사각영역에 클릭된 좌표값이 포함되는지 확인.
                                _IsClicked = _GameBoard[col, row].GetComponent<Block>()._Image.bounds.Contains(_startPos);
                            }

                            // 클릭된 블럭이 있음
                            if (_IsClicked)
                            {
                                _ClickedObject = _GameBoard[col, row];        // 클릭된 블럭을 _ClickObject에 저장
                                goto SearchExit;
                            }
                        }
                    }

                SearchExit:;
                }

                // 마우스 버튼 놓음
                if (Input.GetMouseButtonUp(0))
                {
                    _mouseClick = false;

                    _startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    _endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    _ClickedObject = null;
                }

                // 마우스클릭된 상태에서 마우스 커서 움직임(mousemove)
                if ((_mouseClick == true) && ((Input.GetAxis("Mouse X") < 0 || Input.GetAxis("Mouse X") > 0) ||
                    (Input.GetAxis("Mouse Y") < 0 || Input.GetAxis("Mouse Y") > 0)))
                {
                    _endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    _endPos.z = 0.0f;

                    MouseMove();
                }
                break;

            case GamePlayState.AFTERINPUTMOVECHECK:
                {
                    if (!CheckBlockMove())
                    {
                        //PlayState = GamePlayState.INPUTOK;
                        PlayState = GamePlayState.MATCHCHECK;
                    }
                }
                break;

            case GamePlayState.MATCHCHECK:
                {
                    CheckMatchBlock();
                    if (isMatched)
                    {
                        PlayState = GamePlayState.AFTERMATCH_MOVECHECK;
                    }
                    else
                    {
                        PlayState = GamePlayState.INPUTCANCEL;
                    }

                }

                break;

            case GamePlayState.INPUTCANCEL:
                {
                    PlayState = GamePlayState.AFTERMATCH_MOVECHECK;
                }

                break;

            case GamePlayState.DROPBLOCK:
                {
                    CreateNewBlock();
                    PlayState = GamePlayState.AFTERDROP_MOVECHECK;
                }
                break;

            case GamePlayState.AFTERMATCH_MOVECHECK:

            case GamePlayState.AFTERDROP_MOVECHECK:
                {
                    if (!CheckBlockMove())
                    {
                        if (PlayState == GamePlayState.AFTERMATCH_MOVECHECK)
                        {
                            if (CheckAllBlockInGameBoard())
                            {
                                if (CheckAfterMoveMatchBlock())
                                {
                                    PlayState = GamePlayState.INPUTOK;
                                }
                                else
                                {
                                    Application.Quit();
                                    UnityEditor.EditorApplication.isPlaying = false;
                                    Debug.Log("Game Over");
                                }
                            }
                            else
                            {
                                PlayState = GamePlayState.DROPBLOCK;
                            }
                        }
                        else if (PlayState == GamePlayState.AFTERDROP_MOVECHECK)
                        {
                            PlayState = GamePlayState.MATCHCHECK;
                        }
                    }
                }
                break;
        }
    }
}