using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Speeds { Slow = 0, Normal = 1, Fast = 2, Faster = 3, Fastest = 4 };
// public enum Gamemodes { Cube = 0, Ship = 1 }

public class Movement : MonoBehaviour
{
    public Speeds CurrentSpeed;
    // public Gamemodes CurrentGameMode;
    //                       0      1      2       3      4
    float[] SpeedValues = { 8.6f, 10.4f, 12.96f, 15.6f, 19.27f };

    public float GroundCheckRadius;
    public float yLastPortal = -2.3f;
    public LayerMask GroundMask;
    public Transform Sprite;

    Rigidbody2D rb;

    int Gravity = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        transform.position += Vector3.right * SpeedValues[(int)CurrentSpeed] * Time.deltaTime;

        if (rb.velocity.y < -24.2f) {
            rb.velocity = new Vector2(rb.velocity.x, -24.2f);
        }

        Invoke("Cube", 0);
    }

    bool OnGround()
    {
        return Physics2D.OverlapBox(transform.position + Vector3.down * Gravity * 0.5f, Vector2.right * 1.1f + Vector2.up * GroundCheckRadius, 0, GroundMask);
    }

    bool TouchingWall() {
        return Physics2D.OverlapBox((Vector2)transform.position * (Vector2.right * 0.55f), Vector2.up * 0.8f * (Vector2.right * GroundCheckRadius), 0, GroundMask); 
    }

    void Cube() {
        if (OnGround())
        {
            Vector3 Rotation = Sprite.rotation.eulerAngles;
            Rotation.z = Mathf.Round(Rotation.z / 90) * 90;
            Sprite.rotation = Quaternion.Euler(Rotation);

            // Jump
            if (Input.GetKey(KeyCode.Space))
            {
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.up * 26.6581f * Gravity, ForceMode2D.Impulse);
            } 
         }
        else
        {
            Sprite.Rotate(Vector3.back, 452.4f * Time.deltaTime * Gravity);
        }
    }
    public void ChangeThroughPortal( Speeds Speed, int gravity, int State, float yPortal) {
        switch (State) {
            case 0:
                CurrentSpeed = Speed;
                break;
            case 1:
                Gravity = gravity;
                rb.gravityScale = Mathf.Abs(rb.gravityScale) * (int)gravity;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Portal portal = collision.gameObject.GetComponent<Portal>();
        if (portal) {
            portal.initiatePortal(this);
        }
     }
}
