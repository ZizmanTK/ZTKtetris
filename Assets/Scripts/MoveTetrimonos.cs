using UnityEngine;

// Keyboard input for the falling piece. Disabled by Controls while paused.
public class MoveTetrimonos : MonoBehaviour
{
    public TetrimonoBehaviour behaviour;
    // Delayed auto shift: hold left/right to slide after a short pause.
    public float autoShiftDelay = 0.17f;
    public float autoShiftRepeat = 0.035f;

    int direction;
    float shiftTimer;

    void OnDisable()
    {
        behaviour.SoftDropping = false;
        direction = 0;
    }

    void Update()
    {
        if (behaviour.IsGameOver) return;

        bool left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) StartShift(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) StartShift(1);
        else if (direction == -1 && !left) direction = right ? 1 : 0;
        else if (direction == 1 && !right) direction = left ? -1 : 0;

        if (direction != 0)
        {
            shiftTimer -= Time.deltaTime;
            while (shiftTimer <= 0f && behaviour.Move(direction))
                shiftTimer += autoShiftRepeat;
            if (shiftTimer < 0f) shiftTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.X))
            behaviour.Rotate(1);
        if (Input.GetKeyDown(KeyCode.Z))
            behaviour.Rotate(-1);
        if (Input.GetKeyDown(KeyCode.Space))
            behaviour.HardDrop();
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            behaviour.Hold();

        behaviour.SoftDropping = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
    }

    void StartShift(int dir)
    {
        direction = dir;
        behaviour.Move(dir);
        shiftTimer = autoShiftDelay;
    }
}
