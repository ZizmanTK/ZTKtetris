using System.Collections;
using UnityEngine;

public class MoveTetrimono : MonoBehaviour
{
    public float fallSpeed;
    PositionOnGrid position;
    // Start is called before the first frame update
    void Start()
    {
        position = GetComponent<PositionOnGrid>();
        StartCoroutine("Fall");
    }

    // Update is called once per frame
    void Update()
    {
        Move();
       if (Input.GetKey(KeyCode.Return))
        {
            fallSpeed = .009f;
        }
        else fallSpeed = .1f;
    }



    void LeftMove()
    {
        //Tetrimono.position += new Vector3(-1, 0, 0);
        position.j--;
    }
    void RightMove()
    {
        //Tetrimono.position += new Vector3(1, 0, 0);
        position.j++;
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
    IEnumerator ManyLeftMoves()
    {
        LeftMove();
        yield return new WaitForSeconds(.7f);
        //while (Input.GetKey(KeyCode.LeftArrow))
        while (true)
        {
            LeftMove();
            yield return new WaitForSeconds(.1f);
        }
    }
    IEnumerator ManyRightMoves()
    {
        RightMove();
        yield return new WaitForSeconds(.7f);
        while  (true)
        {
            RightMove();
            yield return new WaitForSeconds(.1f);
        }
    }

    IEnumerator Fall()
    {
        while (true)
        {
            yield return new WaitForSeconds(fallSpeed);
            position.i--;
        }
    }
        
}
