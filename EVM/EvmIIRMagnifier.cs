using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Collections.Generic;

class EvmIIRMagnifier
{
    private double alpha;
    private double r1;
    private double r2;
    private int nlevels;
    private double attenuation;
    private List<Image<Gray, double>> lowpass1;
    private List<Image<Gray, double>> lowpass2;

    public EvmIIRMagnifier(double alpha = 50, double r1 = 60 / 60.0, double r2 = 50 / 60.0, int nlevels = 6, double attenuation = 1)
    {
        this.alpha = alpha;
        this.r1 = r1;
        this.r2 = r2;
        this.nlevels = nlevels;
        this.attenuation = attenuation;
        this.lowpass1 = null;
        this.lowpass2 = null;

        Console.WriteLine($"EvmIIRMagnifier initialized with alpha: {alpha}, r1: {r1}, r2: {r2}, nlevels: {nlevels}, attenuation: {attenuation}");
    }

    public List<Image<Gray, double>> BuildGaussianPyramid(Image<Gray, double> image)
    {
        var gaussianPyramid = new List<Image<Gray, double>> { image };
        for (int i = 1; i < nlevels; i++)
        {
            image = image.PyrDown();
            gaussianPyramid.Insert(0, image);
        }
        return gaussianPyramid;
    }

    public List<Image<Gray, double>> ApplyIIRFilter(List<Image<Gray, double>> pyr)
    {
        if (lowpass1 == null || lowpass2 == null)
        {
            lowpass1 = new List<Image<Gray, double>>();
            lowpass2 = new List<Image<Gray, double>>();

            foreach (var level in pyr)
            {
                lowpass1.Add(level.Clone());
                lowpass2.Add(level.Clone());
            }
        }

        var filtered = new List<Image<Gray, double>>();
        for (int i = 0; i < nlevels; i++)
        {
            var tempPyr = pyr[i].Clone();
            CvInvoke.AddWeighted(lowpass1[i], 1.0 - r1, tempPyr, r1, 0, lowpass1[i]);
            CvInvoke.AddWeighted(lowpass2[i], 1.0 - r2, tempPyr, r2, 0, lowpass2[i]);

            filtered.Add(lowpass1[i] - lowpass2[i]);
        }
        return filtered;
    }

    public List<Image<Gray, double>> AmplifyPyramid(List<Image<Gray, double>> filtered)
    {
        for (int l = 0; l < nlevels; l++)
        {
            filtered[l]._Mul(alpha);
        }
        return filtered;
    }

    public Image<Gray, double> ReconstructPyramid(List<Image<Gray, double>> filtered)
    {
        var upsampled = filtered[0].Clone();
        for (int l = 1; l < nlevels; l++)
        {
            upsampled = upsampled.PyrUp();
            upsampled = upsampled.Resize(filtered[l].Width, filtered[l].Height, Inter.Linear);
            upsampled += filtered[l];
        }

        upsampled /= nlevels;

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
        var filtered = ApplyIIRFilter(pyramid);
        var amplified = AmplifyPyramid(filtered);
        var upsampled = ReconstructPyramid(amplified);

        var reconstructed = frameDouble + attenuation * upsampled;

        Mat reconstructedMat = reconstructed.Mat;
        CvInvoke.Min(reconstructedMat, new ScalarArray(255), reconstructedMat); 
        CvInvoke.Max(reconstructedMat, new ScalarArray(0), reconstructedMat); 
 
        return reconstructed.Convert<Gray, byte>();
    }
}