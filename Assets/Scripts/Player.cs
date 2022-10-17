using System;
using System.Collections;
using System.Collections.Generic;
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
    public float ballDistance;
    public float ballAngle;
    public float ballPower;
    public float holdingBall = 0;
    private Ball ball;

    [Header("Enemy")]
    public float enemyDistance;
    public float enemyAngle;
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
    public float queueTime = .5f;

    //Sensor
    [Header("Sensors")]
    public float[] innerWallDistance = new float[4];
    public float[] outerWallDistance = new float[7];
    private readonly float sensorLenght = 20f;
    private readonly float[] innerDegrees = { 0, 90, 180 };
    private readonly float[] outerDegrees = { 0, 15, 30, 45 };
    public LayerMask innerMask;
    public LayerMask outerMask;

    //References
    public NeuralNetwork network;
    private Rigidbody2D rb;
    public GameController gameController;
    private Controller controller;

    // Start is called before the first frame update
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameController = transform.parent.GetComponent<GameController>();
        controller = GameObject.Find("Controller").GetComponent<Controller>();
        ball = gameController.ball;
        enemy = gameController.players[team == 0 ? 1 : 0];
        degrees = transform.eulerAngles.z;
        innerWallDistance = Sensors(innerDegrees, innerMask);
        outerWallDistance = Sensors(outerDegrees, outerMask);
        NeuralInput();
        //Create the neural network
        network = new(team);

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
                Outside();
                
                BallDistance();
                EnemyDistance();

                if (!gameController.manual)
                {
                    RunNetwork();
                }

                Fire();
                //Change angle
                Rotate();

                time = 0;
            }
            Move();

            innerWallDistance = Sensors(innerDegrees, innerMask);
            outerWallDistance = Sensors(outerDegrees, outerMask);

            //Increases time and score
            time += Time.deltaTime;
        }
    }

    private void Outside()
    {
        if (!gameController.GetComponentInChildren<BoxCollider>().bounds.Contains(transform.position))
        {
            //Return to the field
            transform.localPosition = new(2.5f * (team == 0 ? 1 : -1), 0, 0);
            score -= 20;
        }

        if (team == 0 && transform.localPosition.x < 0)
        {
            //Return to the field
            transform.localPosition = new(2.5f * (team == 0 ? 1 : -1), 0, 0);
            score -= 20;
        }

        if (team == 1 && transform.localPosition.x > 0)
        {
            //Return to the field
            transform.localPosition = new(2.5f * (team == 0 ? 1 : -1), 0, 0);
            score -= 20;
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
            ballDistance,
            ballAngle,
            ballPower,
            enemyDistance,
            enemyAngle,
            innerWallDistance,
            outerWallDistance
        };

        //Change how many neurons exists in input layer
        int totalIndex = 0;
        for (int inputIndex = 0; inputIndex < input.Count; inputIndex++)
        {
            if (!input[inputIndex].GetType().IsArray)
            {
                totalIndex++;
            }
            else
            {
                //Iterating the sensors
                float[] array = (float[])input[inputIndex];
                totalIndex += array.Length;
            }
        }
        controller.neuronsLayer[0] = totalIndex;

        return input;
    }

    private void RunNetwork()
    {
        List<float> output = network.RunNetwork(NeuralInput());
        accelerate = (output[0] > 0 ? 1 : 0) + (output[1] > 0 ? -1 : 0);
        lateral = (output[2] > 0 ? 1 : 0) + (output[3] > 0 ? -1 : 0);
        rotate = (output[4] > 0 ? 1 : 0) + (output[5] > 0 ? -1 : 0);
        shot = output[6] > 0 ? 1 : 0;
    }

    private float[] Sensors(float[] sensorDegrees, LayerMask layerMask)
    {
        //Direction index has more indexs than sensorDegrees, because direction also has negative values
        int directionIndex = 0;
        int sensorSize = 0;
        foreach (float i in sensorDegrees)
        {
            if (i == 0 || i == 180)
            {
                sensorSize++;
            }
            else
            {
                sensorSize += 2;
            }
        }
        Vector3[] sensorDirection = new Vector3[sensorSize];


        //Convert degrees in Vector2 direction
        for (int i = 0; i < sensorDegrees.Length; i++)
        {
            //Degrees to radian, positive and zero
            angle = (float)Math.PI * (sensorDegrees[i] + degrees) / 180;

            //Zero doesn't have a negative value
            if (sensorDegrees[i] == 0)
            {
                //Direction is a Vector2, with range between 0 and 1
                sensorDirection[0] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
                directionIndex++;
            }
            else if (sensorDegrees[i] == 180)
            {
                //Direction is a Vector2, with range between 0 and 1
                sensorDirection[^1] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
                directionIndex++;
            }
            else
            {
                //Direction is a Vector2, with range between 0 and 1
                sensorDirection[directionIndex] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);

                //Degrees to radian, negative
                angle = (float)Math.PI * (-sensorDegrees[i] + degrees) / 180;
                sensorDirection[directionIndex + 1] = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);

                //Index for positive and negative value
                directionIndex += 2;

            }
        }

        //Unity didn't allow global arrays
        float[] distance = new float[sensorSize];

        //Create a raycast
        int sensorIndex = 0;
        foreach (Vector3 direction in sensorDirection)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, sensorLenght, ~layerMask);
            if (hit.transform != null)
            {
                //Saving raycast hit position and object type
                distance[sensorIndex] = Utils.Distance(hit.point.x, hit.point.y, transform.position.x, transform.position.y);

                Debug.DrawLine(transform.position, hit.point);
            }
            else
            {
                distance[sensorIndex] = 0;
            }

            sensorIndex++;
        }

        //Global Output
        return distance;
    }
    private void BallDistance()
    {
        if (holdingBall == 1)
        {
            ballDistance = 0;
            ballAngle = 0;
        }
        else
        {
            ballDistance = Utils.Distance(ball.transform.localPosition.x, ball.transform.localPosition.y, transform.localPosition.x, transform.localPosition.y);
            ballAngle = Utils.DistanceAngle(ball.transform, transform);
        }
        ballPower = ball.power;
        ballPower = (ballPower > 1) ? 1 : 0;
    }

    private void EnemyDistance()
    {
        enemyDistance = Utils.Distance(enemy.transform.localPosition.x, enemy.transform.localPosition.y, transform.localPosition.x, transform.localPosition.y);
        enemyAngle = Utils.DistanceAngle(enemy.transform, transform);
    }

    private void Move()
    {
        //Move
        transform.localPosition += 2f * accelerate * Time.deltaTime * transform.right;
        transform.localPosition += 2f * lateral * Time.deltaTime * transform.up;
        
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
            ball.hold = false;

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
            ball.shooter = GetComponent<Player>();

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
            if (ball.power == 0)
            {
                //Disable collider
                ball.transform.GetComponent<CircleCollider2D>().enabled = false;

                //Stops the ball
                ball.speed = 0;

                //Holds the ball
                ball.transform.parent = transform;
                ball.hold = true;
                ball.transform.localPosition = new(1.5f, 0, 0);
                holdingBall = 1;
                score++;

                //Which team is holding
                ball.team = team;

                //Animate
                ball.transform.GetComponent<Animator>().SetInteger("holding", 1);
            }
            else if (ball.team != team)
            {
                //If player is hit when power is greater than 0, the scores decreases
                score -= 40;
            }
        }
    }
 }

