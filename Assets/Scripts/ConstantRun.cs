using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConstantRun : MonoBehaviour
{
    public GameObject quad;
    public GameObject heightMapTemplate;
    public Transform parentContainer;

    public Slider centralTSlider;
    public Slider angleStepsSlider;
    public Slider radialStepsSlider;
    public Slider timeStepsSlider;

    public Slider progressSlider;

    public TextMeshPro radialSegmentsText;
    public TextMeshPro angularSegmentsText;
    public TextMeshPro centralTText;
    public TextMeshPro timeStepsText;

    Simulation sim = new Simulation();

    private Texture2D texture;


    double min = double.MaxValue;
    double max = double.MinValue;

    private FilterMode mode = FilterMode.Point;

    void Start()
    {
        angleStepsSlider.minValue = radialStepsSlider.minValue = 3;
        angleStepsSlider.maxValue = radialStepsSlider.maxValue = 200;

        sim.R = 1;
        sim.endT = 1;
        sim.Nt = 10000;
        sim.Nr = 10;
        sim.NAlpha = 10;

        sim.solve();

        radialSegmentsText.text = sim.Nr.ToString();
        angularSegmentsText.text = sim.NAlpha.ToString();
        timeStepsText.text = sim.Nt.ToString();

        radialStepsSlider.value = sim.Nr;
        angleStepsSlider.value = sim.NAlpha;
        centralTSlider.maxValue = sim.Nt;


        angleStepsSlider.onValueChanged.AddListener(delegate { AngleStepsChanged(); });
        radialStepsSlider.onValueChanged.AddListener(delegate { RadialStepsChanged(); });
        timeStepsSlider.onValueChanged.AddListener(delegate { TimeStepsChanged(); });
        centralTSlider.onValueChanged.AddListener(delegate { setCentralT(); });

        sim.init();

        timeStepsSlider.value = (float)sim.dt;
        timeStepsSlider.maxValue = 0.0001f;
        timeStepsText.text = sim.dt.ToString("0.000000");

        centralTSlider.maxValue = 2;
        centralTSlider.value = (float) sim.CentralTemperature;
        centralTText.text = sim.CentralTemperature.ToString("0.0000");

        updateMinMax();
        createTexture();
    }

    private void setCentralT()
    {
        sim.CentralTemperature = centralTSlider.value;
        centralTText.text = sim.CentralTemperature.ToString("0.0000");
    }

    private void TimeStepsChanged()
    {
        sim.dt = timeStepsSlider.value;
        timeStepsText.text = sim.dt.ToString("0.000000");
    }
    private void RadialStepsChanged()
    {
        sim.Nr = (int)radialStepsSlider.value;
        radialSegmentsText.text = sim.Nr.ToString();

        updateEverything();
    }

    private void AngleStepsChanged()
    {
        sim.NAlpha = (int)angleStepsSlider.value;
        angularSegmentsText.text = sim.NAlpha.ToString();

        updateEverything();
    }


    private void updateEverything()
    {
        sim.init();

        createTexture();
    }

    private void updateMinMax()
    {
        min = double.MaxValue;
        max = double.MinValue;
        var cur = sim.heatMap[0];
        int i = sim.Nr - 1;
        {
            for (int y = 0; y < sim.NAlpha; y++)
            {
                if (!double.IsNaN(cur[i, y]))
                {
                    min = Math.Min(cur[i, y], min);
                    max = Math.Max(cur[i, y], max);
                }
            }
        }
    }

    private void createTexture()
    {
        if (texture != null)
        {
            texture.Resize(sim.Nr, sim.NAlpha);
        }
        else
        {
            texture = new Texture2D(sim.Nr, sim.NAlpha);
        }
    }

    private static void writeToTexture(double[,] current, Texture2D texture, double min, double max, Gradient gradient)
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                float t = (float) ((current[x, y] - min) / (max - min));

                Color color =
                    float.IsNaN(t) ? Color.green : gradient.Evaluate(t); // ((x + y)%2 != 0 ? Color.white : Color.red);
                texture.SetPixel(x, y, color);
            }
        }
    }

    private void updateTexture(double[,] current, Gradient gradient)
    {
        writeToTexture(current, texture, min, max, gradient);

        texture.filterMode = mode;
        texture.wrapModeU = TextureWrapMode.Mirror;
        texture.wrapModeV = TextureWrapMode.Repeat;

        texture.Apply(true);
        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        Renderer heightRenderer = heightMapTemplate.GetComponent<Renderer>();
        heightRenderer.material.mainTexture = texture;
    }


    private void setValue(int value)
    {
        if (value < sim.heatMap.Count)
        {
            updateMinMax();

            CircleMeshGenerator.generateCircleHeightOnGO(heightMapTemplate, sim.heatMap[value], min, max, 10,
                sim.Nr - 1, sim.NAlpha);

            updateTexture(sim.heatMap[value], GradientManager.Gradient);
        }
        else
        {
            Debug.LogWarning("Unable " + value.ToString());
        }
    }

    private double maxDelayMs = 10;

    void Update()
    {
        var startTime = Time.realtimeSinceStartupAsDouble;
        for (int i = 0; i < 15; i++)
        {
            sim.simStep();
            var timeNow = Time.realtimeSinceStartupAsDouble;
            if (timeNow - startTime > maxDelayMs * 1000.0)
                break;
        }

        setValue(sim.heatMap.Count - 1);
    }
}