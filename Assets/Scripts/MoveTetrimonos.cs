using System.Collections;
using UnityEngine;

public class MoveTetrimonos : MonoBehaviour
{

    public Sounds sounds;
    public PositionOnGrid[] monos;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Rotate(monos[1],monos[0].i,monos[0].j);
            RotateTetrimonos();
        }
    }


    //A rotation of 90 degres around monos[0]
    void Rotate(PositionOnGrid mono,int i0, int j0)
    {
        int deltaI = mono.i - i0;
        int deltaJ = mono.j - j0;
        mono.i = i0 + deltaJ;
        mono.j = j0 - deltaI;
      

        //Debug.Log("setTo" + mono.i + "    " + mono.j + "    " + deltaI  + "and   " +deltaJ);
        mono.UpdatePosition();
        
    }

    bool RotationImpossible()
    {
        bool impossible = false;
        int targetI, targetJ;
        int deltaI, deltaJ;
        int i0 = monos[0].i, j0 = monos[0].j;
        foreach (var item in monos)
        {
            deltaI = item.i - i0;
            deltaJ = item.j - j0;
            targetI = i0 + deltaJ;
            targetJ = j0 - deltaI;
            if (targetI < 0|| targetJ < 0 || targetI > 38 || targetJ > 20) 
            {
                impossible = true;
                break;
            }
        }
        return impossible;
    }
    void RotateTetrimonos()
    {
        if (!RotationImpossible())
        {
            sounds.PlayMove();
            int i0 = monos[0].i, j0 = monos[0].j;
            foreach (var item in monos)
            {
                Rotate(item, i0, j0);
            }
        }
    }
    void LeftMove()
    {
        bool impossible = false;
        foreach (var item in monos)
        {
            if (item.j == 0) impossible = true;
            else if (item.i <= item.instance.colsHeight[item.j - 1]) 
            {
                impossible = true;
                break;
            }
        }
        if (!impossible)
        {
            sounds.PlayMove();
            //transform.position -= new Vector3(-1,0,0);
            foreach (var item in monos)
            {
                item.j -= 1;
                item.UpdatePosition();

            }
        }
    }

    void RightMove()
    {
        bool impossible = false;
        foreach (var item in monos)
        {
            if (item.j == 20) impossible = true;
            else if (item.i <= item.instance.colsHeight[item.j + 1])
            { 
                impossible = true;
                break;
            }
        }
        if (!impossible)
        {
            sounds.PlayMove();

            //transform.position -= new Vector3(1, 0, 0);
            foreach (var item in monos)
            {
                item.j += 1;
                item.UpdatePosition();

            }
        }
    }




    IEnumerator ManyLeftMoves()
    {
        LeftMove();
        yield return new WaitForSeconds(.23f);
        while (true)
        {
            LeftMove();
            yield return new WaitForSeconds(.02f);
        }
    }
    IEnumerator ManyRightMoves()
    {
        RightMove();
        yield return new WaitForSeconds(.23f);
        while (true)
        {
            RightMove();
            yield return new WaitForSeconds(.02f);
        }
    }



    void Move()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine("ManyLeftMoves");
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            StopCoroutine("ManyLeftMoves");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine("ManyRightMoves");
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            StopCoroutine("ManyRightMoves");
        }

    }


   
}
