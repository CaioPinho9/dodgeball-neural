using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class NeuralNetwork
{
    //Neural Setting
    public static int[] neuronsLayer;
    //How big is the network
    public int lastLayer;

    //Each layer have neurons and links
    public Layer[] layer;

    //String with all weights and bias
    public string dna;

    //Max value of weights and bias
    public static int weightLimit;
    //Max value of mutation
    public int mutate;
    public float learningRate;

    public int team;
    private readonly Controller controller;
    public static List<List<(int, int)>> freeLinks = new();


    public NeuralNetwork(int team)
    {
        controller = GameObject.Find("Controller").GetComponent<Controller>();

        neuronsLayer = controller.neuronsLayer;
        lastLayer = neuronsLayer.Length;
        layer = new Layer[neuronsLayer.Length];
        weightLimit = controller.weightLimit;
        mutate = controller.mutate;
        learningRate = controller.learningRate;

        freeLinks.Clear();
        List<List<int[]>> neuronBlocks = NeuronBlocks();

        for (int i = 0; i < neuronBlocks.Count; i += 2)
        {
            freeLinks.Add(FreeLinks(neuronBlocks[i], neuronBlocks[i + 1]));
        }

        this.team = team;
        for (int i = 0; i < neuronsLayer.Length; i++)
        {
            layer[i] = new()
            {
                //How many neurons in this layer
                neuronCount = neuronsLayer[i],
                //Which layer is this
                layerId = i,
                //Range of link weight
                randomStart = weightLimit,
                //How long is the network
                networkSize = neuronsLayer.Length
            };
        }
        //Create all neurons than create links
        CreateNeurons();
        LinkLayers();
        //Show current dna in player
        dna = Copy();
    }

    private List<List<int[]>> NeuronBlocks()
    {
        List<int[]> neuronBlocksIn1 = new()
        {
            new int[] { 0, 4, 5, 10, 11, 12, 13, 14, 15, 16 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 6, 7, 8, 9 }
        };
        List<int[]> neuronBlocksOut1 = new()
        {
            new int[] { 6, 7, 8 },
            new int[] { 0, 1, 2 },
            new int[] { 3, 4, 5 }
        };

        List<int[]> neuronBlocksIn2 = new()
        {
            new int[] { 0, 1, 2, 3, 4, 5 },
            new int[] { 6, 7, 8 },
        };
        List<int[]> neuronBlocksOut2 = new()
        {
            new int[] { 0, 1, 2, 3, 4, 5 },
            new int[] { 0, 1, 2, 3, 4, 5, 6 },
        };

        List<List<int[]>> neuronBlocks = new()
        {
            neuronBlocksIn1,
            neuronBlocksOut1,
            neuronBlocksIn2,
            neuronBlocksOut2,
        };
        return neuronBlocks;
    }

    private List<(int, int)> FreeLinks(List<int[]> neuronBlocks1, List<int[]> neuronBlocks2)
    {
        List<(int, int)> freeLinks = new();
        for (int i = 0; i < neuronBlocks1.Count; i++)
        {
            foreach (int neuron1 in neuronBlocks1[i])
            {
                foreach (int neuron2 in neuronBlocks2[i])
                {
                    freeLinks.Add((neuron1, neuron2));
                }
            }
        }
        return freeLinks;
    }

    public List<float> RunNetwork(ArrayList input)
    {
        //Reset outputs
        Clear();
        //Receive new information
        Input(input);
        //Calculate the next move
        Forward();

        //Get output from neurons in output layer
        List<float> output = new();
        foreach (Neuron neuron in layer[lastLayer - 1].neuron)
        {
            output.Add(neuron.output);
        }
        //Return to player
        return output;
    }

    public string Copy()
    {
        dna = "";
        //Weight/Bias;Weight/Bias;Weight/Bias;
        //";" separates different links, and "/" separates weight and bias
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                //Create string with all weights and bias
                dna += link.weight.ToString("0.00") + "/" + link.bias.ToString("0.00") + ";";
            }
        }
        return dna;
    }

    public void Paste(string dna)
    {
        string[] rna = dna.Split(";");
        int index = 0;
        //Weight/Bias;Weight/Bias;Weight/Bias;
        //";" separates different links, and "/" separates weight and bias
        //Iterates and set weights and bias
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                string[] gene = rna[index].Split("/");
                link.weight = float.Parse(gene[0]);
                link.bias = float.Parse(gene[1]);
                index++;
            }
        }
    }

    public void Mutate()
    {
        dna = "";
        //Iterates the network
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                //Mutates using a random number, mutate = max value of mutation
                MutateLink(link, RandomNumber(mutate) * learningRate, RandomNumber(mutate) * learningRate);
            }
        }
        //Show current dna in player
    }

    public void MutateLink(Link link, float randomW, float randomB)
    {
        //Limits the max of a weight 
        if (link.weight < weightLimit && link.weight > -weightLimit)
        {
            link.weight += randomW;
        }
        else if (link.weight >= weightLimit)
        {
            link.weight -= Math.Abs(randomW);
        }
        else
        {
            link.weight += Math.Abs(randomW);
        }

        //Limits the max of a bias 
        if (link.bias < mutate && link.bias > -mutate)
        {
            link.bias += randomB;
        }
        else if (link.weight >= mutate)
        {
            link.bias -= Math.Abs(randomB);
        }
        else
        {
            link.bias += Math.Abs(randomB);
        }

        //Copy
        dna += link.weight.ToString("0.00") + "/" + link.bias.ToString("0.00") + ";";
    }

    public void Random()
    {
        dna = "";
        //Creates a random network, same as start
        foreach (Layer layer in layer)
        {
            foreach (Link link in layer.link)
            {
                float random = RandomNumber(weightLimit);
                link.weight = random;

                random = RandomNumber(weightLimit);
                link.bias = random;
                dna += link.weight.ToString("0.00") + "/" + link.bias.ToString("0.00") + ";";
            }
        }
    }

    public static float RandomNumber(float limit)
    {
        //Creates a float number with 2 decimals
        return (float)UnityEngine.Random.Range(-limit, limit) + (float)UnityEngine.Random.Range(-100, 100) / 100;
    }

    public void CreateNeurons()
    {
        int neuronId = 0;
        //Create neurons to each layer
        foreach (Layer layer in layer)
        {
            neuronId = layer.CreateNeurons(neuronId);
        }
    }

    public void LinkLayers()
    {
        int linkId = 0;
        //Layer 1 connects with 2, etc
        for (int index = 0; index < layer.Length - 1; index++)
        {
            linkId = layer[index].LinkNeurons(linkId, layer[index], layer[index + 1]);
        }
    }

    public void Clear()
    {
        //Iterates the network and set outputs to 0
        foreach (Layer layer in layer)
        {
            if (layer.layerId > 0)
            {
                //Reset neuron input
                for (int index = 0; index < layer.neuron.Count; index++)
                {
                    layer.neuron[index].input = 0;
                    layer.neuron[index].output = 0;
                }
            }
        }
    }

    public void Input(ArrayList input)
    {
        //totalIndex is inputIndex + sensors arrays indexs
        //Ex: x,x,x,x,x,[x,x,x],[x,x,x],[x,x,x] inputIndex = 8, totalIndex = 14
        int totalIndex = 0;
        for (int inputIndex = 0; inputIndex < input.Count; inputIndex++)
        {
            if (!input[inputIndex].GetType().IsArray)
            {
                //A neuron in input layer receives an input value
                layer[0].neuron[inputIndex].output = (float)input[inputIndex];
                totalIndex++;
            }
            else
            {
                //Iterating the sensors
                float[] array = (float[])input[inputIndex];
                for (int arrayIndex = 0; arrayIndex < array.Length; arrayIndex++)
                {
                    //A neuron in input layer receives an input value
                    layer[0].neuron[totalIndex].output = array[arrayIndex];
                    totalIndex++;
                }
            }
        }
    }

    public void Forward()
    {
        //Each layer
        for (int index = 0; index < layer.Length; index++)
        {
            layer[index].Forward(layer[index]);
        }
    }
}

public class Layer
{
    //How many neurons there's in this layer
    public int neuronCount;
    //Id
    public int layerId;
    //Last layer
    public int networkSize;
    //Neurons in this layer
    public List<Neuron> neuron = new();
    //Connections that start in this layer
    public List<Link> link = new();
    public int randomStart;

    public Layer() {}

    public int CreateNeurons(int neuronId)
    {
        //Create neurons, limiting to how many must be
        for (int index = neuron.Count; index < neuronCount; index++)
        {
            neuron.Add(new(this, neuronId));
            neuronId++;
        }
        return neuronId;
    }

    public int LinkNeurons(int linkId, Layer thisLayer, Layer nextLayer)
    {
        List<(int, int)> freeLinks = new();

        freeLinks = NeuralNetwork.freeLinks[thisLayer.layerId];
        

        //Iterates to connect every neuron in layer 1 with each neuron in layer 2
        for (int thisIndex = 0; thisIndex < thisLayer.neuronCount; thisIndex++)
        {
            for (int nextIndex = 0; nextIndex < nextLayer.neuronCount; nextIndex++)
            {
                bool notBlocked = false;
                foreach ((int,int) free in freeLinks)
                {
                    if (thisIndex == free.Item1 && nextIndex == free.Item2)
                    {
                        notBlocked = true;
                    }
                }

                if (notBlocked)
                {
                    //Create link between neurons
                    link.Add(new Link(thisLayer, linkId, thisLayer.neuron[thisIndex], nextLayer.neuron[nextIndex], UnityEngine.Random.Range(-randomStart, randomStart), UnityEngine.Random.Range(-randomStart, randomStart)));
                    linkId++;
                }
            }
        }
        return linkId;
    }
    public void Forward(Layer layer)
    {
        //Input layer is always active
        if (layer.layerId > 0)
        {
            for (int index = 0; index < layer.neuron.Count; index++)
            {
                //If input is greater than 0 the information is passed on
                layer.neuron[index].output = layer.neuron[index].ReLU();
            }
        }

        //Output layer isn't weighted
        if (layer.layerId < networkSize)
        {
            for (int index = 0; index < layer.link.Count; index++)
            {
                //Each neuron is a sum of all neurons outputs in the last layer
                layer.link[index].neuron2.input += layer.link[index].Weight();
            }
        }
    }

}

public class Link
{
    //UI reference
    public GameObject render;

    //Beginning of link
    public Neuron neuron1;
    //Ending
    public Neuron neuron2;

    //Where this link is located
    public Layer layer;
    public int linkId;

    //Used to proccess data
    public float weight;
    public float bias;
    public Link(Layer layer, int linkId, Neuron neuron1, Neuron neuron2, float weight, float bias)
    {
        this.layer = layer;
        this.linkId = linkId;
        this.neuron1 = neuron1;
        this.neuron2 = neuron2;
        this.weight = weight;
        this.bias = bias;
    }

    public float Weight()
    {
        //Weight and bias are used to manipulate the data
        return neuron1.output * weight + bias;
    }
}
public class Neuron
{
    //UI reference
    public GameObject render;

    //Where this neuron is located
    public Layer layer;
    public int neuronId;

    //Input and output
    public float input = 0;
    public float output = 0;

    public Neuron(Layer layer, int neuronId)
    {
        this.layer = layer;
        this.neuronId = neuronId;
    }

    //Rectifier
    public float ReLU()
    {
        //Activate if input > 0
        if (input > 0)
        {
            if (input < 10000)
            {
                return input;
            }
            else
            {
                return 10000;
            }
        }
        return 0;
    }
}
