using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        Vector3 dir = Vector3.zero;
        if (kb.wKey.isPressed) dir.z += 1;   // W = +Z
        if (kb.sKey.isPressed) dir.z -= 1;   // S = -Z
        if (kb.dKey.isPressed) dir.x += 1;   // D = +X
        if (kb.aKey.isPressed) dir.x -= 1;   // A = -X
        if (dir == Vector3.zero)
        {
            // stop horizontal motion on release, keep vertical for gravity/floor contact
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }
        rb.linearVelocity = new Vector3(dir.x * speed, rb.linearVelocity.y, dir.z * speed);
    }
}
