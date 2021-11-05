using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


class Simulation
{
    public double alpha = 1;
    public double dt;
    public double dr;
    public double dalpha;

    public double endT { get; set; }
    public double R { get; set; }

    public int Nt { get; set; }
    public int Nr { get; set; }

    public int NAlpha;

    void fillBoundaryCondition(double[,] heatMap)
    {
        for (int i = 0; i < Nr; i++)
        {
            for (int j = 0; j < NAlpha; j++)
            {
                heatMap[i, j] = 0; //hz what has to be here let it be 0, idk
            } 
        }

        for (int j = 0; j < NAlpha; j++)
        {
            heatMap[Nr - 1, j] = Math.Sin(Math.PI * 2.0 * j / (NAlpha));
        }
    }

    double getValue(double[,] heatMap, int r, int angle)
    {
        r = Math.Abs(r);
        if (angle < 0)
        {
            angle = NAlpha - angle;
        }

        angle = angle % NAlpha;

        return heatMap[r, angle];
    }
    private double d;
    double nextPoint(int r, int angle, double[,] heatMap)
    {
        var m = heatMap;
        double result = heatMap[r, angle];
        double r_i = dr * r;
        double derivative = 0;
        if (r > 0)
        {
            derivative =
                dr / (2 * r_i) * (getValue(m, r + 1, angle) - getValue(m, r - 1, angle)) +
                1.0 / (r_i * r_i) * (dr * dr) / (dalpha * dalpha) * (getValue(m, r, angle - 1) -
                    2.0 * getValue(m, r, angle) + getValue(m, r, angle + 1));
        }
        result += d * (
            (
                getValue(m, r - 1, angle) - 2.0 * getValue(m, r, angle) + getValue(m, r + 1, angle)) + derivative);

        return result;
    }
    public List<double[,]> solve()
    {
        dt = endT / (Nt - 1);
        dr = R / (Nr - 1);
        dalpha = 2 * Math.PI / (NAlpha);

        //heatMap = [N_R, N_T]
        //      ...
        //    End Time
        
        List<double[,]> heatMap = new List<double[,]>();
        heatMap.Add(new double[Nr, NAlpha]);

        fillBoundaryCondition(heatMap[0]);

        d = alpha * dt / (dr * dr);
        
        for (int i = 0; i < Nt; i++)
        {
            var current = new double[Nr, NAlpha];

            //boundary is fixed so Nr - 1
            for (int r = 0; r < Nr - 1; r++)
            {
                for (int angle = 0; angle < NAlpha; angle++)
                {
                    current[r, angle] = nextPoint(r, angle, heatMap[i]);
                }
            }

            for (int angle = 0; angle < NAlpha; angle++)
            {
                current[Nr - 1, angle] = heatMap[i][Nr - 1, angle];
            }

            heatMap.Add(current);
        }
        return heatMap;
    }
}
public class TestCreate : MonoBehaviour
{
    public GameObject quad;
    public Slider slider;
    private List<double[,]> res;
    Simulation sim = new Simulation();
    private Texture2D texture;

    private GameObject obj;

    double min = double.MaxValue;
    double max = double.MinValue;

    private FilterMode mode = FilterMode.Point;
    void Start()
    {

        sim.R = 0.2;
        sim.endT = 0.1;
        sim.Nt = 5000;
        sim.Nr = 10;
        sim.NAlpha = 15;

        res  = sim.solve();

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
        


        texture = new Texture2D(sim.Nr, sim.NAlpha);

        obj = CircleMeshGenerator.createCircleMesh(10, sim.Nr - 1, sim.NAlpha);

        var current = res[0];
        updateTexture(current, GradientManager.Gradient);

        slider.maxValue = sim.Nt;

        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    private void updateTexture(double[,] current, Gradient gradient)
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

        texture.filterMode = mode;
        texture.wrapModeU = TextureWrapMode.Mirror;
        texture.wrapModeV = TextureWrapMode.Repeat;
        texture.Apply();

        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        Renderer rendererObj = obj.GetComponent<Renderer>();
        rendererObj.material.mainTexture = texture;
    }


    public void ValueChangeCheck()
    {
        int value = (int) slider.value;

        Debug.Log(value);
        updateTexture(res[value], GradientManager.Gradient);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
