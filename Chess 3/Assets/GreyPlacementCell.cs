using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreyPlacementCell : MonoBehaviour {

    public GameObject greyPiece;

    private GameManager gameManager;
	// Use this for initialization
	void Start () {
        gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}



    private void OnMouseDown()
    {
        Piece piece = gameManager.pickedPiece.GetComponent<Piece>();
        gameManager.piecesPlayed.Add(piece.type);

        Piece greyPieceNew = Instantiate(greyPiece, new Vector3(transform.position.x, transform.position.y, -1), Quaternion.identity).GetComponent<Piece>();
        if (gameManager.turn == "Star")
        {
            greyPieceNew.name = "Star Grey";
            greyPieceNew.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Stars/spr_StarPieceGray_0");
            greyPieceNew.team = "Star";
            
        }
        if (gameManager.turn == "X")
        {
            greyPieceNew.name = "X Grey";
            greyPieceNew.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Xs/spr_XPieceGray_0");
            greyPieceNew.team = "X";
        }
        foreach (GameObject pieces in piece.pieces)
        {
            if (greyPieceNew.team == pieces.GetComponent<Piece>().team)
                pieces.GetComponent<Piece>().friendlyPieces.Add(greyPieceNew.gameObject);
            else
                pieces.GetComponent<Piece>().enemyPieces.Add(greyPieceNew.gameObject);
        }

        foreach (GameObject placementCell in GameObject.FindGameObjectsWithTag("Grey Placement Cell"))
        {
            Destroy(placementCell);
        }
        gameManager.moveCount++;
        gameManager.SetGameState(GameManager.GameStates.pickPiece);
    }
}
