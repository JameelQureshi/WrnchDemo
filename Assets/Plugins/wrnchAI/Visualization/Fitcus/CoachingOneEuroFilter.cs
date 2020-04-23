using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoachingOneEuroFilter : MonoBehaviour
{

    public float min_cutoff;
    public float beta;
    public float d_cutoff;
    public float x_prev;
    public float dx_prev;
    public float t_prev;


    public CoachingOneEuroFilter(float t0, float x0,float dx0= 0.0f,float min_cutoff= 0.1f,float beta= 0.0f,float d_cutoff= 1.0f)
    {

        this.min_cutoff = min_cutoff;
        this.beta = beta;
        this.d_cutoff = d_cutoff;

        x_prev = x0;
        dx_prev = dx0;
        t_prev = t0;
    }


    public float SmoothingFactor(float t_e, float cutoff)
    {
        float r = 2 * Mathf.PI * cutoff * t_e;
        return r / (r + 1);
    }


    public float ExponentialSmoothing(float a,float x,float x_prev)
    {
        return a * x + (1 - a) * x_prev;
    }

    public float ApplyFilter(float t,float x)
    {
        ///Compute the filtered signal.
        float t_e = t - t_prev;

        // The filtered derivative of the signal.
        float a_d = SmoothingFactor(t_e, d_cutoff);
        float dx = (x - x_prev) / t_e;
        float dx_hat = ExponentialSmoothing(a_d, dx, dx_prev);

        // The filtered signal.
        float cutoff = min_cutoff + beta * Mathf.Abs(dx_hat);
        float a = SmoothingFactor(t_e, cutoff);
        float x_hat = ExponentialSmoothing(a, x, x_prev);

        // Memorize the previous values.
        x_prev = x_hat;
        dx_prev = dx_hat;
        t_prev = t;

        return x_hat;
    }



}
