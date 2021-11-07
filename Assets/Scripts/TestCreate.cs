using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class TestCreate : MonoBehaviour
{
    public GameObject quad;
    public GameObject circleTemplate;
    public Transform parentContainer;

    public Slider currentTimeSlider;
    public Slider angleStepsSlider;
    public Slider radialStepsSlider;

    public TextMeshPro radialSegmentsText;
    public TextMeshPro angularSegmentsText;
    public TextMeshPro timeText;

    private List<double[,]> res;
    Simulation sim = new Simulation();
    private Texture2D texture;

    double min = double.MaxValue;
    double max = double.MinValue;

    private FilterMode mode = FilterMode.Point;
    private int currentStep = 0;
    void Start()
    {
        angleStepsSlider.minValue = radialStepsSlider.minValue = 3;
        angleStepsSlider.maxValue = radialStepsSlider.maxValue = 100;

        sim.R = 1;
        sim.endT = 1;
        sim.Nt = 10000;
        sim.Nr = 10;
        sim.NAlpha = 10;

        sim.solve();
        res = sim.heatMap;

        radialSegmentsText.text = sim.Nr.ToString();
        angularSegmentsText.text = sim.NAlpha.ToString();

        radialStepsSlider.value = sim.Nr;
        angleStepsSlider.value = sim.NAlpha;
        currentTimeSlider.maxValue = sim.Nt;

        currentTimeSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        angleStepsSlider.onValueChanged.AddListener(delegate { AngleStepsChanged(); });
        radialStepsSlider.onValueChanged.AddListener(delegate { RadialStepsChanged(); });

        updateEverything();

    }

    private void updateCurrentTime()
    {
        timeText.text = (currentStep * sim.dt).ToString("0.00000");
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
        //sim.solve();
        sim.init();
        res = sim.heatMap;
        updateMinMax();
        createTexture();

        CircleMeshGenerator.generateCircleOnGO(circleTemplate, 10, sim.Nr - 1, sim.NAlpha);
        setValue(currentStep);
        
        double stab = (sim.alpha * sim.dt / (sim.dr * sim.dr) + sim.alpha * sim.dt / (sim.dalpha * sim.dalpha));
        bool stable = stab < 0.5;
        //Debug.Log(stab +" " + stable);
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

    IEnumerator runSimulation()
    {


        yield break;
    }

    private void updateMinMax()
    {
        min = double.MaxValue;
        max = double.MinValue;
        //for (int i = 0; i < res.Count; i++)
        int i = 0; //(int) currentTimeSlider.value;
        {
            var cur = res[i];
            for (int x = 0; x < sim.Nr; x++)
            {
                for (int y = 0; y < sim.NAlpha; y++)
                {
                    if (!double.IsNaN(cur[x, y]))
                    {
                        min = Math.Min(cur[x, y], min);
                        max = Math.Max(cur[x, y], max);
                    }
                }
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

        Renderer rendererObj = circleTemplate.GetComponent<Renderer>();
        rendererObj.material.mainTexture = texture;
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


    public void ValueChangeCheck()
    {
        var value = (int) currentTimeSlider.value;

        //Debug.Log(value);
        setValue(value);
    }

    private void setValue(int value)
    {
        if (value < res.Count)
        {
            currentStep = value;
            updateTexture(res[value], GradientManager.Gradient);

            updateCurrentTime();
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
        for (int i = 0; i < 10; i++)
        {
            if (sim.steps < sim.Nt)
            {
                sim.simStep();
                var timeNow = Time.realtimeSinceStartupAsDouble;
                if(timeNow - startTime > maxDelayMs * 1000.0)
                    break;
            }
            else
            {
                break;
            }
        }
        if (sim.steps < (int)currentTimeSlider.value)
        {
            setValue(sim.steps);
        }
        else
        {
            setValue(currentStep);
        }
    }
}