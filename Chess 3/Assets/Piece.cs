using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Piece : MonoBehaviour {

    public enum PieceType { grey, purple, blue, green, orange, black, red, yellow, brown, white }

    public PieceType type;
    public string team;
    [HideInInspector]
    public List<GameObject> selectedGreyPieces;

    [SerializeField]
    private GameObject openCell;
    [SerializeField]
    private GameObject greySelectCell;
    [SerializeField]
    private GameObject cross;

    [HideInInspector]
    public Vector2 pos;
    private GameObject[] cells;
    [HideInInspector]
    public List<GameObject> pieces = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> friendlyPieces = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> enemyPieces = new List<GameObject>();
    private List<GameObject> crosses = new List<GameObject>();

    private Sprite sprite;
    private GameManager gameManager;

    private List<Vector2> onBoardMoves = new List<Vector2>();
    private List<Vector2> greySelectMoves = new List<Vector2>();
    public List<Vector2> greyPlacementMoves = new List<Vector2>();

    [HideInInspector]
    public bool carry = false;
    [HideInInspector]
    public float z;
    private bool pieceCanMoveOnFriendly = false;
    private bool pieceCantTake = false;
    [HideInInspector]
    public Vector2 direction;
    [HideInInspector]
    public bool invincible = false;
    [HideInInspector]
    public int killCount = 0;
    [HideInInspector]
    public int greyPieceCount = 0;

	// Use this for initialization
	void Start () {
        sprite = GetComponent<SpriteRenderer>().sprite;
        gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
        if (type == PieceType.green)
            z = -1.5f;
        else
            z = -1;
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
        pos = transform.position;
        cells = GameObject.FindGameObjectsWithTag("Cell");


        foreach (GameObject cross in GameObject.FindGameObjectsWithTag("Cross")) { crosses.Add(cross); };
        foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece")) { pieces.Add(piece); };
        foreach (GameObject piece in pieces)
        {
            if (piece.name.Contains(team))
            {
                friendlyPieces.Add(piece);
            }
            else
            {
                enemyPieces.Add(piece);
            }
        }
        if (type == PieceType.brown || type == PieceType.green)
            pieceCanMoveOnFriendly = true;
        if (type == PieceType.green || type == PieceType.white)
            pieceCantTake = true;
    }

    private void Update()
    {
        pos = transform.position;

        if (Input.GetKeyDown(KeyCode.Space) && gameObject == gameManager.pickedPiece && type == PieceType.green)
        {
            carry = !carry;
            GameObject.FindGameObjectWithTag("Carry Label").GetComponent<Text>().text = carry ? "Carry" : "Not Carry";
        }
        greyPieceCount = 0;
        foreach(GameObject friendlyPiece in friendlyPieces)
        {
            if (friendlyPiece.GetComponent<Piece>().type == PieceType.grey)
                greyPieceCount++;
        }
    }

    private void OnMouseDown()
    {
        if (gameManager.piecesPlayed.Count != 0)
            Debug.Log(gameManager.piecesPlayed[0]);
        if (gameManager.turn == team && 
            (!gameManager.piecesPlayed.Contains(type)))
        {
            switch(gameManager.GetGameState())
            {
                case GameManager.GameStates.pickPiece:
                    if (!NextToEnemyWhite())
                        PickPiece();
                    break;
                
                case GameManager.GameStates.chooseMove:
                    UndoMove();
                    break;

                case GameManager.GameStates.placeGrey:
                    UndoMove();
                    break;
            }
        }
        else { Debug.Log("D"); }
    }

    private void PickPiece()
    {
        gameManager.pickedPiece = gameObject;
        switch (type)
        {
            case PieceType.grey:
            case PieceType.purple:
            case PieceType.blue:
            case PieceType.green:
            case PieceType.black:
            case PieceType.red:
            case PieceType.yellow:
            case PieceType.brown:
            case PieceType.white:
                ShowMoves();
                if (onBoardMoves.Count != 0)
                {
                    if (type == PieceType.green)
                        GameObject.FindGameObjectWithTag("Carry Label").GetComponent<Text>().text = carry ? "Carry" : "Not Carry";
                    gameManager.SetGameState(GameManager.GameStates.chooseMove);
                }
                onBoardMoves.Clear();
                greySelectMoves.Clear();
                break;
            case PieceType.orange:
                if (greyPieceCount <= 10)
                {
                    PlaceGrey();
                    if (greyPlacementMoves.Count != 0)
                        gameManager.SetGameState(GameManager.GameStates.placeGrey);
                }
                break;
            default:
                Debug.LogWarning("Missing piece type");
                break;
        }
    }

    private void ShowMoves()
    {

        bool pieceInWay = false;

        switch (type)
        {

            case PieceType.grey:
                for (int hor = -1; hor <= 1; hor++)
                {
                    for (int ver = -1; ver <= 1; ver++)
                    {
                        if (!(hor == 0 && ver == 0))
                        {
                            for (int step = 1; step <= 2; step++)
                            {
                                Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                                foreach (GameObject piece in friendlyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                        if (piece.GetComponent<Piece>().type == PieceType.grey && step == 1
                                            && piece.GetComponent<Piece>().team == team && !piece.GetComponent<Piece>().invincible
                                            && !piece.GetComponent<Piece>().NextToEnemyWhite())
                                            greySelectMoves.Add(checkPos);
                                    }
                                }
                                foreach (GameObject piece in enemyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                        if (!piece.GetComponent<Piece>().invincible)
                                            onBoardMoves.Add(checkPos);
                                    }
                                }
                                if (pieceInWay)
                                {
                                    pieceInWay = false;
                                    break;
                                }
                                bool onBoard = false;
                                foreach (GameObject cell in cells)
                                {
                                    Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                    if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        onBoardMoves.Add(checkPos);
                                        onBoard = true;
                                    }
                                    
                                }
                                if (!onBoard)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }
                foreach (Vector2 greySelectMove in greySelectMoves)
                {
                    Instantiate(greySelectCell, new Vector3(greySelectMove.x, greySelectMove.y, -2), Quaternion.identity);
                }

                //selectedGreyPieces.Add(gameObject);

                break;



            case PieceType.purple:

                for (int hor = -1; hor <= 1; hor++)
                {
                    for (int ver = -1; ver <= 1; ver++)
                    {
                        if (!(hor == 0 && ver == 0))
                        {
                            for (int step = 1; step <= 2; step++)
                            {
                                Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                                foreach (GameObject piece in friendlyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                    }
                                }
                                foreach (GameObject piece in enemyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                        if (!piece.GetComponent<Piece>().invincible)
                                            onBoardMoves.Add(checkPos);
                                    }
                                }
                                if (pieceInWay)
                                {
                                    pieceInWay = false;
                                    break;
                                }
                                bool onBoard = false;
                                foreach (GameObject cell in cells)
                                {
                                    Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                    if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        onBoardMoves.Add(checkPos);
                                        onBoard = true;
                                    }
                                }
                                if (!onBoard)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }

                break;


            
            case PieceType.blue:

                for (int hor = -1; hor <= 1; hor += 2)
                {
                    for (int ver = -1; ver <= 1; ver += 2)
                    {
                        for (int step = 1; step <= 3; step++)
                        {
                            Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                            foreach (GameObject piece in enemyPieces)
                            {
                                Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                {
                                    pieceInWay = true;
                                    if (!piece.GetComponent<Piece>().invincible)
                                        onBoardMoves.Add(checkPos);
                                }
                            }
                            
                            if (pieceInWay)
                            {
                                pieceInWay = false;
                                break;
                            }
                            bool onBoard = false;
                            foreach (GameObject cell in cells)
                            {
                                Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                {
                                    onBoardMoves.Add(checkPos);
                                    onBoard = true;
                                }
                            }
                            if (!onBoard)
                            {
                                break;
                            }
                            foreach (GameObject piece in friendlyPieces)
                            {
                                Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                {
                                    onBoardMoves.Remove(checkPos);
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }

                break;
                


            case PieceType.green:

                int flip = team == "Star" ? 1 : -1;

                for (int hor = -1; hor <= 1; hor++)
                {
                    for (int ver = -1; ver <= 1; ver += 2)
                    {
                        if (hor == 0 && ver == -1 || hor != 0 && ver == 1)
                        {
                            for (int step = 1; step <= 6; step++)
                            {
                                Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step * flip);
                                foreach (GameObject piece in enemyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                    }
                                }
                                if (pieceInWay)
                                {
                                    pieceInWay = false;
                                    continue;
                                }
                                bool onBoard = false;
                                foreach (GameObject cell in cells)
                                {
                                    Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                    if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        onBoardMoves.Add(checkPos);
                                        onBoard = true;
                                    }
                                }
                                if (!onBoard)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }

                break;



            case PieceType.black:

                for (int hor = -1; hor <= 1; hor++)
                {
                    for (int ver = -1; ver <= 1; ver++)
                    {
                        if (hor == 0 ^ ver == 0)
                        {
                            for (int step = 1; step <= 3; step++)
                            {
                                Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                                foreach (GameObject piece in friendlyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                    }
                                }
                                foreach (GameObject piece in enemyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                        if (!piece.GetComponent<Piece>().invincible)
                                            onBoardMoves.Add(checkPos);
                                    }
                                }
                                if (pieceInWay)
                                {
                                    pieceInWay = false;
                                    break;
                                }
                                foreach (GameObject cross in crosses)
                                {
                                    Bounds crossBounds = cross.GetComponent<Collider2D>().bounds;
                                    if (crossBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        foreach (GameObject adjacentCross in cross.GetComponent<Cross>().adjacentCrosses)
                                        {
                                            TeleportBlack(adjacentCross.transform.position, new Vector2(hor, ver), 3-step);
                                        }
                                    }
                                }
                                foreach (GameObject cell in cells)
                                {
                                    Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                    if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        if (!onBoardMoves.Contains(checkPos))
                                            onBoardMoves.Add(checkPos);
                                    }
                                }
                            }
                        }
                    }
                }
                
                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }

                break;



            case PieceType.red:

                for (int hor = -1; hor <= 1; hor++)
                {
                    for (int ver = -1; ver <= 1; ver++)
                    {
                        if (!(hor == 0 && ver == 0))
                        {
                            for (int step = 1; step <= 1; step++)
                            {
                                Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                                foreach (GameObject piece in friendlyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                    }
                                }
                                foreach (GameObject piece in enemyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                        if (!piece.GetComponent<Piece>().invincible)
                                            onBoardMoves.Add(checkPos);
                                    }
                                }
                                if (pieceInWay)
                                {
                                    pieceInWay = false;
                                    break;
                                }
                                foreach (GameObject cell in cells)
                                {
                                    Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                    if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        onBoardMoves.Add(checkPos);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }

                break;

            case PieceType.yellow:

                for (int hor = -1; hor <= 1; hor++)
                {
                    for (int ver = -1; ver <= 1; ver++)
                    {
                        if (hor == 0 ^ ver == 0)
                        {
                            int steps;
                            if (hor == 0)
                                steps = 5;
                            else
                                steps = 1;

                            for (int step = 1; step <= steps; step++)
                            {
                                Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                                foreach (GameObject piece in friendlyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                    }
                                }
                                foreach (GameObject piece in enemyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                        if (!piece.GetComponent<Piece>().invincible)
                                            onBoardMoves.Add(checkPos);
                                    }
                                }
                                if (pieceInWay)
                                {
                                    pieceInWay = false;
                                    break;
                                }
                                foreach (GameObject cell in cells)
                                {
                                    Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                    if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        onBoardMoves.Add(checkPos);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }

                break;



            case PieceType.brown:

                for (int hor = -1; hor <= 1; hor++)
                {
                    for (int ver = -1; ver <= 1; ver++)
                    {
                        if (hor == 0 ^ ver == 0)
                        {
                            int steps;
                            if (hor == 0)
                                steps = 1;
                            else
                                steps = 5;

                            for (int step = 1; step <= steps; step++)
                            {
                                Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                                foreach (GameObject piece in friendlyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                        onBoardMoves.Add(checkPos);
                                    }
                                }
                                foreach (GameObject piece in enemyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                        if (!piece.GetComponent<Piece>().invincible)
                                            onBoardMoves.Add(checkPos);
                                    }
                                }
                                if (pieceInWay)
                                {
                                    pieceInWay = false;
                                    break;
                                }
                                foreach (GameObject cell in cells)
                                {
                                    Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                    if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        onBoardMoves.Add(checkPos);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }

                break;



            case PieceType.white:

                for (int hor = -1; hor <= 1; hor++)
                {
                    for (int ver = -1; ver <= 1; ver++)
                    {
                        if (!(hor == 0 && ver == 0))
                        {
                            for (int step = 1; step <= 4; step++)
                            {
                                Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                                foreach (GameObject piece in friendlyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                    }
                                }
                                foreach (GameObject piece in enemyPieces)
                                {
                                    Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                                    if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                                    {
                                        pieceInWay = true;
                                    }
                                }
                                if (pieceInWay)
                                {
                                    pieceInWay = false;
                                    break;
                                }
                                bool onBoard = false;
                                foreach (GameObject cell in cells)
                                {
                                    Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                                    if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                                    {
                                        if (step != 1 && onBoardMoves.Count != 0)
                                            onBoardMoves.RemoveAt(onBoardMoves.Count - 1);
                                        onBoardMoves.Add(checkPos);
                                        onBoard = true;
                                    }
                                }
                                if (!onBoard)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 move in onBoardMoves)
                {
                    Instantiate(openCell, new Vector3(move.x, move.y, -2), Quaternion.identity);
                }

                break;



            default:
                Debug.LogWarning("Missing piece, should not display");
                break;

        }
    }


    private void TeleportBlack(Vector3 crossPos, Vector2 dir, int stepsLeft)
    {
        bool pieceInWay = false;
        for (int step = 0; step <= stepsLeft; step++)
        {
            Vector2 checkPos = new Vector2(crossPos.x + dir.x * step, crossPos.y + dir.y * step);
            foreach (GameObject piece in friendlyPieces)
            {
                Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                {
                    pieceInWay = true;
                }
            }
            foreach (GameObject piece in enemyPieces)
            {
                Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                {
                    pieceInWay = true;
                    if (step != 0)
                    {
                        if (!piece.GetComponent<Piece>().invincible)
                            onBoardMoves.Add(checkPos);
                    }
                       
                }
            }
            if (pieceInWay)
            {
                pieceInWay = false;
                break;
            }
            foreach (GameObject cell in cells)
            {
                Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                {
                    if (!onBoardMoves.Contains(checkPos) && step != 0)
                        onBoardMoves.Add(checkPos);
                }
            }
        }
    }

    public bool MoveGroupGrey(Vector2 dir, int maxStep)
    {
        for (int step = 1; step <= maxStep; step++)
        {
            bool cantMove = true;
            Vector2 checkPos = new Vector2(pos.x + dir.x * step, pos.y + dir.y * step);
            foreach (GameObject cell in cells)
            {
                Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                {
                    cantMove = false;
                }
            }
            foreach (GameObject piece in friendlyPieces)
            {
                Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                {
                    if (!gameManager.pickedPiece.GetComponent<Piece>().selectedGreyPieces.Contains(piece) 
                        && piece != gameManager.pickedPiece)
                    {
                        cantMove = true;
                    }
                }
            }
            foreach (GameObject piece in enemyPieces)
            {
                Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                {
                    if (step != maxStep)
                    {
                        cantMove = true;
                    }
                }
            }
            if (cantMove)
            {
                return false;
            }
        }

        return true;
    }

    private void UndoMove()
    {
        if (gameObject == gameManager.pickedPiece)
        {
            foreach (GameObject openCell in GameObject.FindGameObjectsWithTag("Open Cell"))
            {
                Destroy(openCell);
            }
            foreach (GameObject greySelectedCells in GameObject.FindGameObjectsWithTag("Select Grey Cell"))
            {
                Destroy(greySelectedCells);
            }
            foreach (GameObject greyPlacementCell in GameObject.FindGameObjectsWithTag("Grey Placement Cell"))
            {
                Destroy(greyPlacementCell);
            }
            selectedGreyPieces.Clear();
            gameManager.SetGameState(GameManager.GameStates.pickPiece);
        }
    }

    private void OnDestroy()
    {
        if (type == PieceType.purple)
        {
            foreach (GameObject enemyPiece in enemyPieces)
            {
                if (enemyPiece != null)
                {
                    if ((Vector2) enemyPiece.transform.position == (Vector2) transform.position)
                    {
                        Destroy(enemyPiece);
                        foreach (GameObject pieces in enemyPieces)
                        {
                            pieces.GetComponent<Piece>().friendlyPieces.Remove(enemyPiece);
                        }
                        foreach (GameObject pieces in friendlyPieces)
                        {
                            pieces.GetComponent<Piece>().enemyPieces.Remove(enemyPiece);
                        }
                    }
                }
            }
        }
    }

    public void AOE(int step)
    {
        for (int hor = -1; hor <= 1; hor++)
        {
            for (int ver = -1; ver <= 1; ver++)
            {
                for (int s = 1; s <= step; s++)
                {
                    Vector2 checkPosition = new Vector2(transform.position.x + hor * s, transform.position.y + ver * s);
                    for (int i = 0; i < enemyPieces.Count; i++)
                    {
                        GameObject enemyPiece = enemyPieces[i];
                        if ((Vector2) enemyPiece.transform.position == checkPosition)
                        {
                            if (type == PieceType.green)
                                enemyPiece.GetComponent<Piece>().MovePiece(hor, ver);
                            if (type == PieceType.red)
                            {
                                GameObject takenPiece = null;
                                for (int j = 0; j < enemyPieces.Count; j++)
                                {
                                    if (checkPosition == (Vector2) enemyPieces[j].transform.position 
                                        && (checkPosition - (Vector2) transform.position).magnitude <= 2.1
                                        && enemyPieces[j].GetComponent<Piece>().type != PieceType.orange
                                        && !enemyPieces[j].GetComponent<Piece>().invincible)
                                    {
                                        takenPiece = enemyPieces[j];
                                        Destroy(takenPiece);
                                        foreach (GameObject pieces in enemyPieces)
                                        {
                                            pieces.GetComponent<Piece>().friendlyPieces.Remove(enemyPieces[j]);
                                        }
                                        foreach (GameObject pieces in friendlyPieces)
                                        {
                                            if (pieces != gameObject)
                                                pieces.GetComponent<Piece>().enemyPieces.Remove(enemyPieces[j]);
                                        }
                                        if (takenPiece != null)
                                            enemyPieces.Remove(takenPiece);
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
        }
    }

    public void MovePiece(int hor, int ver)
    {
        if (type == PieceType.orange)
            return;

        Vector2 checkPos = new Vector2(transform.position.x + hor, transform.position.y + ver);
        foreach (GameObject piece in friendlyPieces)
        {
            Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
            if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
            {
                if (!pieceCanMoveOnFriendly)
                    return;
            }
        }
        foreach (GameObject piece in enemyPieces)
        {
            Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
            if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
            {
                if (pieceCantTake)
                    return;
            }
        }
        bool onBoard = false;
        foreach (GameObject cell in cells)
        {
            Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
            if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
            {
                onBoard = true;
                break;
            }
        }
        if (!onBoard)
            return;

        transform.position = new Vector3(checkPos.x, checkPos.y, z);

        GameObject takenPiece = null;

        for (int i = 0; i < enemyPieces.Count; i++)
        {
            if ((Vector2) transform.position == (Vector2) enemyPieces[i].transform.position)
            {
                takenPiece = enemyPieces[i];
                Destroy(takenPiece);
                foreach (GameObject pieces in enemyPieces)
                {
                    pieces.GetComponent<Piece>().friendlyPieces.Remove(enemyPieces[i]);
                }
                foreach (GameObject pieces in friendlyPieces)
                {
                    if (pieces != gameObject)
                        pieces.GetComponent<Piece>().enemyPieces.Remove(enemyPieces[i]);
                }
                if (takenPiece != null)
                    enemyPieces.Remove(takenPiece);
            }
        }
    }

    public void MoveYellowReverse()
    {
        bool stopPiece = false;
        for (int step = 1; step <= 7; step++)
        {
            Vector2 checkPos = new Vector2(transform.position.x + direction.x *  -1, transform.position.y + direction.y * -1);
            Debug.Log(checkPos);
            foreach (GameObject piece in friendlyPieces)
            {
                Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                {
                    stopPiece = true;
                }
            }
            foreach (GameObject piece in enemyPieces)
            {
                Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                if (pieceBounds.Contains(new Vector3(transform.position.x, transform.position.y, piece.GetComponent<Piece>().z)))
                {
                    stopPiece = true;
                }
            }
            bool onBoard = false;
            foreach (GameObject cell in cells)
            {
                Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                {
                    onBoard = true;
                }
            }
            if (!onBoard)
                stopPiece = true;

            if (stopPiece)
            {
                break;
            }
            transform.position = new Vector3(checkPos.x, checkPos.y, z);
        }


        GameObject takenPiece = null;

        for (int i = 0; i < enemyPieces.Count; i++)
        {
            if ((Vector2)transform.position == (Vector2)enemyPieces[i].transform.position)
            {
                takenPiece = enemyPieces[i];
                Destroy(takenPiece);
                foreach (GameObject pieces in enemyPieces)
                {
                    pieces.GetComponent<Piece>().friendlyPieces.Remove(enemyPieces[i]);
                }
                foreach (GameObject pieces in friendlyPieces)
                {
                    if (pieces != gameObject)
                        pieces.GetComponent<Piece>().enemyPieces.Remove(enemyPieces[i]);
                }
                if (takenPiece != null)
                    enemyPieces.Remove(takenPiece);
            }
        }
    }

    public bool NextToEnemyWhite()
    {
        foreach(GameObject enemyPiece in enemyPieces)
        {
            Piece piece = enemyPiece.GetComponent<Piece>();
            if (piece.type == PieceType.white 
                && Vector2.Distance(transform.position, enemyPiece.transform.position) < 2)
            {
                return true;
            }

            bool onCross = false;
            bool enemyOnCross = false;
            foreach (GameObject cross in crosses)
            {
                if ((Vector2) cross.transform.position == (Vector2) piece.transform.position)
                {
                    enemyOnCross = true;
                }
                if ((Vector2) cross.transform.position == (Vector2) transform.position)
                {
                    onCross = true;
                }
            }

            if (piece.type == PieceType.white && onCross && enemyOnCross)
            {
                return true;
            }
        }
        return false;
    }

    public void PlaceGrey()
    {
        bool pieceInWay = false;
        for (int hor = -1; hor <= 1; hor++)
        {
            for (int ver = -1; ver <= 1; ver++)
            {
                if (!(hor == 0 && ver == 0))
                {
                    for (int step = 1; step <= 1; step++)
                    {
                        Vector2 checkPos = new Vector2(pos.x + hor * step, pos.y + ver * step);
                        foreach (GameObject piece in friendlyPieces)
                        {
                            Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                            if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                            {
                                pieceInWay = true;
                            }
                        }
                        foreach (GameObject piece in enemyPieces)
                        {
                            Bounds pieceBounds = piece.GetComponent<Collider2D>().bounds;
                            if (pieceBounds.Contains(new Vector3(checkPos.x, checkPos.y, piece.GetComponent<Piece>().z)))
                            {
                                pieceInWay = true;
                            }
                        }
                        if (pieceInWay)
                        {
                            pieceInWay = false;
                            break;
                        }
                        foreach (GameObject cell in cells)
                        {
                            Bounds cellBounds = cell.GetComponent<Collider2D>().bounds;
                            if (cellBounds.Contains(new Vector3(checkPos.x, checkPos.y, 0)))
                            {
                                greyPlacementMoves.Add(checkPos);
                            }
                        }
                    }
                }
            }
        }

        foreach (Vector2 move in greyPlacementMoves)
        {
            Instantiate(Resources.Load<Object>("Grey Placement Cell"), new Vector3(move.x, move.y, -2), Quaternion.identity);
        }
    }
}
