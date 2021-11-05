using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientManager
{
    private Gradient _gradient;
    private static GradientManager _instance;

    private static GradientManager Instance
    {
        get { return _instance ??= new GradientManager(); }
    }
    public static Gradient Gradient
    {
        get
        {
            var instance = Instance;

            if(instance._gradient == null)
                instance.createHeatMapGradient();
            return instance._gradient;
        }
    }

    public void createHeatMapGradient()
    {
        _gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        var colorKey = new GradientColorKey[5];
        colorKey[0].color = Color.black;
        colorKey[0].time = 0.0f;

        colorKey[1].color = new Color(148.0f / 255, 0, 211.0f / 255);
        colorKey[1].time = 0.4f;

        colorKey[2].color = Color.red;
        colorKey[2].time = 0.6f;

        colorKey[3].color = Color.yellow;
        colorKey[3].time = 0.8f;

        colorKey[4].color = Color.white;
        colorKey[4].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        var alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        _gradient.SetKeys(colorKey, alphaKey);
    }
}
