using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TopDownPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 讀鍵盤（New Input System）
        if (Keyboard.current == null) return;

        float h = 0f;
        float v = 0f;

        if (Keyboard.current.aKey.isPressed) h -= 1f;
        if (Keyboard.current.dKey.isPressed) h += 1f;
        if (Keyboard.current.wKey.isPressed) v += 1f;
        if (Keyboard.current.sKey.isPressed) v -= 1f;

        Vector3 move = new Vector3(h, 0f, v);
        if (move.sqrMagnitude > 1f) move.Normalize();

        // 如果有 Rigidbody：用 MovePosition（碰撞更正常）
        if (rb != null && !rb.isKinematic)
        {
            Vector3 nextPos = rb.position + move * moveSpeed * Time.deltaTime;
            rb.MovePosition(nextPos);
        }
        else
        {
            // 沒有 Rigidbody 就退回用 transform（但比較容易穿牆）
            transform.position += move * moveSpeed * Time.deltaTime;
        }
    }
}
