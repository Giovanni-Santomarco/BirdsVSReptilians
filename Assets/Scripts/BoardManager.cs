using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    //the following class' objects store data that refers to a tile, boardData stores data for each tile
    public class CellData
    {
        public bool isPassable;
    }
    private CellData[,] boardData;
    private Tilemap m_Tilemap;
    //public properties, those are controlled in Unity interface
    public int Width;
    public int Height;
    public Tile SideTile;
    public Tile[] GroundTiles;

    // Start is called before the first frame update
    void Start()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();

        boardData = new CellData[Width, Height];

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                boardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = SideTile;
                    boardData[x, y].isPassable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    boardData[x, y].isPassable = true;
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}
