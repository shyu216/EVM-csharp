namespace EVM
{
    public class VideoProcessorConfig
    {
        public string InputFile { get; set; } = "";
        public string OutputFolder { get; set; } = "";
        public string OutputFileName { get; set; } = "";
        public double Fl { get; set; } = 20.0 / 60;
        public double Fh { get; set; } = 100.0 / 60;
        public int NLevels { get; set; } = 8;
        public double Attenuation { get; set; } = 1;
        public double Alpha { get; set; } = 50;
        public PyramidType PyramidType { get; set; } = PyramidType.Gaussian;
        public double Fps { get; set; } = 30;
    }
}
