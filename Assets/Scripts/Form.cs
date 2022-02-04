using UnityEngine;
using UnityEngine.UI;


public class Form : MonoBehaviour
{
    //public PositionOnGrid[] monos;
    //public Forms form;
    //public int activeMonos;
    // Start is called before the first frame update
    public Image preview;
    public Sprite L;
    public Sprite Z;
    public Sprite T;
    public Sprite O;
    public Sprite J;
    public Sprite S;
    public Sprite I;
    void Awake()
    {
        //monos = GetComponent<MoveTetrimonos>().monos;
    }

    // Update is called once per frame
    void Update()
    {

    }



    public static void ChangeForm(PositionOnGrid[] monos, Forms form)
    {
        int refI = monos[0].spawnI, refJ = monos[0].spawnJ;
        switch (form)
        {
            case Forms.L:
                monos[1].spawnI = refI - 1;
                monos[2].spawnI = refI - 2;
                monos[3].spawnI = refI - 2;

                monos[1].spawnJ = refJ;
                monos[2].spawnJ = refJ;
                monos[3].spawnJ = refJ + 1;
                break;
            case Forms.Z:
                monos[1].spawnI = refI;
                monos[2].spawnI = refI - 1;
                monos[3].spawnI = refI - 1;

                monos[1].spawnJ = refJ +1;
                monos[2].spawnJ = refJ + 1;
                monos[3].spawnJ = refJ + 2;
                break;
            case Forms.T:
                monos[1].spawnI = refI - 1;
                monos[2].spawnI = refI - 1;
                monos[3].spawnI = refI - 1;

                monos[1].spawnJ = refJ;
                monos[2].spawnJ = refJ + 1;
                monos[3].spawnJ = refJ - 1;
                break;
            case Forms.O:
                monos[1].spawnI = refI;
                monos[2].spawnI = refI - 1;
                monos[3].spawnI = refI - 1;

                monos[1].spawnJ = refJ + 1;
                monos[2].spawnJ = refJ;
                monos[3].spawnJ = refJ + 1;
                break;
            case Forms.J:
                monos[1].spawnI = refI - 1;
                monos[2].spawnI = refI - 2;
                monos[3].spawnI = refI - 2;

                monos[1].spawnJ = refJ;
                monos[2].spawnJ = refJ;
                monos[3].spawnJ = refJ - 1;
                break;
            case Forms.S:
                monos[1].spawnI = refI;
                monos[2].spawnI = refI - 1;
                monos[3].spawnI = refI - 1;

                monos[1].spawnJ = refJ - 1;
                monos[2].spawnJ = refJ - 1;
                monos[3].spawnJ = refJ - 2;
                break;
            case Forms.I:
                monos[1].spawnI = refI - 1;
                monos[2].spawnI = refI - 2;
                monos[3].spawnI = refI - 3;

                monos[1].spawnJ = refJ;
                monos[2].spawnJ = refJ;
                monos[3].spawnJ = refJ;
                break;
        }
    }


    public void DisplayNextForm( Forms nextForm)
    {
        switch (nextForm)
        {
            case Forms.S:
                preview.sprite = S;
                break;
            case Forms.L:
                preview.sprite = L;
                break;
            case Forms.J:
                preview.sprite = J;
                break;
            case Forms.Z:
                preview.sprite = Z;
                break;
            case Forms.T:
                preview.sprite = T;
                break;
            case Forms.O:
                preview.sprite = O;
                break;
            case Forms.I:
                preview.sprite = I;
                break;
        }
    }
    public enum Forms
    {
        L,
        Z,
        T,
        O,
        J,
        S,
        I,
    }
}
