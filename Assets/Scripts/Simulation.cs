using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation
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

    public double CentralTemperature { get; set; } = 0;

    void fillBoundaryCondition(double[,] heatMap)
    {
        for (int i = 0; i < Nr; i++)
        {
            for (int j = 0; j < NAlpha; j++)
            {
                heatMap[i, j] = (Math.Sin(Math.PI * 4.0 * j / (NAlpha) + Math.PI * 0.5) + 1.0) * Math.Pow((double) i/(Nr - 1.0), 16.0);
            }
        }

        for (int j = 0; j < NAlpha; j++)
        {
            heatMap[0, j] = CentralTemperature;//Math.Sin(Math.PI * 2.0 * j / (NAlpha));
        }
    }

    double getValue(double[,] heatMap, int r, int angle)
    {
        if (angle < 0)
        {
            angle = NAlpha - angle;
        }

        angle = angle % (NAlpha - 1);

        return heatMap[r, angle];
    }

    private double d;

    double nextPoint(int r, int angle, double[,] heatMap)
    {
        var m = heatMap;
        double r_i = dr * r;
        double derivative = 0;

        double result = heatMap[r, angle];
        if (r > 0)
        {
            derivative = (getValue(m, r - 1, angle) - 2 * getValue(m, r, angle) + getValue(m, r + 1, angle))/(dr * dr) +
                         (1.0 / r_i) * (getValue(m, r + 1, angle) - getValue(m, r - 1, angle)) / (2.0 * dr) + 
                         1.0/(r_i * r_i) * (getValue(m, r, angle + 1) - 2.0 * getValue(m, r, angle) + getValue(m, r, angle - 1)) / (dalpha * dalpha);
        }

        result += alpha * dt * derivative;

        return result;
        if (r > 0)
        {
            derivative =
                dr / (2 * r_i) * (getValue(m, r + 1, angle) - getValue(m, r - 1, angle)) +
                1.0 / (r_i * r_i) * (dr * dr) / (dalpha * dalpha) * (getValue(m, r, angle - 1) -
                    2.0 * getValue(m, r, angle) + getValue(m, r, angle + 1));
        }

        result += d * (
            (getValue(m, r - 1, angle) - 2.0 * getValue(m, r, angle) + getValue(m, r + 1, angle))
            + derivative);

        return result;
    }

    public List<double[,]> solve()
    {
        dt = endT / (Nt - 1);
        dr = R / (Nr - 1);
        dalpha = 360.0 / (NAlpha);

        d = alpha * dt / (dr * dr);

        //heatMap = [N_R, N_T]
        //      ...
        //    End Time

        List<double[,]> heatMap = new List<double[,]>();
        heatMap.Add(new double[Nr, NAlpha]);

        fillBoundaryCondition(heatMap[0]);


        for (int i = 0; i < Nt; i++)
        {
            var current = new double[Nr, NAlpha];

            //boundary is fixed so Nr - 1
            for (int r = 1; r < Nr - 1; r++)
            {
                for (int angle = 0; angle < NAlpha; angle++)
                {
                    current[r, angle] = nextPoint(r, angle, heatMap[i]);
                }
            }

            for (int angle = 0; angle < NAlpha; angle++)
            {
                current[0, angle] = heatMap[i][0, angle];

                current[Nr - 1, angle] = heatMap[i][Nr - 1, angle];
            }

            heatMap.Add(current);
        }

        return heatMap;
    }
}