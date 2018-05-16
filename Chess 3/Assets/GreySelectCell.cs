using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreySelectCell : MonoBehaviour {


    private GameManager gameManager;

    [SerializeField]
    private GameObject selectGreyCell;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
    }

    private void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Grey Selected Cell");

        List<GameObject> greyObjects = new List<GameObject>();
        foreach(GameObject piece in GameObject.FindGameObjectsWithTag("Piece"))
        {
            if (piece.GetComponent<Piece>().type == Piece.PieceType.grey && !greyObjects.Contains(piece))
            {
                greyObjects.Add(piece);
            }
        }

        foreach (GameObject grey in greyObjects)
        {
            if ((Vector2) grey.transform.position == (Vector2) transform.position 
                && !gameManager.pickedPiece.GetComponent<Piece>().selectedGreyPieces.Contains(grey))
            {
                gameManager.pickedPiece.GetComponent<Piece>().selectedGreyPieces.Add(grey);
            }
        }
        for (int hor = -1; hor <= 1; hor++)
        {
            for (int ver = -1; ver <= 1; ver++)
            {
                foreach (GameObject grey in greyObjects)
                {
                    Vector3 nearPosition = new Vector3(transform.position.x + hor, transform.position.y + ver, grey.GetComponent<Piece>().z);
                    if (grey.transform.position == nearPosition && grey != gameManager.pickedPiece
                        && grey.GetComponent<Piece>().team == gameManager.pickedPiece.GetComponent<Piece>().team
                        && !grey.GetComponent<Piece>().NextToEnemyWhite())
                    {
                        bool alreadyCreated = false;
                        foreach (GameObject selectGreyCells in GameObject.FindGameObjectsWithTag("Select Grey Cell"))
                        {
                            if (selectGreyCells.transform.position == new Vector3(nearPosition.x, nearPosition.y, -2))
                            {
                                alreadyCreated = true;
                            }
                        }
                        if (alreadyCreated)
                        {
                            continue;
                        }
                        Instantiate(Resources.Load<Object>("Grey Select Cell"), new Vector3(nearPosition.x, nearPosition.y, -2), Quaternion.identity);
                        
                    }
                }
            }
        }
    }
}
