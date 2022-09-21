using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{//Control
    [Header("Control")]
    public float rotate;
    public float accelerate;
    public float lateral;
    public bool shot;
    public float force = 5;

    //Movement
    [Header("Movement")]
    public float speed = 2f;
    public float degrees = 0;
    public float angle;

    [Header("Gameplay")]
    //Gameplay
    public float holdingBall = 0;
    public bool gameOver = false;
    public float score = 0;

    [Header("Identity")]
    public int id;
    public string dna;
    public string rna = "";
    public int team;

    [Header("Timer")]
    //Timer
    public float time;
    public float queueTime = .1f;

    //References
    //public NeuralNetwork network;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        degrees = transform.eulerAngles.z;
        //Create the neural network
        //network = new();
        //Don't rotate without a command
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Until losing
        if (!gameOver)
        {
            accelerate = Input.GetAxis("Vertical");
            lateral = Input.GetAxis("Horizontal");
            rotate = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
            shot = Input.GetMouseButton(0);

            //Timer
            if (time > queueTime)
            {
                Fire();
                //Change angle
                Rotate();

                Move();

                time = 0;
            }
            //Increases time and score
            time += Time.deltaTime;

            //Animate
            //Animate(speed, horizontal);
        }
    }

    private void Move()
    {
        //Move
        transform.position += 20f * accelerate * Time.deltaTime * transform.right;
        transform.position += 20f * lateral * Time.deltaTime * -transform.up;
    }

    private void Rotate()
    {
        //Increasing the input
        degrees += rotate * -20f;

        //Change angle of plane
        transform.eulerAngles = new(0f, 0f, degrees);
    }

    private void Fire()
    {
        //Manual and automatic input, shot when recharged
        if (shot && holdingBall == 1)
        {
            holdingBall = 0;
            Transform ball = transform.GetChild(0).transform;
            ball.GetComponent<Animator>().SetInteger("holding", 0);
            ball.eulerAngles = transform.eulerAngles;
            ball.GetComponent<Ball>().power = 3;
            ball.GetComponent<Ball>().speed = force;
            ball.GetComponent<Ball>().shooter = transform.gameObject;
            ball.GetComponent<CircleCollider2D>().enabled = true;
            ball.GetComponent<Animator>().SetInteger("power", 3);
            ball.parent = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Transform ball = collision.transform;
        if (ball.CompareTag("Ball"))
        {
            if (ball.GetComponent<Ball>().power <= 0)
            {
                ball.GetComponent<CircleCollider2D>().enabled = false;
                ball.parent = transform;
                ball.GetComponent<Ball>().speed = 0;
                ball.localPosition = new(1.5f, 0, 0);
                ball.GetComponent<Ball>().team = team;
                holdingBall = 1;
                ball.GetComponent<Animator>().SetInteger("holding", 1);
            }
            else if (ball.GetComponent<Ball>().team != team)
            {
                score -= 1;
            }
        }
    }
 }

