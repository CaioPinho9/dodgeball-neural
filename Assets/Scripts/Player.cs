using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{   
    [Header("Control")]
    public float rotate;
    public float accelerate;
    public float lateral;
    public float shot;
    public float force = 5;

    [Header("Ball")]
    public float xBallDistance;
    public float yBallDistance;
    public float zBallAngle;
    public int power;
    public float holdingBall = 0;
    private Ball ball;

    [Header("Enemy")]
    public float xEnemyDistance;
    public float yEnemyDistance;
    public float zEnemyAngle;
    private Player enemy;

    [Header("Movement")]
    public float speed = 2f;
    public float degrees = 0;
    public float angle;

    [Header("Gameplay")]
    public bool gameOver = false;
    public float score = 0;
    public static int inputAmount;

    [Header("Identity")]
    public int id;
    public string dna;
    public string rna = "";
    public int team;

    [Header("Timer")]
    public float time;
    public float queueTime = .1f;

    //References
    //public NeuralNetwork network;
    private Rigidbody2D rb;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameController = transform.parent.GetComponent<GameController>();
        ball = gameController.ball;
        enemy = gameController.players[team == 0 ? 1 : 0];
        degrees = transform.eulerAngles.z;
        //Create the neural network
        //network = new();

        //Don't rotate without a command
        rb.freezeRotation = true;
    }

    public void Restart()
    {
        //Position and degrees are different in each team
        transform.localPosition = new(2.5f * (team == 0 ? 1 : -1), 0, 0);
        degrees = team == 0 ? 180 : 0;
        gameOver = false;
        holdingBall = 0;
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //Until losing
        if (!gameOver)
        {
            //Check if losing
            gameOver = gameController.gameOver;

            if (gameController.manual)
            {
                ManualInput();
            }

            //Timer
            if (time > queueTime)
            {
                BallDistance();
                EnemyDistance();


                Fire();
                //Change angle
                Rotate();

                Move();

                time = 0;
            }
            //Increases time and score
            time += Time.deltaTime;
        }
    }

    private void ManualInput()
    {
        if (team == 0)
        {
            //Blue
            accelerate = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKey(KeyCode.DownArrow) ? -1 : 0);
            lateral = (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? -1 : 0);
            rotate = (Input.GetKey(KeyCode.Comma) ? 1 : 0) + (Input.GetKey(KeyCode.Period) ? -1 : 0);
            shot = Input.GetKey(KeyCode.M) ? 1 : 0;
        }
        else
        {
            //Orange
            accelerate = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
            lateral = (Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? -1 : 0);
            rotate = (Input.GetKey(KeyCode.Q) ? 1 : 0) + (Input.GetKey(KeyCode.E) ? -1 : 0);
            shot = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }
    }

    private ArrayList NeuralInput()
    {
        //Organize data in array to sort the inputs
        ArrayList input = new()
        {
            holdingBall,
            xBallDistance,
            yBallDistance,
            zBallAngle,
            power,
            xEnemyDistance,
            yEnemyDistance,
            zEnemyAngle
        };

        //Change how many neurons exists in input layer
        inputAmount = input.Count;
        Controller.neuronsLayer[0] = inputAmount;

        return input;
    }

    private void BallDistance()
    {
        if (holdingBall == 1)
        {
            xBallDistance = 0;
            yBallDistance = 0;
        }
        else
        {
            xBallDistance = ball.transform.position.x - transform.position.x;
            yBallDistance = ball.transform.position.y - transform.position.y;
        }
        zBallAngle = Utils.DistanceAngle(ball.transform, transform);
        power = ball.power;
    }

    private void EnemyDistance()
    {
        xEnemyDistance = enemy.transform.position.x - transform.position.x;
        yEnemyDistance = enemy.transform.position.y - transform.position.y;
        zEnemyAngle = Utils.DistanceAngle(enemy.transform, transform);
    }

    private void Move()
    {
        //Move
        transform.position += 20f * accelerate * Time.deltaTime * transform.right;
        transform.position += 20f * lateral * Time.deltaTime * transform.up;
        
        //Ball fixed
        if (transform.childCount > 0)
        {
            transform.GetChild(0).localPosition = new(1.5f, 0, 0);
        }
    }

    private void Rotate()
    {
        //Increasing the input
        degrees += rotate * 20f;

        //Change angle of plane
        transform.eulerAngles = new(0f, 0f, degrees);
    }

    private void Fire()
    {
        //Shot when holding
        if (shot == 1 && holdingBall == 1)
        {
            holdingBall = 0;

            //Limit force
            if (force >= 10)
            {
                force = 10;
            }

            //Set directon
            ball.transform.eulerAngles = transform.eulerAngles;
            
            //When power is bigger than 1, it scores if hits the enemy
            ball.power = 3;
            ball.speed = force;

            //Send this player to ball
            ball.shooter = transform.gameObject;

            //Remove ball from player
            ball.transform.parent = transform.parent;

            //Enable collider and animate
            ball.transform.GetComponent<CircleCollider2D>().enabled = true;
            ball.transform.GetComponent<Animator>().SetInteger("holding", 0);
            ball.transform.GetComponent<Animator>().SetInteger("power", 3);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ball"))
        {
            //Power greater than 0 is impossible to hold
            if (ball.power <= 0)
            {
                //Disable collider
                ball.transform.GetComponent<CircleCollider2D>().enabled = false;

                //Stops the ball
                ball.speed = 0;

                //Holds the ball
                ball.transform.parent = transform;
                ball.transform.localPosition = new(1.5f, 0, 0);
                holdingBall = 1;

                //Which team is holding
                ball.team = team;

                //Animate
                ball.transform.GetComponent<Animator>().SetInteger("holding", 1);
            }
            else if (ball.team != team)
            {
                //If player is hit when power is greater than 0, the scores decreases
                score -= 1;
            }
        }
    }
 }

