using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class NetworkUI : MonoBehaviour
{
    //First neuron position
    [Header("Start Position")]
    public float startX;
    public float startY;

    //Size
    [Header("Size")]
    public float width;
    public float height;

    //Distance between neurons
    private float neuronDistanceX;
    private float neuronDistanceY;

    public GameObject neuronCircle;
    private List<Neuron> neuronData = new();
    private List<Link> linkData = new();
    private Player bestNetwork;
    private Layer[] network;

    public Material material;

    private bool built = false;
    private float weightLimit;

    private float time = 0;
    private readonly float queueTime = .5f;

    private Camera cm;


    private void Start()
    {
        weightLimit = NeuralNetwork.weightLimit;
    }

    private void Awake()
    {
        cm = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (built && bestNetwork != null)
        {
            if (time > queueTime)
            {
                int renderIndex = 0;
                int linkIndex = 0;
                foreach (Layer layer in bestNetwork.network.layer)
                {
                    foreach (Neuron neuron in layer.neuron)
                    {
                        if (neuron.output > 0)
                        {
                            neuronData[renderIndex].render.GetComponentInChildren<SpriteRenderer>().color = bestNetwork.network.team == 0 ? new(.3098f, .6352f, 1f) : new(1f, .3725f, .1215f);
                        }
                        else
                        {
                            neuronData[renderIndex].render.GetComponentInChildren<SpriteRenderer>().color = new(1, 1, 1);
                        }
                        neuronData[renderIndex].render.transform.GetChild(1).GetComponentInChildren<Text>().text = neuron.output.ToString("0.00");
                        renderIndex++;
                    }
                    foreach (Link link in layer.link)
                    {
                        float width = Width(linkIndex) * cm.orthographicSize / 5;
                        LineRenderer lr = linkData[linkIndex].render.GetComponent<LineRenderer>();
                        lr.startWidth = width;
                        lr.endWidth = width;

                        if (link.neuron1.output > 0)
                        {
                            Color color = bestNetwork.network.team == 0 ? new(.0784f, .5882f, .8705f) : new(.9215f, .4117f, .1294f);
                            lr.startColor = color;
                            lr.endColor = color;
                        }
                        else
                        {
                            Color color = new(0, 0, 0);
                            lr.startColor = color;
                            lr.endColor = color;
                        }
                        linkIndex++;
                    }
                }
                time = 0;
            }
            time += Time.deltaTime;
        }
    }

    public void Build(Player bestNetwork)
    {
        this.bestNetwork = bestNetwork;
        network = bestNetwork.network.layer;

        //Clear
        if (neuronData.Count > 0 && linkData.Count > 0)
        {
            foreach (Neuron neuron in neuronData)
            {
                Destroy(neuron.render);
            }
            foreach (Link link in linkData)
            {
                Destroy(link.render);
            }
        }

        neuronData = new();
        linkData = new();
        cm.transform.GetComponent<CameraMovement>().width.Clear();
        cm.transform.GetComponent<CameraMovement>().links.Clear();

        float x = startX;
        float y;
        float layerSize = 0;
        float biggestLayer = 0;
        float secondBiggestSize = 0;
        neuronDistanceX = width / network.Length;

        foreach (Layer layer in network)
        {
            if (layer.neuronCount > layerSize)
            {
                layerSize = layer.neuronCount;
                biggestLayer = layer.layerId;
            }
        }

        foreach (Layer layer in network)
        {
            if (layer.neuronCount > secondBiggestSize && layer.neuronCount != layerSize)
            {
                secondBiggestSize = layer.neuronCount;
            }
        }

        //Create neurons by layer
        foreach (Layer layer in network)
        {
            neuronDistanceY = height / (secondBiggestSize - 1);
            if (layer.layerId == biggestLayer)
            {
                neuronDistanceY = height / (layerSize - 1);
            }

            float layerDistance = 0;
            if (layer.neuronCount != layerSize)
            {
                if (layer.neuronCount % 2 > 0)
                {
                    layerDistance = (height / 2) - (float)Math.Floor((double)layer.neuronCount / 2) * neuronDistanceY;
                }
                else
                {
                    layerDistance = (height / 2) - ((float)Math.Floor((double)layer.neuronCount / 2) - .5f) * neuronDistanceY;
                }
            }
            y = (float)(startY - layerDistance);

            //Neurons from layer
            for (int neuronIndex = 0; neuronIndex < layer.neuron.Count; neuronIndex++)
            {
                Neuron neuron = layer.neuron[neuronIndex];

                //Separating neurons
                if (neuronIndex != 0)
                {
                    y -= neuronDistanceY;
                }

                //Create neuron gameobject
                neuron.render = DrawCircle(new Vector3(x, y, 0), neuron.neuronId);

                //Save neuron to use later
                neuronData.Add(neuron);

                //Red if activated
                if (neuron.output > 0 || neuron.layer.layerId == 0)
                {
                    neuron.render.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                }
            }

            //Links from layer
            for (int linkIndex = 0; linkIndex < layer.link.Count; linkIndex++)
            {
                Link link = layer.link[linkIndex];

                //Saves links for later
                linkData.Add(link);
            }

            //Separate neurons
            x += neuronDistanceX;
            built = true;
        }

        //Create links
        int index = 0;
        cm.transform.GetComponent<CameraMovement>().width.Clear();
        foreach (Link link in linkData)
        {
            float width = Width(index);
            cm.transform.GetComponent<CameraMovement>().width.Add(width);

            //Save render object in the link
            link.render = DrawLine(link.neuron1.render.transform, link.neuron2.render.transform, new(0, 0, 0), width, link.linkId);
            index++;
        }
        cm.transform.GetComponent<CameraMovement>().links = linkData;
    }

    private float Width(int index)
    {
        //Width is based on the weight of the link
        float weight = Mathf.Abs(linkData[index].weight / weightLimit);
        float width = 0.04f * weight;
        if (width < 0.02f) { width = 0.02f; }

        return width;
    }

    private GameObject DrawLine(Transform start, Transform end, Color color, float width, int linkId)
    {
        GameObject myLine = new() { name = "Link " + linkId.ToString() };
        myLine.AddComponent<LineRenderer>();
        myLine.transform.parent = transform;
        myLine.transform.localPosition = start.localPosition;
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.sortingOrder = 0;
        lr.sortingLayerName = "UI";
        lr.material = new Material(material);
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width *= cm.orthographicSize / 5;
        lr.endWidth = width *= cm.orthographicSize / 5;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, end.position - start.position);
        return myLine;
    }

    private GameObject DrawCircle(Vector3 position, int neuronId)
    {
        GameObject neuron = Instantiate(neuronCircle);
        neuron.transform.parent = transform;
        neuron.transform.localPosition = position;
        neuron.transform.localScale *= cm.orthographicSize / 5;
        neuron.name = "Neuron " + neuronId.ToString();
        neuron.transform.Find("Canvas").GetComponent<Canvas>().overrideSorting = true;
        return neuron;
    }
}
