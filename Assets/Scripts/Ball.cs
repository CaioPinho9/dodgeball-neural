using System;
using TMPro;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Physics")]
    public float speed;
    public int power;

    [Header("Status")]
    public bool hold;
    public bool gameOver;

    [Header("Team")]
    public int team;
    public Player shooter;

    private Rigidbody2D rb;
    private Animator anim;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gameController = transform.parent.GetComponent<GameController>();
        rb.freezeRotation = true;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gameController = transform.parent.GetComponent<GameController>();
        rb.freezeRotation = true;
    }

    public void Restart()
    {
        transform.localPosition = Vector3.zero;
        hold = false;
        gameOver = false;
        power = 0;
        speed = 0;
        GetComponent<CircleCollider2D>().enabled = true;
        anim.SetInteger("holding", 0);
        anim.SetInteger("power", power);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            transform.position += speed * Time.deltaTime * transform.right;
        }

        //Check if ball is not inside the field
        if (!hold && Math.Abs(transform.localPosition.x) > 5.1 || Math.Abs(transform.localPosition.y) > 3.1)
        {
            //Return to the field
            transform.localPosition = Vector3.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!gameOver && !(collision.collider.CompareTag("Player") && team == collision.collider.GetComponent<Player>().team && power == 3))
        {
            Vector3 collisionPoint = collision.GetContact(0).point;
            if (Utils.Distance(collisionPoint.x, 0, transform.position.x, 0) <= Utils.Distance(0, collisionPoint.y, 0, transform.position.y))
            {
                transform.eulerAngles *= -1;
            }
            else
            {
                transform.eulerAngles = new(0, 0, 180 - transform.eulerAngles.z);
            }

            //Reduce power when collide
            if (power > 0)
            {
                power--;
            }
            anim.SetInteger("power", power);
            speed *= .8f;

            if (collision.collider.CompareTag("Player") && team != collision.collider.GetComponent<Player>().team && power > 1)
            {
                shooter.score += 30;
                Transform canvas = transform.parent.Find("Canvas");
                if (collision.collider.GetComponent<Player>().team == 0)
                {
                    gameController.orangeScore++;
                    canvas.Find("Orange").GetComponent<TMP_Text>().text = gameController.orangeScore.ToString();
                }
                else
                {
                    gameController.blueScore++;
                    canvas.Find("Blue").GetComponent<TMP_Text>().text = gameController.blueScore.ToString();
                }
            }
        }
    }
}
