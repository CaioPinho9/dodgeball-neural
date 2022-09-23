using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed;
    private float vertical;
    private float horizontal;
    private float zoom;

    // Update is called once per frame
    void Update()
    {
        //Movement
        vertical = (Input.GetKey(KeyCode.Keypad8) ? 1 : 0) + (Input.GetKey(KeyCode.Keypad2) ? -1 : 0);
        horizontal = (Input.GetKey(KeyCode.Keypad4) ? 1 : 0) + (Input.GetKey(KeyCode.Keypad6) ? -1 : 0);
        zoom = (Input.GetKey(KeyCode.KeypadMinus) ? .1f : 0) + (Input.GetKey(KeyCode.KeypadPlus) ? -.1f : 0);

        transform.position += speed * vertical * Time.deltaTime * transform.up;
        transform.position += speed * horizontal * Time.deltaTime * -transform.right;

        if (GetComponent<Camera>().orthographicSize + zoom >= 0)
        {
            GetComponent<Camera>().orthographicSize += zoom;
        }
    }
}
