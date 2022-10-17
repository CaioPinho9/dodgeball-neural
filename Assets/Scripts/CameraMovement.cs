using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed;
    private float vertical;
    private float horizontal;
    private float zoom;

    public List<Link> links = new();
    public List<float> width = new();
    private Camera cm;
    private Arrow arrow;

    private void Start()
    {
        cm = GetComponent<Camera>();
        arrow = GameObject.Find("Arrow").GetComponent<Arrow>();
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        if (Input.anyKey)
        {
            arrow.Direction();
            vertical = (Input.GetKey(KeyCode.Keypad8) ? 1 : 0) + (Input.GetKey(KeyCode.Keypad2) ? -1 : 0);
            horizontal = (Input.GetKey(KeyCode.Keypad4) ? 1 : 0) + (Input.GetKey(KeyCode.Keypad6) ? -1 : 0);
            zoom = (Input.GetKey(KeyCode.KeypadMinus) ? .5f : 0) + (Input.GetKey(KeyCode.KeypadPlus) ? -.5f : 0);

            transform.position += speed * vertical * Time.deltaTime * transform.up;
            transform.position += speed * horizontal * Time.deltaTime * -transform.right;

            if (cm.orthographicSize + zoom >= 0)
            {
                cm.orthographicSize += zoom;
                int index = 0;
                foreach (Link link in links)
                {
                    link.render.GetComponent<LineRenderer>().startWidth = width[index] * cm.orthographicSize / 5;
                    link.render.GetComponent<LineRenderer>().endWidth = width[index] * cm.orthographicSize / 5;
                    index++;
                }
            }
        }
    }
}
