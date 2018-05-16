using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    
    public enum GameStates { pickPiece, chooseMove, placeGrey }

    public GameObject pickedPiece = null;
    public string turn;
    public int moveCount;
    public List<Piece.PieceType> piecesPlayed;

    private GameStates gameState;

	// Use this for initialization
	void Start () {
        gameState = GameStates.pickPiece;
        turn = "Star";
        moveCount = 0;
	}
	
    public void SetGameState (GameStates state)
    {
        gameState = state;
    }

    public GameStates GetGameState()
    {
        return gameState;
    }
}
