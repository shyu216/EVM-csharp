/*
 * Eulerian Video Magnification (EVM) implementation in C# using Emgu CV.
 *
 * This implementation performs spatial decomposition using a Gaussian pyramid
 * and temporal filtering using a Butterworth filter.
 *
 * Optimized for facial pulse detection applications.
 *
 * Author: SIHONG YU
 * Date: 2025.6
 */
 
using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Collections.Generic;


class EvmMagnifier
{
    private double alpha;
    private int nLevels;
    private double attenuation;
    private List<Image<Gray, double>> lowpass1;
    private List<Image<Gray, double>> lowpass2;
    private List<Image<Gray, double>> prevPyr;
    private double[] lowA;
    private double[] lowB;
    private double[] highA;
    private double[] highB;

    public EvmMagnifier(double alpha = 50, double fl = 60 / 60.0, double fh = 100 / 60.0, int nLevels = 6, int fps = 30, double attenuation = 1)
    {
        this.alpha = alpha;
        this.nLevels = nLevels;
        this.attenuation = attenuation;
        this.lowpass1 = null;
        this.lowpass2 = null;
        this.prevPyr = null; // Used for butter only, not IIR

        this.lowA = new double[] { 0.04979798, 0.04979798 };
        this.lowB = new double[] { 1.0, -0.90040404 };
        this.highA = new double[] { 0.08045018, 0.08045018 };
        this.highB = new double[] { 1.0, -0.83909963 };

        Console.WriteLine($"EvmMagnifier initialized with alpha: {alpha}, fl: {fl}, fh: {fh}, nLevels: {nLevels}, attenuation: {attenuation}");
        Console.WriteLine($"Butter coefficients - Low: a={string.Join(", ", lowA)}, b={string.Join(", ", lowB)}");
        Console.WriteLine($"Butter coefficients - High: a={string.Join(", ", highA)}, b={string.Join(", ", highB)}");
    }

    public List<Image<Gray, double>> BuildGaussianPyramid(Image<Gray, double> image)
    {
        var gaussianPyramid = new List<Image<Gray, double>> { image };
        for (int i = 1; i < nLevels; i++)
        {
            image = image.PyrDown();
            gaussianPyramid.Insert(0, image);
        }
        return gaussianPyramid;
    }

    public List<Image<Gray, double>> ApplyButterFilter(List<Image<Gray, double>> pyr)
    {

        if (lowpass1 == null || lowpass2 == null || prevPyr == null)
        {
            lowpass1 = new List<Image<Gray, double>>();
            lowpass2 = new List<Image<Gray, double>>();
            prevPyr = new List<Image<Gray, double>>();

            foreach (var level in pyr)
            {
                lowpass1.Add(level.Clone());
                lowpass2.Add(level.Clone());
                prevPyr.Add(level.Clone());}
        }

        var filtered = new List<Image<Gray, double>>();
        for (int i = 0; i < nLevels; i++)
        {
            var tempPyr = pyr[i].Clone();

            // Apply high-pass filter
            CvInvoke.AddWeighted(lowpass1[i], -highB[1], tempPyr, highA[0], 0, lowpass1[i]);
            CvInvoke.AddWeighted(lowpass1[i], 1, prevPyr[i], highA[1], 0, lowpass1[i]);
            CvInvoke.Divide(lowpass1[i], new ScalarArray(highB[0]), lowpass1[i]);

            // Apply low-pass filter
            CvInvoke.AddWeighted(lowpass2[i], -lowB[1], tempPyr, lowA[0], 0, lowpass2[i]);
            CvInvoke.AddWeighted(lowpass2[i], 1, prevPyr[i], lowA[1], 0, lowpass2[i]);
            CvInvoke.Divide(lowpass2[i], new ScalarArray(lowB[0]), lowpass2[i]);

            prevPyr[i] = tempPyr;

            filtered.Add(lowpass1[i] - lowpass2[i]);
        }

        return filtered;
    }

    public List<Image<Gray, double>> AmplifyPyramid(List<Image<Gray, double>> filtered)
    {
        for (int l = 0; l < nLevels; l++)
        {
            filtered[l]._Mul(alpha);
        }
        return filtered;
    }

    public Image<Gray, double> ReconstructPyramid(List<Image<Gray, double>> filtered)
    {
        var upsampled = filtered[0].Clone();
        for (int l = 1; l < nLevels; l++)
        {
            upsampled = upsampled.PyrUp();
            upsampled = upsampled.Resize(filtered[l].Width, filtered[l].Height, Inter.Linear);
            upsampled += filtered[l];
        }

        upsampled /= nLevels;

        return upsampled;
    }

    public void Reset()
    {
        lowpass1 = null;
        lowpass2 = null;
    }

    public Image<Gray, byte> ProcessFrame(Image<Gray, byte> frame)
    {
        var frameDouble = frame.Convert<Gray, double>();

        var pyramid = BuildGaussianPyramid(frameDouble);
        var filtered = ApplyButterFilter(pyramid);
        var amplified = AmplifyPyramid(filtered);
        var upsampled = ReconstructPyramid(amplified);

        var reconstructed = frameDouble + attenuation * upsampled;

        Mat reconstructedMat = reconstructed.Mat;
        CvInvoke.Min(reconstructedMat, new ScalarArray(255), reconstructedMat);
        CvInvoke.Max(reconstructedMat, new ScalarArray(0), reconstructedMat);

        return reconstructed.Convert<Gray, byte>();
    }
}