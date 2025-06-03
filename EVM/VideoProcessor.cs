using System;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

class VideoProcessor
{
    static void Main(string[] args)
    {
        // Console.WriteLine("Please enter the video file path or press Enter to search for videos in the default folder:");
        // string inputPath = Console.ReadLine();

        // string inputPath = "C:/Users/LMAPA/Documents/GitHub/vision-black-tech/EVM_Matlab/data/face.mp4";
        // string inputPath = "C:/Users/LMAPA/Documents/GitHub/vision-black-tech/EVM_Matlab/data/face2.mp4";
        string inputPath = "C:/Users/LMAPA/Documents/GitHub/vision-black-tech/EVM_Matlab/data/myface2.mp4";
        

        VideoCapture capture = new VideoCapture(inputPath);
        if (!capture.IsOpened)
        {
            Console.WriteLine("Unable to open the video file.");
            return;
        }

        EvmMagnifier yMagnifier = new EvmMagnifier(attenuation: 1.0);
        EvmMagnifier crMagnifier = new EvmMagnifier(attenuation: 1.0);
        EvmMagnifier cbMagnifier = new EvmMagnifier(attenuation: 1.0);

        Mat frame = new Mat();
        while (capture.Read(frame))
        {
            Image<Bgr, byte> frameImage = frame.ToImage<Bgr, byte>();
            CvInvoke.Imshow("Original Video", frameImage.Mat);

            // Apply processing to all channels
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

            if (CvInvoke.WaitKey(1) == 'q')
            {
                break;
            }
        }

        capture.Dispose();
        CvInvoke.DestroyAllWindows();
    }
}