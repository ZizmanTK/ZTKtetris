using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrimonoBehaviour : MonoBehaviour
{
    public float difficulty = .01f;
    [HideInInspector]
    public bool gamePaused = false;
    public Sounds sounds;
    Form.Forms nextForm;
    public Form form;
    [HideInInspector]
    public bool placed =false;
    public float normalFallSpeed = .1f;
    public float quickFallSpeed = .009f;
    float fallSpeed;
    [HideInInspector]
    public PositionOnGrid[] monos;
    public GameGrid instance;

    private void Awake()
    {
        monos = GetComponent<MoveTetrimonos>().monos;
        nextForm = (Form.Forms)Random.Range(0, 7);
        //nextForm = Form.Forms.I;
        instance.PickNewColor();

    }
    void Start()
    {
        ReplaceTetrimono();
        nextForm = (Form.Forms)Random.Range(0, 7);
        Form.ChangeForm(monos, nextForm);
        form.DisplayNextForm(nextForm);
        fallSpeed = normalFallSpeed;
        StartCoroutine("Fall");


    }

    void Update()
    {
        Placed();
        if (!gamePaused)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                fallSpeed = quickFallSpeed;
                StopCoroutine("Fall");
                StartCoroutine("Fall");
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                fallSpeed = normalFallSpeed;
                StopCoroutine("Fall");
                StartCoroutine("Fall");
            }
        }
    }

    void Placed()
    {
        if (placed)
        {
            sounds.PlayPlaced();
            StopCoroutine("Fall");
            FillHoles();
            OnPlaced();
            OnRowFilled();
            Form.ChangeForm(monos, nextForm);
            ReplaceTetrimono();
            StartCoroutine("Fall");
            nextForm = (Form.Forms)Random.Range(0,7);
            form.DisplayNextForm(nextForm);
            instance.PickNewColor();

        }
    }


    void ReplaceTetrimono()
    {
        foreach (var item in monos)
        {
            item.i = item.spawnI;
            item.j = item.spawnJ;

        }
    }
    void OnPlaced()
    {
        int[] sortedIndex = SortByRow();
        for(int i = 0; i < sortedIndex.Length; i++)
        {
            monos[sortedIndex[i]].OnPlaced();
        }
        /*foreach (var item in monos)
        {
            item.OnPlaced();
        }*/
        placed = false;
        
    }


    int [] SortByRow()
    {
        int[] index = new int[monos.Length];
        for(int i = 0; i < monos.Length; i++)
        {
            index[i] = i;
        }
        int tempvar;
        for (int i = 0; i < index.Length -1; i++)
        {
            for (int j = 0; j < index.Length - i -1; j++)
            {
                if (monos[index[j]].i > monos[index[j + 1]].i)
                {
                    tempvar = index[j];
                    index[j] = index[j + 1];
                    index[j+1] = tempvar;
                }
            }
        }
        return index;
    }
    void FillHoles()
    {
        SortByRow();
        
    }
    void OnRowFilled()
    {
         foreach (var item in monos)
        {
            if (item.RowFilled()) 
            { 
                item.OnRowFilled(item.i);
                if(normalFallSpeed -difficulty > 0)normalFallSpeed -= difficulty;
            }
        }
    }

    IEnumerator Fall()
    {

        //Debug.Log("Starting Fall");
        while (!placed)
        {
            bool localPlaced = false;
            yield return new WaitForSeconds(fallSpeed);
            foreach (var item in monos)
            {
                item.Fall();
                if (item.IsPlaced()) localPlaced = true;
            }
            placed = localPlaced;
        }
        //Debug.Log("Stopping Fall");

    }



    public void StopFall()
    {
        StopCoroutine("Fall");
    }

    public void StartFall()
    {
        StartCoroutine("Fall");
    }
}
