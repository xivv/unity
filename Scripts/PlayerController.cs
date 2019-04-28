using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    public Tilemap groundTilemap;           
    public Tilemap wallTilemap;
    public Tilemap encounterTilemap;

    public int tilesize;

    public float moveTime = 0.1f;
    public float movementDelay = 0.1f;
    private bool isMoving;

    // Update is called once per frame
    void Update()
    {


        if (isMoving) return;

        //To store move directions.
        int horizontal = 0;
        int vertical = 0;
        //To get move directions
        horizontal = (int)(Input.GetAxisRaw("Horizontal")) * tilesize;
        vertical = (int)(Input.GetAxisRaw("Vertical")) * tilesize;



        //We can't go in both directions at the same time
        if (horizontal != 0)
        {
            vertical = 0;
        }

        //If there's a direction, we are trying to move.
        if (horizontal != 0 || vertical != 0)
        {
            StartCoroutine(Move(horizontal, vertical));
        }
    }

    private IEnumerator Move(int xDir, int yDir)
    {
        isMoving = true;

        Vector2 startCell = transform.position;
        Vector2 targetCell = startCell + new Vector2(xDir, yDir);

        bool isOnGround = getCell(groundTilemap, startCell) != null; //If the player is on the ground
        bool hasGroundTile = getCell(groundTilemap, targetCell) != null; //If target Tile has a ground
        bool hasObstacleTile = getCell(wallTilemap, targetCell) != null; //if target Tile has an obstacle
        bool hasEncounterTile = getCell(encounterTilemap, targetCell) != null; //if target Tile is an encounter

        //If the player starts their movement from a ground tile.
        if (isOnGround)
        {

            //If the front tile is a walkable ground tile, the player moves here.
            if (hasGroundTile && !hasObstacleTile)
            {
                float sqrRemainingDistance = (transform.position - (Vector3)targetCell).sqrMagnitude;
                while (sqrRemainingDistance > float.Epsilon)
                {
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, targetCell, (1 / moveTime) * Time.deltaTime);
                    transform.position = newPosition;
                    sqrRemainingDistance = (transform.position - (Vector3)targetCell).sqrMagnitude;
                    yield return null;
                }

            }

            if (hasEncounterTile)
            {         
                SceneManager.LoadScene("BattleScene");
            }
            
            Invoke("resetMovement", movementDelay);

        }
    }

    private void resetMovement()
    {
        this.isMoving = false;
    }

    private TileBase getCell(Tilemap tilemap, Vector2 cellWorldPos)
    {
        return tilemap.GetTile(tilemap.WorldToCell(cellWorldPos));
    }
}
