using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCell : MonoBehaviour {

    private GameManager gameManager;

	// Use this for initialization
	void Start () {
        gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
	}

    private void OnMouseDown()
    {
        switch (gameManager.GetGameState())
        {
            case GameManager.GameStates.chooseMove:
                MovePiece();
                DestroyExtraCells();
                TakePiece();
                Debug.Log(gameManager.moveCount);
                if (gameManager.moveCount >= 3
                    && !(gameManager.pickedPiece.GetComponent<Piece>().type == Piece.PieceType.blue &&
                         gameManager.pickedPiece.GetComponent<Piece>().killCount == 1))
                {
                    foreach (GameObject redPiece in GameObject.FindGameObjectsWithTag("Piece"))
                    {
                        if (redPiece.GetComponent<Piece>().type == Piece.PieceType.red
                            && redPiece.GetComponent<Piece>().team == gameManager.turn)
                        {
                            redPiece.GetComponent<Piece>().AOE(1);
                        }
                    }
                    gameManager.turn = gameManager.turn == "X" ? "Star" : "X";
                    gameManager.moveCount = 0;
                    gameManager.piecesPlayed.Clear();
                    foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece"))
                    {
                        if (piece.GetComponent<Piece>().type == Piece.PieceType.red
                            && piece.GetComponent<Piece>().team == gameManager.turn)
                        {
                            piece.GetComponent<Piece>().AOE(2);
                        }
                        if (gameManager.turn == piece.GetComponent<Piece>().team)
                        {
                            piece.GetComponent<Piece>().killCount = 0;
                            if (piece.GetComponent<Piece>().type == Piece.PieceType.blue)
                                piece.GetComponent<Piece>().invincible = false;
                        }
                    }
                }
                break;
        }
    }

    private void MovePiece()
    {
        Piece piece = gameManager.pickedPiece.GetComponent<Piece>();
        if (piece.type == Piece.PieceType.grey)
        {
            MoveGreyPieces(piece);
        }
        else
        {
            if (piece.type == Piece.PieceType.green && piece.carry)
            {
                foreach (GameObject friendlyPiece in piece.friendlyPieces)
                {
                    if (friendlyPiece != piece.gameObject && (Vector2) friendlyPiece.transform.position == (Vector2) piece.transform.position)
                    {
                        friendlyPiece.transform.position = new Vector3(transform.position.x, transform.position.y, friendlyPiece.GetComponent<Piece>().z);
                        break;
                    }
                }
            }
            piece.direction = new Vector2(System.Math.Sign(transform.position.x - piece.transform.position.x),
                                          System.Math.Sign(transform.position.y - piece.transform.position.y));
            piece.transform.position = new Vector3(transform.position.x, transform.position.y, piece.z);
            if (piece.type == Piece.PieceType.green)
            {
                piece.AOE(1);
            }
            if (piece.type == Piece.PieceType.brown)
            {
                bool onPiece = false;
                foreach(GameObject friendlyPiece in piece.friendlyPieces)
                {
                    if ((Vector2) piece.transform.position == (Vector2) friendlyPiece.transform.position
                        && piece.gameObject != friendlyPiece)
                    {
                        onPiece = true;
                        friendlyPiece.GetComponent<Piece>().invincible = true;
                        piece.z = -1.75f;
                        piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, piece.z);
                    }
                    else
                    {
                        if (friendlyPiece.GetComponent<Piece>().killCount != 2)
                            friendlyPiece.GetComponent<Piece>().invincible = false;
                    }
                }
                piece.invincible = onPiece;
                if (!piece.invincible)
                {
                    piece.z = -1f;
                    piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, piece.z);
                }
                Debug.Log(piece.invincible);
            }
            if (!(piece.type == Piece.PieceType.blue && piece.killCount != 0))
                gameManager.moveCount++;
        }
    }

    private void TakePiece()
    {
        Piece piece = gameManager.pickedPiece.GetComponent<Piece>();
        if (piece.type != Piece.PieceType.grey)
            gameManager.piecesPlayed.Add(piece.type);

        GameObject takenPiece = null;
        for (int i = 0; i < piece.enemyPieces.Count; i++)
        {
            if ((Vector2) piece.transform.position == (Vector2) piece.enemyPieces[i].transform.position)
            {
                takenPiece = piece.enemyPieces[i];
                takenPiece.tag = "Untagged";
                Destroy(takenPiece);
                if (piece.type == Piece.PieceType.blue && piece.killCount <= 1)
                {
                    if (piece.killCount < 1)
                        gameManager.piecesPlayed.Remove(piece.type);
                    piece.killCount++;
                    if (piece.killCount == 2)
                    {
                        piece.invincible = true;
                    }
                    Debug.Log(piece.killCount);
                }
                foreach (GameObject pieces in piece.enemyPieces)
                {
                    pieces.GetComponent<Piece>().friendlyPieces.Remove(piece.enemyPieces[i]);
                }
                foreach (GameObject pieces in piece.friendlyPieces)
                {
                    if (pieces != piece.gameObject)
                        pieces.GetComponent<Piece>().enemyPieces.Remove(piece.enemyPieces[i]);
                }
                if (takenPiece != null)
                    piece.enemyPieces.Remove(takenPiece);

                if (piece.type == Piece.PieceType.yellow)
                {
                    piece.MoveYellowReverse();
                }
            }
        }
        if (piece.type == Piece.PieceType.grey)
        {
            foreach (GameObject grey in piece.selectedGreyPieces)
            {
                Piece greyPiece = grey.GetComponent<Piece>();
                for (int i = 0; i < greyPiece.enemyPieces.Count; i++)
                {
                    if ((Vector2) greyPiece.transform.position == (Vector2) greyPiece.enemyPieces[i].transform.position)
                    {
                        takenPiece = greyPiece.enemyPieces[i];
                        Destroy(takenPiece);
                        foreach (GameObject pieces in greyPiece.enemyPieces)
                        {
                            pieces.GetComponent<Piece>().friendlyPieces.Remove(greyPiece.enemyPieces[i]);
                        }
                        foreach (GameObject pieces in greyPiece.friendlyPieces)
                        {
                            if (pieces != greyPiece.gameObject)
                                pieces.GetComponent<Piece>().enemyPieces.Remove(greyPiece.enemyPieces[i]);
                        }
                        if (takenPiece != null)
                            greyPiece.enemyPieces.Remove(takenPiece);
                    }
                }
            }
        }
        piece.selectedGreyPieces.Clear();
    }

    private void MoveGreyPieces(Piece piece)
    {
        bool allGreysCanMove = true;
        foreach (GameObject selectedGrey in piece.selectedGreyPieces)
        {
            Vector2 dir = new Vector3(System.Math.Sign(transform.position.x - piece.transform.position.x),
                                      System.Math.Sign(transform.position.y - piece.transform.position.y));
            int step = (int)Mathf.Max(Mathf.Abs(transform.position.x - piece.transform.position.x),
                                       Mathf.Abs(transform.position.y - piece.transform.position.y));

            if (!selectedGrey.GetComponent<Piece>().MoveGroupGrey(dir, step))
            {
                allGreysCanMove = false;
                break;
            }
        }

        if (allGreysCanMove)
        {
            foreach (GameObject selectedGrey in piece.selectedGreyPieces)
            {
                Vector3 diff = new Vector3(transform.position.x - piece.transform.position.x,
                                            transform.position.y - piece.transform.position.y,
                                            0f);
                selectedGrey.transform.position = selectedGrey.transform.position + diff;
            }
            piece.transform.position = new Vector3(transform.position.x, transform.position.y, piece.z);
            gameManager.moveCount++;
            gameManager.piecesPlayed.Add(piece.type);
        }
    }

    private void DestroyExtraCells()
    {
        foreach (GameObject openCell in GameObject.FindGameObjectsWithTag("Open Cell"))
        {
            Destroy(openCell);
        }
        foreach (GameObject selectGreyCell in GameObject.FindGameObjectsWithTag("Select Grey Cell"))
        {
            Destroy(selectGreyCell);
        }
        gameManager.SetGameState(GameManager.GameStates.pickPiece);
    }

}
