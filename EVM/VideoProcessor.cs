using System;
using System.IO;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

class VideoProcessor
{
    static void Main(string[] args)
    {
        string inputFolder = "C:/Users/LMAPA/Documents/GitHub/vision-black-tech/EVM_Matlab/data/";
        string[] videoFiles = {
            "face.mp4",
            //"mybody.mp4",
            //"mybody_light.mp4",
            //"mybody_sun.mp4",
            //"myface.mp4",
            //"myhand_head_inner.mp4",
            //"myhand_head_outer.mp4",
            //"myhand_table_inner.mp4",
            //"myhand_table_outer.mp4"
        };

        string outputFolder = Path.Combine(inputFolder, "csharp");
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        foreach (string videoFile in videoFiles)
        {
            string inputPath = Path.Combine(inputFolder, videoFile);
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"File not found: {inputPath}");
                continue;
            }

            VideoCapture capture = new VideoCapture(inputPath);
            if (!capture.IsOpened)
            {
                Console.WriteLine($"Unable to open the video file: {inputPath}");
                continue;
            }

            string outputPath = Path.Combine(outputFolder, $"amplified_{Path.GetFileNameWithoutExtension(videoFile)}.mp4");
            VideoWriter writer = new VideoWriter(outputPath, VideoWriter.Fourcc('H', '2', '6', '4'), capture.Get(CapProp.Fps),
                new Size((int)capture.Get(CapProp.FrameWidth), (int)capture.Get(CapProp.FrameHeight)), true);

            EvmMagnifier yMagnifier = new EvmMagnifier(attenuation: 1);
            EvmMagnifier crMagnifier = new EvmMagnifier(attenuation: 1);
            EvmMagnifier cbMagnifier = new EvmMagnifier(attenuation: 1);

            Mat frame = new Mat();
            while (capture.Read(frame))
            {
                Image<Bgr, byte> frameImage = frame.ToImage<Bgr, byte>();
                CvInvoke.Imshow("Original Video", frameImage.Mat);

                var yccFrame = frameImage.Convert<Ycc, byte>();
                var yChannel = yccFrame.Split()[0];
                var crChannel = yccFrame.Split()[1];
                var cbChannel = yccFrame.Split()[2];
                var processedY = yMagnifier.ProcessFrame(yChannel);
                var processedCr = crMagnifier.ProcessFrame(crChannel);
                var processedCb = cbMagnifier.ProcessFrame(cbChannel);
                var reconstructedYcc = new Image<Ycc, byte>(frame.Size);
                reconstructedYcc[0] = processedY;
                reconstructedYcc[1] = processedCr;
                reconstructedYcc[2] = processedCb;

                var reconstructedBgr = reconstructedYcc.Convert<Bgr, byte>();

                CvInvoke.Imshow("Amplified Video", reconstructedBgr.Mat);

                writer.Write(reconstructedBgr.Mat);

                if (CvInvoke.WaitKey(1) == 'q')
                {
                    break;
                }
            }

            capture.Dispose();
            writer.Dispose();
            Console.WriteLine($"Processed and saved: {outputPath}");
        }

        CvInvoke.DestroyAllWindows();
    }
}