using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuralController : MonoBehaviour
{
    [Header("Config")]
    private int playerAmount;
    private int playerSurviveAmount;
    private float randomAmount;

    [Header("Manage")]
    public int playerAlive;
    public Player bestPlayer;
    private Player lastBest;
    public List<Player> bestPlayers = new();
    public List<Player> bestPlayerTeam = new();
    public List<Player> players = new();
    private int gen = 0;
    private Controller controller;

    [Header("Timer")]
    public float time;
    public float lastRun = 0;
    public float queueRunTime = 2f;
    public float lastGame = 0;
    public float queueGameTime = 60f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("Controller").GetComponent<Controller>();
        controller.Begin();

        playerAmount = controller.gameAmount * 2;
        playerSurviveAmount = controller.playerSurviveAmount;
        randomAmount = controller.randomAmount;

        GameObject[] objs = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in objs)
        {
            players.Add(obj.GetComponent<Player>());
        }

        players[0].Start();
        players[1].Start();

        lastBest = players[0];

        GameObject.Find("NeuronUI").GetComponent<NetworkUI>().Build(players[0]);
        GameObject.Find("alive").GetComponent<Text>().text = "Players " + playerAmount.ToString() + " / " + playerAmount.ToString();
        GameObject.Find("Arrow").GetComponent<Arrow>().game = players[0].gameController.gameObject;
        GameObject.Find("Arrow").GetComponent<Arrow>().Begin();
        Check();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (time - lastRun > queueRunTime)
        {
            Check();
            lastRun = time;
        }
        time += Time.deltaTime;

        if (playerAlive <= 0 || time - lastGame > queueGameTime)
        {
            Check();
            Restart();
            lastGame = time;
        }
    }

    void Check()
    {
        playerAlive = playerAmount;
        bestPlayer = players[0];
        bestPlayers.Clear();
        bestPlayerTeam.Clear();
        bestPlayerTeam.Add(players[0]);
        bestPlayerTeam.Add(players[1]);

        foreach (Player player in players)
        {
            if (player.gameOver)
            {
                playerAlive--;
                GameObject.Find("alive").GetComponent<Text>().text = "Player " + playerAlive.ToString() + " / " + playerAmount.ToString();
            }

            if (bestPlayer.score < player.score)
            {
                bestPlayer = player;
            }

            if (bestPlayerTeam[player.team].score < player.score)
            {
                bestPlayerTeam[player.team] = player;
            }

            if (bestPlayers.Count < playerSurviveAmount)
            {
                bestPlayers.Add(player);
            }
            else
            {
                int index = 0;
                for (int i = 0; i < bestPlayers.Count; i++)
                {
                    for (int j = 0; j < bestPlayers.Count; j++)
                    {
                        if (bestPlayers[i].score < bestPlayers[j].score &&
                            bestPlayers[i].score < bestPlayers[index].score)
                        {
                            index = i;
                        }
                    }
                }

                if (bestPlayers[index].score < player.score)
                {
                    bestPlayers[index] = player;
                }
            }
        }

        Arrow arrow = GameObject.Find("Arrow").GetComponent<Arrow>();
        arrow.game = bestPlayer.gameController.gameObject;
        arrow.Direction();
        GameObject.Find("best").GetComponent<Text>().text = "Best Player ID " + bestPlayer.id.ToString();
        GameObject.Find("bestGame").GetComponent<Text>().text = "Best Game ID " + Math.Floor((double)bestPlayer.id / 2).ToString();

        if (lastBest.score < bestPlayer.score)
        {
            GameObject.Find("NeuronUI").GetComponent<NetworkUI>().Build(bestPlayer);
        }
        lastBest = bestPlayer;
    }

    void UpdateUI()
    {
        Debug.Log("Gen: " + gen);
        Debug.Log("Blue Network:");
        Debug.Log(bestPlayerTeam[0].network.Copy());
        Debug.Log("Orange Network:");
        Debug.Log(bestPlayerTeam[1].network.Copy());
        gen++;
        if (bestPlayerTeam[1].score > bestPlayerTeam[0].score)
        {
            GameObject.Find("NeuronUI").GetComponent<NetworkUI>().Build(bestPlayerTeam[1]);
            GameObject.Find("Window Chart").GetComponent<WindowGraph>().score.Add((int)bestPlayerTeam[1].score);
        }
        else
        {
            GameObject.Find("NeuronUI").GetComponent<NetworkUI>().Build(bestPlayerTeam[0]);
            GameObject.Find("Window Chart").GetComponent<WindowGraph>().score.Add((int)bestPlayerTeam[0].score);
        }
        GameObject.Find("gen").GetComponent<Text>().text = "Gen " + gen.ToString();
    }

    void Restart()
    {
        int index = 0;
        int lastIndex = -1;
        string dna = "";
        playerAlive = playerAmount;
        UpdateUI();

        foreach (Player player in players)
        {
            bool isBest = false;
            foreach (Player best in bestPlayers)
            {
                if (player.id == best.id)
                {
                    isBest = true;
                }
            }
            if (isBest)
            {
                player.Restart();
            }
            else
            {
                int randomPlayerAmount = (int)Math.Floor((double)playerAmount * randomAmount) / 2;
                int bestBirdIndex = (int)Math.Floor((double)(((index / 2 - randomPlayerAmount) * (playerSurviveAmount / 2 + 1)) / (playerAmount / 2 - randomPlayerAmount)));
                player.Restart();

                if (index / 2 < randomPlayerAmount)
                {
                    player.network.Random();
                }
                else if (bestBirdIndex < bestPlayers.Count)
                {
                    Player birdMother = bestPlayers[bestBirdIndex];

                    if (bestBirdIndex != lastIndex)
                    {
                        dna = birdMother.network.Copy();
                        lastIndex = bestBirdIndex;
                    }
                    player.network.Paste(dna);
                    player.network.Mutate();
                }
                else
                {
                    Player birdMother = bestPlayers[^1];
                    dna = birdMother.network.Copy();
                    player.network.Paste(dna);
                    player.network.Mutate();
                }
                index++;
            }
        }
        foreach (GameObject game in controller.games)
        {
            game.GetComponent<GameController>().Restart();
        }
        
    }
}
