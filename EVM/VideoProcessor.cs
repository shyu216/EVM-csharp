using System;
using System.IO;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace EVM
{
    public class VideoProcessor
    {
        private readonly VideoProcessorConfig _config;

        public event Action<int>? OnProgress;
        public event Action<string>? OnLog;
        public event Action<bool, string>? OnComplete;

        public VideoProcessor(VideoProcessorConfig config)
        {
            _config = config;
        }

        public void ProcessAsync()
        {
            Task.Run(() => Process());
        }

        private void Process()
        {
            try
            {
                Log($"Starting video processing: {Path.GetFileName(_config.InputFile)}");
                Log($"Input file: {_config.InputFile}");
                Log($"Output folder: {_config.OutputFolder}");
                Log($"Output file: {_config.OutputFileName}");
                Log($"Parameters - fl: {_config.Fl:F4}, fh: {_config.Fh:F4}, nLevels: {_config.NLevels}, attenuation: {_config.Attenuation}, pyramidType: {_config.PyramidType}");

                if (!File.Exists(_config.InputFile))
                {
                    OnComplete?.Invoke(false, $"Input file does not exist: {_config.InputFile}");
                    return;
                }

                if (!Directory.Exists(_config.OutputFolder))
                {
                    Directory.CreateDirectory(_config.OutputFolder);
                    Log($"Created output folder: {_config.OutputFolder}");
                }

                VideoCapture capture = new VideoCapture(_config.InputFile);
                if (!capture.IsOpened)
                {
                    OnComplete?.Invoke(false, $"Unable to open video file: {_config.InputFile}");
                    return;
                }

                double videoFps = capture.Get(CapProp.Fps);
                int frameWidth = (int)capture.Get(CapProp.FrameWidth);
                int frameHeight = (int)capture.Get(CapProp.FrameHeight);
                int totalFrames = (int)capture.Get(CapProp.FrameCount);

                // Use FPS from config (detected from video), fallback to video FPS if invalid
                double processingFps = _config.Fps > 0 ? _config.Fps : (videoFps > 0 ? videoFps : 30);
                
                Log($"Video info - FPS: {videoFps:F2}, Resolution: {frameWidth}x{frameHeight}, Total frames: {totalFrames}");
                Log($"Using processing FPS: {processingFps:F2} for Butterworth filter (critical for accurate frequency filtering)");

                string outputPath = Path.Combine(_config.OutputFolder, _config.OutputFileName);
                VideoWriter writer = new VideoWriter(outputPath, VideoWriter.Fourcc('H', '2', '6', '4'), videoFps,
                    new System.Drawing.Size(frameWidth, frameHeight), true);

                EvmMagnifier yMagnifier = new EvmMagnifier(
                    alpha: _config.Alpha,
                    fl: _config.Fl, 
                    fh: _config.Fh, 
                    nLevels: _config.NLevels, 
                    fps: (int)processingFps,
                    attenuation: _config.Attenuation,
                    pyramidType: _config.PyramidType);
                EvmMagnifier crMagnifier = new EvmMagnifier(
                    alpha: _config.Alpha,
                    fl: _config.Fl, 
                    fh: _config.Fh, 
                    nLevels: _config.NLevels, 
                    fps: (int)processingFps,
                    attenuation: _config.Attenuation,
                    pyramidType: _config.PyramidType);
                EvmMagnifier cbMagnifier = new EvmMagnifier(
                    alpha: _config.Alpha,
                    fl: _config.Fl, 
                    fh: _config.Fh, 
                    nLevels: _config.NLevels, 
                    fps: (int)processingFps,
                    attenuation: _config.Attenuation,
                    pyramidType: _config.PyramidType);

                Mat frame = new Mat();
                int frameCount = 0;

                while (capture.Read(frame))
                {
                    Image<Bgr, byte> frameImage = frame.ToImage<Bgr, byte>();

                    var yccFrame = frameImage.Convert<Ycc, byte>();
                    var channels = yccFrame.Split();
                    var yChannel = channels[0];
                    var crChannel = channels[1];
                    var cbChannel = channels[2];

                    var processedY = yMagnifier.ProcessFrame(yChannel);
                    var processedCr = crMagnifier.ProcessFrame(crChannel);
                    var processedCb = cbMagnifier.ProcessFrame(cbChannel);

                    var reconstructedYcc = new Image<Ycc, byte>(frame.Size);
                    reconstructedYcc[0] = processedY;
                    reconstructedYcc[1] = processedCr;
                    reconstructedYcc[2] = processedCb;

                    var reconstructedBgr = reconstructedYcc.Convert<Bgr, byte>();
                    writer.Write(reconstructedBgr.Mat);

                    frameCount++;

                    if (frameCount % 10 == 0 || frameCount == totalFrames)
                    {
                        int progress = totalFrames > 0 ? (int)((double)frameCount / totalFrames * 100) : 0;
                        OnProgress?.Invoke(progress);
                        Log($"Progress: {frameCount}/{totalFrames} frames ({progress}%)");
                    }
                }

                capture.Dispose();
                writer.Dispose();

                Log($"Processing completed, output file: {outputPath}");
                OnComplete?.Invoke(true, $"Video processing completed!\nOutput file: {outputPath}\nTotal frames processed: {frameCount}");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                OnComplete?.Invoke(false, $"Error during processing: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }
    }
}
