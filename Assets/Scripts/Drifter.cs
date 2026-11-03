using UnityEngine;

// Светлячок или тень: плывёт справа налево и исчезает у края.
public class Drifter : MonoBehaviour
{
    public bool harmful;
    public float speed = 4f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        if (!harmful) transform.position += Vector3.up * Mathf.Sin(Time.time * 3f + transform.position.x) * 0.01f;
        if (transform.position.x < -14f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.GetComponent<Moth>()) return;
        if (harmful) GameManager.Instance.Hurt(); else GameManager.Instance.AddScore(10);
        Destroy(gameObject);
    }
}
