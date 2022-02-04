using UnityEngine;
using UnityEngine.Tilemaps;


public class PositionOnGrid : MonoBehaviour
{
    public Sounds sounds;
    public Effects effects;
    public GameGrid instance;
    public Tilemap gameGrid;
    Vector2 reference = new Vector2(-9.5f, -21.5f);
    //[HideInInspector]
    public int i , j ;
    public int spawnI;
    public int spawnJ;
    void Awake()
    {
        instance.InitGameGrid();
        //i = spawnI;
        //j = spawnJ;
    }
 
    void Update()
    {
        //i = Mathf.Clamp(i, 0, 38);
        //j = Mathf.Clamp(j, 0, 20);
        //Placed();
    }

    public void OnPlaced()
    {
        if (instance.colsHeight[j] >= 36) instance.GameOver();
        gameGrid.SetTile(gameGrid.WorldToCell(gameObject.transform.position), instance.currentTile ) ;
        if (instance.colsHeight[j] < i + 1)
        {
            FillHoles();
            instance.colsHeight[j] = i + 1;
        }
        instance.rowsFill[i] += 1;
      
    }

    public void FillHoles()
    {
        bool filledHoles = false;
        if (instance.colsHeight[j] < i + 1)
        {
            for (int _i = i - 1; _i >= instance.colsHeight[j]; _i--)
            {
                gameGrid.SetTile(gameGrid.WorldToCell(gameObject.transform.position - new Vector3(0, -_i + i, 0)), instance.errorTile);
                filledHoles = true;
            }
            if(filledHoles) sounds.PlayError();
        }
    }


    public bool IsPlaced()
    {
        return i <= instance.colsHeight[j];
    }

    public bool RowFilled()
    {
        //Debug.Log(instance.rowsFill[i]);
        return instance.rowsFill[i] >= 21;
    }
    void Placed()
    {
        if (i==instance.colsHeight[j])
        {
            OnPlaced();
        }
      
    }

    public void OnRowFilled(int filledLine)
    {
        effects.PlayAtPosition(transform.position.y);
        sounds.PlayRowFill();
        int maxHeight = -1;
        
        for (int _j = 0; _j <= 20; _j++)
        {
            for (int _i = filledLine; _i < instance.colsHeight[_j]; _i++)
            {
                Vector3Int curPos = gameGrid.WorldToCell(reference + new Vector2(_j, _i));
                TileBase tileAbove = gameGrid.GetTile(curPos + new Vector3Int(0,1,0));
                gameGrid.SetTile(curPos, tileAbove) ;
                if (_i > maxHeight)
                {
                    instance.rowsFill[_i] = instance.rowsFill[_i + 1];
                    maxHeight = _i;
                }
                
            }
            instance.colsHeight[_j] = Mathf.Clamp(instance.colsHeight[_j]-1,0,39);

        }

    }
    public void Fall() 
    {
        if (i > 0)
        {
            i--;
            UpdatePosition();
            
        }
    }

    public void UpdatePosition()
    {
        transform.position = reference + new Vector2(j, i);
    }

    void moveToLeft()
    {
        //transform.position += 0;
    }

}
