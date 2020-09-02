using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ContinuousChargeDIstribution : MonoBehaviour
{
    // Public Variables (NOTE: Public variables can be changed in the Unity UI)
    public int numCharges = 2; // total number of charges along line of length 2L
    public double L = 2; // 1/2 length of line charge
    public double Q = 1; // Total charge for line
    public double desiredError = 0.1; // desired maximum error between exact and approxmate values
    public GameObject[] num = new GameObject[0]; // array of table entries for number of charges
    public GameObject[] delt = new GameObject[0]; // array of table entries for delta d
    public GameObject[] err = new GameObject[0]; // array of table entries for error

    // Private Variables
    private double _delta_d = 0; // distance between charges
    private double[] _chargeLocs; // array of charge locations on axis
    private double Eya = 0; // E approximation
    private double Eye = 0; // E exact
    private double[] err_array = new double[100]; // array of error values
    private double[] delta_array = new double[100]; // array of delta values
    private double[] num_array = new double[100]; // array or charge numbers

    // Start is called before the first frame update
    void Start()
    {
        CalcChargeLocs(); // Determine charge locations
        DetermineDelta(desiredError); // determine maximum delta d to achieve desired error
    }

    /// <summary>
    /// Determine the maximum distance delta between charges to achieve desired maximum error
    /// </summary>
    /// <param name="error"></param>
    private void DetermineDelta(double error)
    {
        double curError = 0; // variable for current error

        // Determine actual value for line of length 2L centered on teh x axis on a point (0,L)
        // E_exact = lambda / (2pi * L sqrt(2)) = (Q / 2*L) / (2pi * L sqrt(2))
        Debug.Log("Q = " + Q + "\n" +
                   "pi = " + Math.PI + "\n" +
                   "L^2 = " + Math.Pow(L,2) + "\n" +
                   "square root of 2 = " + Math.Sqrt(2));
        Eye = Q / (4 * Math.PI * Math.Pow(L,2) * Math.Sqrt(2));
        Debug.Log("E exact = " + Eye);

        // Calculate E approx from point chagers and compare
        int i = 0; // variable limiting number of iterations
        do
        {
            //Calculate approxmimation from _chargeLocs
            Eya =(numCharges/ (2*L)) * CalcApproxE(_chargeLocs);
            Debug.Log("E approx = " + Eya);

            // Calculate current error
            curError = CompareE(Eye, Eya);

            // Store relevant information
            StoreInfo(i, curError);

            // Increment the number of charges
            IncrementCharges();
            i++;
        } while ((curError >= desiredError) && i < 100); // end when either desired error has been reached or max chosen iterations occures

        PrintValues(); // Display relevant information

        Debug.Log("Current Error = " + curError);
        Debug.Log("number of charges = " + numCharges);
        Debug.Log("Delta d = " + _delta_d);
    }

    /// <summary>
    /// Increment the number of charges and calculate new charge locations
    /// </summary>
    private void IncrementCharges()
    {
        numCharges++;
        CalcChargeLocs();
        Debug.Log("Length of _chargeLocs to ensure appropriate incrementation = " + _chargeLocs.Length);
    }

    /// <summary>
    /// Store values in index
    /// </summary>
    /// <param name="i"></param>
    private void StoreInfo(int i, double error)
    {
        num_array[i] = numCharges;
        delta_array[i] = _delta_d;
        err_array[i] = error;
    }

    private void PrintValues()
    {
        int i = 0;
        foreach (GameObject g in num)
        {
            g.GetComponent<UnityEngine.UI.Text>().text =  num_array[i].ToString();
            i++;
        }

        i = 0;
        foreach (GameObject g in delt)
        {
            g.GetComponent<UnityEngine.UI.Text>().text = String.Format("{0:0.00000000}", delta_array[i]);
            i++;
        }

        i = 0;
        foreach (GameObject g in err)
        {
            g.GetComponent<UnityEngine.UI.Text>().text = String.Format("{0:0.00000000}", err_array[i]);
            i++;
        }
    }

    /// <summary>
    /// Calculate error between exact and approximate values
    ///  |(E_exact - E_approx)/E_exact|
    /// </summary>
    /// <param name="exact"></param>
    /// <param name="approx"></param>
    /// <returns></returns>
    private double CompareE(double exact, double approx)
    {
        double error = 0;

        error = Math.Abs((exact - approx) / exact);
        Debug.Log("Calculated Error = " + error);
        return error;
    }

    /// <summary>
    /// Calculate E in x direction at midpoint of line charge estimation at point x = L
    /// </summary>
    /// <param name="locs"></param>
    /// <returns></returns>
    private double CalcApproxE(double[] locs)
    {
        double value = 0;

        // Sum Ex components for each point charge
        for(int i = 0; i < locs.Length; i++)
        {
            double x_comp = 0;
            double y_comp = 0;

            Eq(L, 0, 0, locs[i], out x_comp, out y_comp);

            value += x_comp;
        }

        return value;
    }

    /// <summary>
    /// Calculates the location for each charge along line of length 2L
    /// </summary>
    private void CalcChargeLocs()
    {
        _delta_d = (2 * L) / numCharges; // calculated the distance between charges

        _chargeLocs = new double[numCharges]; // create vector for charge locations

        // Determine placement of charges
        if(numCharges == 1)
        {
            _chargeLocs[0] = 0; // set locataion of single charge at center
        } else
        {
            _chargeLocs[0] = -L; // set location of single charge at "bottom" of line

            // iterate adding the delta_d to charge location
            for (int i = 1; i < _chargeLocs.Length; i++)
            {
                _chargeLocs[i] = _chargeLocs[i - 1] + _delta_d;
            }
        }
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
        x_denominator = Math.Pow(Mag((x - x0), (y - y0)), 3);
        x_comp = x_numerator / x_denominator;

        double y_numerator = 0;
        double y_denominator = 0;
        y_numerator = charge * (y - y0);
        y_denominator = x_denominator;
        y_comp = y_numerator / y_denominator;
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

        value = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

        return value;
    }

}
