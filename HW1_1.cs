using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DipoleElectricFieldGrapher : MonoBehaviour
{
    #region Public Variables
    public GameObject Positive_Charge; // GameObject representing the positive charge
    public GameObject Negative_Charge; // GameObject representing the negative charge
    public int num_field_lines = 1;    // Number of field lines to draw
    public int num_points = 1000;      // Number of points to plot per field line
    public double[] Xinit_points = new double[1]; // Array of init point x values
    public double[] Yinit_points = new double[1]; // Array of init point y values
    public float delta_s = 0.1f;                  // Value to interate by
    public GameObject point;                      // GameObject that is placed at field line point
    #endregion

    #region Private Variables
    private double x0p = 0, y0p = 0, x0n = 0, y0n =0; // Variables for Positive and Negative charge coordinates
    #endregion

    void Start()
    {
        Setup();
        PlotField();
    }


    /// <summary>
    /// Debug printout for charge locations
    /// Sets charge location variables based on GameObjedct locations
    /// </summary>
    private void Setup()
    {    
        Debug.Log(string.Format("Positive Charge Location : " + "( {0} , {1} , {2})", Positive_Charge.transform.position.x, Positive_Charge.transform.position.y, Positive_Charge.transform.position.z));
        Debug.Log(string.Format("Negative Charge Location : " + "( {0} , {1} , {2})", Negative_Charge.transform.position.x, Negative_Charge.transform.position.y, Negative_Charge.transform.position.z));
        x0p = Positive_Charge.transform.position.x;
        y0p = Positive_Charge.transform.position.y;
        x0n = Negative_Charge.transform.position.x;
        y0n = Negative_Charge.transform.position.y;
    }

    /// <summary>
    /// Plots Electic field for dipole using Positive and Negative charge game object locations
    /// </summary>
    private void PlotField()
    {
        double[] Pos_x = new double[num_points]; // Array of x coordinates for all points on field line
        double[] Pos_y = new double[num_points]; // Array of y coordinates for all points on field line
        double dxds = 0, dyds = 0;               // Values for Ex/E for Forward Euler Estimation


        // Loop to generate single field line per interation
        for (int i = 0; i < num_field_lines; i++)
        {
            // Inital point for field line set from array
            Pos_x[0] = Xinit_points[i];
            Pos_y[0] = Yinit_points[i];

            // Loop to generate point of field line per iteration
            for (int j = 1; j< num_points; j++)
            {
                // Get Ex/E and Ey/E for Forward Euler Estimation
                dxds = Calc_dxds(Pos_x[j - 1], Pos_y[j - 1], x0p, y0p, x0n, y0n);
                dyds = Calc_dxds(Pos_y[j - 1], Pos_x[j - 1], y0p, x0p, y0n, x0n);

                // Determine next plot points using Forward Euler Estimation
                Pos_x[j] = ForwardEuler(Pos_x[j - 1], delta_s, dxds);
                Pos_y[j] = ForwardEuler(Pos_y[j - 1], delta_s, dyds);
            }
            
            // Debug printout of each points coordinates
            foreach (var x in Pos_x) Debug.Log(x.ToString());
            foreach (var y in Pos_y) Debug.Log(y.ToString());

            // Plot points
            PlacePoints(Pos_x, Pos_y);
        }
    }

    /// <summary>
    /// Place points from arrays of coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void PlacePoints(double[] x, double[] y)
    {
        // Loop through all coordinates
        for (int i = 0; i < num_points; i++)
        {
            // Instantiate GameObject attached to point to place point in scene
            GameObject points;
            points = (GameObject)(Instantiate(point, new Vector3((float)x[i], (float)y[i], 0), Quaternion.identity));
        }
    }

    /// <summary>
    /// Ex/E = Ex / Sqrt(Ex^2 + Ey^2)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="x0"></param>
    /// <param name="y0"></param>
    /// <returns></returns>
    private double Calc_dxds(double x, double y, double x0p, double y0p, double x0n, double y0n)
    {
        double value = 0;  // Return value
        double pos_eqx = 0, pos_eqy = 0, neg_eqx = 0, neg_eqy = 0; // Variables for electric field components in x and y direction from pos and neg charges
        double total_eqx = 0, total_eqy = 0; // Variables for total field components in x and y from both pos and neg charges

        // Get electric field components from pos and neg charges coordinates x,y
        Eq(x, y, x0p, y0p, out pos_eqx, out pos_eqy, 1);
        Eq(x, y, x0n, y0n, out neg_eqx, out neg_eqy, -1);
        total_eqx = pos_eqx + neg_eqx;
        total_eqy = pos_eqy + neg_eqy;

        // Retrun value of Ex/E (or Ey/E)
        value = (total_eqx) / Mag(total_eqx, total_eqy);
        return value;
    }

    /// <summary>
    /// Eq_plus at point (x,y) due to charge at point (x0, y0)
    /// Eq(x,y) = k * (charge)q * (x - x0)(x^) + (y - y0)(y^)  /  ((x - x0)^2 + (y - y0)^2)^(3/2)  
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="x0"></param>
    /// <param name="y0"></param>
    /// <param name="x_comp"></param>
    /// <param name="y_comp"></param>
    /// <param name="charge">1 for positive charge, -1 for negative charge</param>
    /// <returns></returns>
    private void Eq(double x, double y, double x0, double y0, out double x_comp, out double y_comp, int charge = 1)
    {
        double x_numerator = 0;
        double x_denominator = 0;
        x_numerator = charge * (x - x0);
        x_denominator = Math.Pow(Mag((x - x0),(y - y0)), 3);
        x_comp = x_numerator / x_denominator;

        double y_numerator = 0;
        double y_denominator = 0;
        y_numerator = charge * (y - y0);
        y_denominator = x_denominator;
        y_comp = y_numerator / y_denominator;
    }

    /// <summary>
    /// Forward Euler Estimation : x(i + 1) = x(i) + delta_s * dx/ds
    /// </summary>
    /// <param name="currentValue">x(i)</param>
    /// <param name="delta">delta_s</param>
    /// <param name="d_ddelta">dx/ds</param>
    /// <returns></returns>
    private double ForwardEuler(double currentValue, double delta, double d_ddelta)
    {
        double nextValue = 0f;

        nextValue = currentValue + delta * d_ddelta;

        return nextValue;
    }

    /// <summary>
    /// Returns value = Sqrt[x^2 + y^2]
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private double Mag(double x, double y)
    {
        double value;

        value = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y,2));

        return value;
    }
}
