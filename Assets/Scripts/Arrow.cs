using System;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    public Camera cm;
    public GameObject game;
    public GameObject button;
    private Image im;
    private Button bt;
    private GameController gameController;
    private RectTransform rt;
    public float xMargin;
    public float yMargin;
    private float xMax;
    private float yMax;
    public float angle;
    public bool lockTarget;
    private Vector3 lastPosition;

    // Start is called before the first frame update
    public void Begin()
    {
        cm = GameObject.Find("Main Camera").GetComponent<Camera>();
        im = button.GetComponent<Image>();
        bt = button.GetComponent<Button>();
        rt = button.GetComponent<RectTransform>();
        xMax = cm.pixelWidth / 2 - xMargin;
        yMax = cm.pixelHeight / 2 - yMargin;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Keypad9))
        {
            lockTarget = !lockTarget;
        }

        if (lockTarget && lastPosition != transform.position)
        {
            Move();
            lastPosition = transform.position;
        }

        if (game.GetComponentInChildren<Renderer>().isVisible)
        {
            im.enabled = false;
            bt.enabled = false;
            transform.Find("Border").gameObject.SetActive(false);
        } 
        else
        {
            im.enabled = true;
            bt.enabled = true;
            transform.Find("Border").gameObject.SetActive(true);
        }
        

        gameController = game.GetComponent<GameController>();
        if (gameController.players[0].score >= gameController.players[1].score)
        {
            im.color = new(.0784f, .5882f, .8705f);
        }
        else
        {
            im.color = new(.9215f, .4117f, .1294f);
        }
    }

    public void Move()
    {
        cm.transform.position = new(game.transform.position.x, game.transform.position.y, -10);
    }

    public void Direction()
    {
        angle = Utils.DistanceAngle(game.transform, cm.transform);
        if (game.transform.position.y < cm.transform.position.y)
        {
            angle *= -1;
        }
        transform.eulerAngles = new(0, 0, angle - 90);

        float radians = (float)(Math.PI / 180) * angle;
        

        Vector2 vector = new((float)Math.Cos(radians), (float)Math.Sin(radians));

        if (Math.Abs(vector.x) >= Math.Abs(vector.y))
        {
            vector *= xMax;
        } 
        else
        {
            vector *= yMax;
        }
        rt.anchoredPosition = new(vector.x, vector.y);
    }
}
