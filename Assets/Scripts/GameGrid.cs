using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameGrid : MonoBehaviour
{
    public Tile errorTile;
    public Tile[] baseCubes;
    public Tile currentTile;
    //public static GameGrid instance;
    //[HideInInspector]
    public int[] rowsFill = new int[39];
    //[HideInInspector]
    public int[] colsHeight = new int[21];

    public void InitGameGrid()
    {
        for (int i = 0; i < rowsFill.Length; i++)
        {
            rowsFill[i] = 0;
        }
        for (int i = 0; i < colsHeight.Length; i++)
        {
            colsHeight[i] = 0;
        }
        Debug.Log("Initialized The Grid");
    }
    public void GameOver()
    {
        Debug.Log("You have Lost");
        SceneManager.LoadScene("Level0");
    }

    public void PickNewColor()
    {
        currentTile = baseCubes[Random.Range(0, baseCubes.Length)];
    }
}
