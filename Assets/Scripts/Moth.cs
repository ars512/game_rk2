using UnityEngine;

// Герой — ночная бабочка-фонарщик. Бегает по земле и прыгает.
[RequireComponent(typeof(Rigidbody2D))]
public class Moth : MonoBehaviour
{
    public float speed = 9f, jumpSpeed = 17f, minX = -11f, maxX = 11f;
    public float groundY = -3f;

    Rigidbody2D body;
    int jumpsLeft;

    void Awake() { body = GetComponent<Rigidbody2D>(); }

    void FixedUpdate()
    {
        bool grounded = body.position.y <= groundY + 0.02f && body.linearVelocity.y <= 0.1f;
        if (grounded) jumpsLeft = 2; // двойной прыжок: так проще перепрыгивать тени

        var v = body.linearVelocity;
        v.x = VirtualStick.Horizontal * speed;
        if (JumpButton.Pressed && jumpsLeft > 0) { v.y = jumpSpeed; jumpsLeft--; }
        JumpButton.Pressed = false;
        body.linearVelocity = v;

        var p = body.position;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        body.position = p;

        if (Mathf.Abs(v.x) > 0.1f) transform.localScale = new Vector3(Mathf.Sign(v.x), 1, 1);
    }
}
