using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed;
    public bool hold;
    public int power;
    public int team;
    public GameObject shooter;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += speed * Time.deltaTime * transform.right;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!(collision.collider.CompareTag("Player") && team == collision.collider.GetComponent<Player>().team && power == 3))
        {
            power -= 1;
            GetComponent<Animator>().SetInteger("power", power);
            speed *= .8f;
            Vector3 collisionPoint = collision.GetContact(0).point;
            if (Utils.Distance(collisionPoint.x, 0, transform.position.x, 0) < Utils.Distance(0, collisionPoint.y, 0, transform.position.y))
            {
                transform.eulerAngles *= -1;
            }
            else
            {
                transform.eulerAngles = new(0, 0, 180 - transform.eulerAngles.z);
            }

            if (collision.collider.CompareTag("Player") && team != collision.collider.GetComponent<Player>().team && power >= 1)
            {
                shooter.GetComponent<Player>().score++;
            }
        }
    }
}
