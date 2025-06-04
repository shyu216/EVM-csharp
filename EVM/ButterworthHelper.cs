/*
 * This code is adapted from MathNet.Filtering, Version=0.7.0.0.
 *
 * The original design interface has been removed to directly compute 
 * numerator and denominator coefficients from cutoff frequency and sampling frequency.
 *
 * Author: SIHONG YU
 * Date: 2025.6
 */

using System.Numerics;
using MathNet.Filtering.TransferFunctions;
using MathNet.Numerics;

public static class IirCoefficients
{
    //
    // Summary:
    //     Computes the warped frequency.
    //
    // Parameters:
    //   cutoffFrequency:
    //     Digital cutoff frequency.
    //
    //   samplingTime:
    //     Sampling time (computed as the reverse of the sampling frequency).
    //
    // Returns:
    //     Warped frequency.
    private static double WarpFrequency(double cutoffFrequency, double samplingTime)
    {
        return Math.Tan(Math.PI * cutoffFrequency / samplingTime);
    }

    //
    // Summary:
    //     Computes the coefficients of the polynomial whose solutions are roots are given
    //     as input parameter.
    //
    // Parameters:
    //   roots:
    //     Roots of the polynomial.
    //
    // Returns:
    //     Polynomial coefficients.
    private static Complex[] PolynomialCoefficients(Complex[] roots)
    {
        Complex[] array = Generate.Repeat(roots.Length + 1, Complex.Zero);
        array[0] = Complex.One;
        for (int i = 0; i < roots.Length; i++)
        {
            for (int num = i; num >= 0; num--)
            {
                array[num + 1] -= roots[i] * array[num];
            }
        }

        return array;
    }

    //
    // Summary:
    //     Recomputes the gain and the list of zeros and poles for a low-pass filter.
    //
    // Parameters:
    //   gain:
    //     Initial gain.
    //
    //   zeros:
    //     List of zeros.
    //
    //   poles:
    //     List of poles.
    //
    //   wc:
    //     Cutoff frequency.
    //
    // Returns:
    //     Recomputed gain, list of zeros and list of poles.
    //
    // Comments:
    //     For performance reasons, the method edits the input list of zeros and poles in
    //     place, so the input and output arrays are actually the same object.
    private static (double gain, Complex[] zeros, Complex[] poles) LowPassTransferFunctionTransformer(double gain, Complex[] zeros, Complex[] poles, double wc)
    {
        int num = zeros.Length;
        int num2 = poles.Length;
        gain *= Math.Pow(1.0 / wc, num - num2);
        for (int num3 = num - 1; num3 >= 0; num3--)
        {
            zeros[num3] *= (Complex)wc;
        }

        for (int num4 = num2 - 1; num4 >= 0; num4--)
        {
            poles[num4] *= (Complex)wc;
        }

        return (gain, zeros, poles);
    }

    //
    // Summary:
    //     Computes the IIR coefficients for a low-pass Butterworth filter.
    //
    // Parameters:
    //   passbandFreq:
    //     Passband corner frequency (in Hz).
    //
    //   stopbandFreq:
    //     Stopband corner frequency (in Hz).
    //
    //   passbandRipple:
    //     Maximum allowed passband ripple.
    //
    //   stopbandAttenuation:
    //     Minimum required stopband attenuation.
    //
    // Returns:
    //     IIR coefficients.
    public static (double[] numerator, double[] denominator) LowPass(byte n, double wc)
    {
        byte item = n;
        double item2 = wc;
        (double gain, Complex[] zeros, Complex[] poles) tuple2 = TransferFunction(item);
        double item3 = tuple2.gain;
        Complex[] item4 = tuple2.zeros;
        Complex[] item5 = tuple2.poles;
        item2 = WarpFrequency(item2, 2.0);
        (item3, item4, item5) = LowPassTransferFunctionTransformer(item3, item4, item5, item2);
        return Coefficients(item3, item4, item5, 2.0);
    }

    //
    // Summary:
    //     Computes the transfer function for a generic Butterworth filter.
    //
    // Parameters:
    //   n:
    //     Order of the filter.
    //
    // Returns:
    //     The triplet gain, zeros and poles of the transfer function
    private static (double gain, Complex[] zeros, Complex[] poles) TransferFunction(uint n)
    {
        Complex[] item = new Complex[0];
        Complex[] array = new Complex[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = Complex.Exp(Complex.ImaginaryOne * (Complex)Math.PI * (Complex)(2 * (i + 1) + n - 1) / (Complex)(2 * n));
        }

        if ((n & 1) == 1)
        {
            uint num = (n + 1) / 2;
            array[num - 1] = -1;
        }

        return (1.0, item, array);
    }

    //
    // Summary:
    //     Returns the list of IIR coefficients for a generic Butterworth filter.
    //
    // Parameters:
    //   gain:
    //     Filter gain.
    //
    //   zeros:
    //     Filter zeros list.
    //
    //   poles:
    //     Filter poles list.
    //
    //   T:
    //     Sampling time (inverse of sampling frequency).
    //
    // Returns:
    //     The list of IIR coefficients.
    private static (double[] numerator, double[] denominator) Coefficients(double gain, Complex[] zeros, Complex[] poles, double T)
    {
        (double, Complex[], Complex[]) tuple = BilinearTransform.Apply(gain, zeros, poles, T);
        gain = tuple.Item1;
        zeros = tuple.Item2;
        poles = tuple.Item3;
        double[] item = Generate.Map(PolynomialCoefficients(zeros), (Complex num) => (num * (Complex)gain).Real);
        double[] item2 = Generate.Map(PolynomialCoefficients(poles), (Complex den) => den.Real);
        return (item, item2);
    }
}