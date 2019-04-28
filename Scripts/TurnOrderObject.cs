using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TurnOrderObject : MonoBehaviour
{

    private static float moveTime = 0.1f;
    private static float movementDelay = 0.1f;
    protected bool isMoving;

    /** Tilemaps **/
    protected Tilemap groundTilemap;
    protected Tilemap wallTilemap;
    protected Tilemap encounterTilemap;
    protected LayerMask unitLayer;

    private BoxCollider2D boxCollider;

    private static int tilesize = 1;
    public int remainingMovementSpeed;
    public bool limitedMovement = true;

    // If its the turn of the unit
    public bool canAct = false;
    // If we want to block movement cause of animation or target selection
    public bool pausedMovement = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        this.boxCollider = GetComponent<BoxCollider2D>();
        this.groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        this.encounterTilemap = GameObject.Find("Encounter").GetComponent<Tilemap>();
        this.wallTilemap = GameObject.Find("Wall").GetComponent<Tilemap>();
        this.unitLayer = LayerMask.GetMask("UnitsGround");
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isMoving || !canAct || pausedMovement) return;

        //To store move directions.
        int horizontal = 0;
        int vertical = 0;
        //To get move directionsy
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

    public virtual void BeforeTurn()
    {
        this.canAct = true;
        this.pausedMovement = false;
    }

    protected RaycastHit2D rayCastToUnit(Vector2 end)
    {
        Vector2 start = transform.position;
        boxCollider.enabled = false;
        return Physics2D.Linecast(start, end, unitLayer);
    }

    protected bool hitsUnit(Vector2 end)
    {
        bool hit = rayCastToUnit(end).transform != null;
        boxCollider.enabled = true;
        return hit;
    }

    protected virtual bool allowMovement(Vector2 targetCell)
    {
        bool hasGroundTile = getCell(groundTilemap, targetCell) != null; //If target Tile has a ground
        bool hasObstacleTile = getCell(wallTilemap, targetCell) != null; //if target Tile has an obstacle
        return hasGroundTile && !hasObstacleTile;
    }

    private IEnumerator Move(int xDir, int yDir)
    {
        isMoving = true;

        Vector2 startCell = transform.position;
        Vector2 targetCell = startCell + new Vector2(xDir, yDir);

        Debug.DrawLine(startCell, targetCell, Color.red, 5f);

        bool isOnGround = getCell(groundTilemap, startCell) != null; //If the player is on the ground

        bool hasEncounterTile = getCell(encounterTilemap, targetCell) != null; //if target Tile is an encounter


        //If the player starts their movement from a ground tile.
        if (isOnGround)
        {

            //If the front tile is a walkable ground tile, the player moves here.
            if (allowMovement(targetCell))
            {

                float sqrRemainingDistance = (transform.position - (Vector3)targetCell).sqrMagnitude;
                while (sqrRemainingDistance > float.Epsilon)
                {

                    Vector3 newPosition = Vector3.MoveTowards(transform.position, targetCell, (1 / moveTime) * Time.deltaTime);
                    transform.position = newPosition;
                    sqrRemainingDistance = (transform.position - (Vector3)targetCell).sqrMagnitude;
                    yield return null;
                }

                this.remainingMovementSpeed--;
            }

            if (hasEncounterTile)
            {
                this.canAct = false;
            }

            Invoke("resetMovement", movementDelay);

        }
    }

    protected void resetMovement()
    {
        this.isMoving = false;

        if (limitedMovement && this.remainingMovementSpeed <= 0)
        {
            this.canAct = false;
        }
    }

    protected TileBase getCell(Tilemap tilemap, Vector2 cellWorldPos)
    {
        return tilemap.GetTile(tilemap.WorldToCell(cellWorldPos));
    }
}
