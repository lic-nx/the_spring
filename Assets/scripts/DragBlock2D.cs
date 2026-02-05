using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DragBlock2D : MonoBehaviour
{
    public float followSpeed = 15f;
    private Rigidbody2D rb;
    private Camera cam;
    private bool isDragging;
    private Vector2 offset;

    [SerializeField] private AudioClip soundTake;
    [SerializeField] private AudioClip soundRelease;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        isDragging = true;

        AudioSource.PlayClipAtPoint(
        soundTake,
        transform.position,
        1f
    );

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        offset = rb.position - mouseWorldPos;

        rb.velocity = Vector2.zero;
    }

    void OnMouseUp()
    {
        isDragging = false;

        AudioSource.PlayClipAtPoint(
        soundRelease,
        transform.position,
        1f
    );
        
        rb.velocity = Vector2.zero;
        player_move._instance?.OnWorldChanged();
    }

    void FixedUpdate()
    {
        if (!isDragging) return;

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetPos = mouseWorldPos + offset;

        Vector2 newPos = Vector2.MoveTowards(
            rb.position,
            targetPos,
            followSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPos);
    }
}