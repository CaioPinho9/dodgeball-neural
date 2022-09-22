using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Scores")]
    public int orangeScore = 0;
    public int blueScore = 0;
    public int maxScore = 10;

    [Header("Control")]
    public bool gameOver = false;
    public bool restart = false;
    public bool manual = false;

    [Header("References")]
    public Ball ball;
    public List<Player> players = new();
    private SpriteRenderer sp;

    // Start is called before the first frame update
    public void Start()
    {
        sp = transform.GetChild(0).GetComponent<SpriteRenderer>();
        ball = GetComponentInChildren<Ball>();
        foreach (Player player in GetComponentsInChildren<Player>())
        {
            players.Add(player);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (restart)
        {
            Restart();
            restart = false;
        }

        //When the max score is reached, the game stops and a sprite with the winner's color covers it
        if (blueScore >= maxScore)
        {
            gameOver = true;
            sp.color = new(.0784f, .5882f, .8705f);
            sp.sortingOrder = 10;
        }
        else if (orangeScore >= maxScore)
        {
            gameOver = true;
            sp.color = new(.9215f, .4117f, .1294f);
            sp.sortingOrder = 10;
        }
    }

    public void Restart()
    {
        gameOver = false;
        orangeScore = 0;
        blueScore = 0;
        sp.color = new(1, 1, 1);
        sp.sortingOrder = -2;
        foreach (Player player in players)
        {
            player.Restart();
        }
        ball.transform.parent = transform;
        ball.Restart();
    }
}