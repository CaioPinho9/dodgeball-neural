using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Config")]
    public bool restart = false;
    public int gameAmount;
    public int playerSurviveAmount;
    public float randomAmount;

    public GameObject game;
    public List<GameObject> games;
    private readonly float gameXSize = 10.5f;
    private readonly float gameYSize = 6.5f;

    [Header("Config Neural Network")]

    //Neural Setting
    public int[] neuronsLayer;
    //Max value of weights and bias
    public int weightLimit;
    //Max value of mutation
    public int mutate;
    public float learningRate;

    // Start is called before the first frame update
    public void Begin()
    {
        //Calculate the size of the game matrix
        int[] gameDimensions = GameAmount();
        int gameHeight = gameDimensions[0];
        int gameWidth = gameDimensions[1];

        //If even, it centralizes
        float heightEven = 0;
        float widthEven = 0;

        if (gameHeight % 2 == 0)
        {
            heightEven = .5f;
        }
        if (gameWidth % 2 == 0)
        {
            widthEven = .5f;
        }

        //Id of the game
        int index = 0;

        //Creates the game like a matrix, centralizing the center in the camera
        for (float i = gameHeight / -2; i <= gameHeight / 2 - heightEven; i++)
        {
            for (float j = gameWidth / -2; j <= gameWidth / 2 - widthEven; j++)
            {
                //Create game
                GameObject instantiated = Instantiate(game);
                games.Add(instantiated);
                instantiated.transform.position = new((j + widthEven) * gameXSize, (-i - heightEven) * gameYSize);
                instantiated.transform.SetParent(transform);

                //Set id
                foreach (Player player in instantiated.GetComponent<GameController>().players)
                {
                    player.id += index * 2;
                    player.name = "Player " + player.id.ToString();
                }
                //Rename game
                instantiated.name = "Game " + index.ToString();
                instantiated.transform.Find("Canvas").Find("Id").GetComponent<TMP_Text>().text = index.ToString();
                index++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Check if gameAmount was changed
        if (restart)
        {
            //Destroy and recreate games
            foreach (GameObject game in GameObject.FindGameObjectsWithTag("Game"))
            {
                Destroy(game);
            }
            Begin();
            restart = false;
        }
    }

    private int[] GameAmount()
    {
        int gameHeight;
        int gameWidth;
        int[] gameDimensions = new int[2];
        List<int> dividers = new();

        //Prime numbers are impossible to use to create a rectangular matrix
        if (Utils.IsPrime(gameAmount) && gameAmount != 2)
        {
            gameAmount += 1;
        }

        //Finds all dividers
        for (int i = 1; i <= gameAmount; i++)
        {
            if (gameAmount % i == 0)
            {
                dividers.Add(i);
            }
        }

        //In the middle of the array, there're the best two numbers to create a rectangular matrix
        int index = (int)Math.Floor((double)dividers.Count / 2);
        if (dividers.Count % 2 != 0)
        {
            //If the array is odd, it's possible to create a square matrix
            gameHeight = dividers[index];
            gameWidth = gameHeight;
        }
        else
        {
            gameHeight = dividers[index];
            gameWidth = dividers[index - 1];
        }

        gameDimensions[0] = gameHeight;
        gameDimensions[1] = gameWidth;

        return gameDimensions;
    }
}
