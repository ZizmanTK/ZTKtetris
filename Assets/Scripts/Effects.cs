using UnityEngine;

// Flash played over a cleared row.
public class Effects : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAtPosition(float y)
    {
        transform.position = new Vector2(transform.position.x, y);
        animator.SetTrigger("play");
    }
}
