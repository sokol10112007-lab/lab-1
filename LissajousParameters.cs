namespace LissajousLab.Models
{
    public class LissajousParameters
    {
        public double AmplitudeX { get; init; }
        public double AmplitudeY { get; init; }

        public double FrequencyX { get; init; }
        public double FrequencyY { get; init; }

        public double PhaseXDegrees { get; init; }
        public double PhaseYDegrees { get; init; }

        public double TimeStep { get; init; }
    }
}