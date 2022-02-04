using UnityEngine;

public class Effects : MonoBehaviour
{
    // Start is called before the first frame update
    Animator animator;
    SpriteRenderer spriteRenderer;
    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayAtPosition(float y)
    {
        transform.position = new Vector2(transform.position.x, y);
        animator.SetTrigger("play");
    }
}

