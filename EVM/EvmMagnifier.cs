using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Collections.Generic;

class EvmMagnifier
{
    private double alpha;
    private double r1;
    private double r2;
    private int nlevels;
    private double attenuation;
    private List<Image<Gray, byte>> lowpass1;
    private List<Image<Gray, byte>> lowpass2;

    public EvmMagnifier(double alpha = 100, double r1 = 100/60, double r2 = 60/60, int nlevels = 8, double attenuation = 1)
    {
        this.alpha = alpha;
        this.r1 = r1;
        this.r2 = r2;
        this.nlevels = nlevels;
        this.attenuation = attenuation;
        this.lowpass1 = null;
        this.lowpass2 = null;
    }

    public List<Image<Gray, byte>> BuildGaussianPyramid(Image<Gray, byte> image)
    {
        var gaussianPyramid = new List<Image<Gray, byte>> { image };
        for (int i = 1; i < nlevels; i++)
        {
            image = image.PyrDown();
            // Console.WriteLine($"Gaussian Pyramid Level {i}: {image.Size}, Channels: {image.NumberOfChannels}");
            gaussianPyramid.Insert(0, image);
        }
        return gaussianPyramid;
    }

    public List<Image<Gray, byte>> ApplyIIRFilter(List<Image<Gray, byte>> pyr)
    {
        if (lowpass1 == null || lowpass2 == null)
        {
            lowpass1 = new List<Image<Gray, byte>>();
            lowpass2 = new List<Image<Gray, byte>>();

            foreach (var level in pyr)
            {
                lowpass1.Add(level.Clone());
                lowpass2.Add(level.Clone());
            }
            // Console.WriteLine("Lowpass filters initialized.");
        }

        var filtered = new List<Image<Gray, byte>>();
        for (int i = 0; i < nlevels; i++)
        {
            // Console.WriteLine($"Applying IIR Filter at Level {i}: {pyr[i].Size}, Channels: {pyr[i].NumberOfChannels}, Lowpass1: {lowpass1[i].Size}, Channels: {lowpass1[i].NumberOfChannels}, Lowpass2: {lowpass2[i].Size}, Channels: {lowpass2[i].NumberOfChannels}");
            var tempPyr = pyr[i].Clone();
            CvInvoke.AddWeighted(lowpass1[i], 1 - r1, tempPyr, r1, 0, lowpass1[i]);
            CvInvoke.AddWeighted(lowpass2[i], 1 - r2, tempPyr, r2, 0, lowpass2[i]);

            filtered.Add(lowpass1[i] - lowpass2[i]);
            // Console.WriteLine($"Filtered Level {i}: {filtered[i].Size}, Channels: {filtered[i].NumberOfChannels}");
        }
        return filtered;
    }

    public List<Image<Gray, byte>> AmplifyPyramid(List<Image<Gray, byte>> filtered)
    {
        for (int l = 0; l < nlevels; l++)
        {
            filtered[l]._Mul(alpha);
        }
        return filtered;
    }

    public Image<Gray, byte> ReconstructPyramid(List<Image<Gray, byte>> filtered)
    {
        var upsampled = filtered[0].Clone();
        // Console.WriteLine($"Reconstructing from Pyramid: Initial Level Size {upsampled.Size}, Channels: {upsampled.NumberOfChannels}");
        for (int l = 1; l < nlevels; l++)
        {
            // Console.WriteLine($"Upsampling Level {l}: {upsampled.Size}, Channels: {upsampled.NumberOfChannels}, Filtered Level Size: {filtered[l].Size}, Channels: {filtered[l].NumberOfChannels}");
            upsampled = upsampled.Resize(filtered[l].Width, filtered[l].Height, Inter.Linear);
            // Console.WriteLine($"Upsampling Level {l}: {upsampled.Size}, Channels: {upsampled.NumberOfChannels}, Filtered Level Size: {filtered[l].Size}, Channels: {filtered[l].NumberOfChannels}");
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
        // Console.WriteLine($"Processing Frame: {frame.Size}, Channels: {frame.NumberOfChannels}");
        var pyramid = BuildGaussianPyramid(frame);
        var filtered = ApplyIIRFilter(pyramid);
        var amplified = AmplifyPyramid(filtered);
        var upsampled = ReconstructPyramid(amplified);
        var reconstructed = frame + attenuation * upsampled;
        return reconstructed;
    }
}