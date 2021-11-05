using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;



public class TestCreate : MonoBehaviour
{
    public GameObject quad;
    public Transform parentContainer;

    [FormerlySerializedAs("slider")] public Slider currentTimeSlider;
    private List<double[,]> res;
    Simulation sim = new Simulation();
    private Texture2D texture;

    private GameObject obj;

    double min = double.MaxValue;
    double max = double.MinValue;

    private FilterMode mode = FilterMode.Point;

    void Start()
    {
        currentTimeSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        sim.R = 0.2;
        sim.endT = 0.1;
        sim.Nt = 5000;
        sim.Nr = 10;
        sim.NAlpha = 15;

        res  = sim.solve();

        updateMinMax();
        
        createTexture();

        
        obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.transform.SetParent(parentContainer, false);

        CircleMeshGenerator.generateCircleOnGO(obj, 10, sim.Nr - 1, sim.NAlpha);

        updateTexture(res[0], GradientManager.Gradient);

        currentTimeSlider.maxValue = sim.Nt;

    }

    private void createTexture()
    {
        texture = new Texture2D(sim.Nr, sim.NAlpha);
    }

    private void updateMinMax()
    {
        for (int i = 0; i < res.Count; i++)
        {
            var cur = res[i];
            for (int x = 0; x < sim.Nr; x++)
            {
                for (int y = 0; y < sim.NAlpha; y++)
                {
                    min = Math.Min(cur[x, y], min);
                    max = Math.Max(cur[x, y], max);
                }
            }
        }
    }

    private void updateTexture(double[,] current, Gradient gradient)
    {
        writeToTexture(current, gradient);

        texture.filterMode = mode;
        texture.wrapModeU = TextureWrapMode.Mirror;
        texture.wrapModeV = TextureWrapMode.Repeat;
        texture.Apply();

        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        Renderer rendererObj = obj.GetComponent<Renderer>();
        rendererObj.material.mainTexture = texture;
    }

    private void writeToTexture(double[,] current, Gradient gradient)
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color color =
                    gradient.Evaluate((float) ((current[x, y] - min) /
                                               (max - min))); // ((x + y)%2 != 0 ? Color.white : Color.red);
                texture.SetPixel(x, y, color);
            }
        }
    }


    public void ValueChangeCheck()
    {
        int value = (int) currentTimeSlider.value;

        Debug.Log(value);
        updateTexture(res[value], GradientManager.Gradient);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
