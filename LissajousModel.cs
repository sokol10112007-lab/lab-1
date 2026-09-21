using System;
using System.Collections.Generic;

namespace LissajousLab.Models
{
    public class LissajousModel
    {
        private const int SlowPeriodsToDraw = 5;
        private const int MaximumPointCount = 200_000;

        public IReadOnlyList<PointD> CalculatePoints(
            LissajousParameters parameters)
        {
            Validate(parameters);

            double phaseX =
                DegreesToRadians(parameters.PhaseXDegrees);

            double phaseY =
                DegreesToRadians(parameters.PhaseYDegrees);

            double minFrequency =
                Math.Min(parameters.FrequencyX,
                         parameters.FrequencyY);

            
            double duration =
                SlowPeriodsToDraw / minFrequency;

            int pointCount =
                (int)Math.Ceiling(duration / parameters.TimeStep) + 1;

            if (pointCount > MaximumPointCount)
            {
                throw new ArgumentException(
                    $"Занадто багато точок ({pointCount}). " +
                    $"Збільште крок часу.");
            }

            var points = new List<PointD>(pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                double t = i * parameters.TimeStep;

                double x =
                    parameters.AmplitudeX *
                    Math.Sin(
                        2 * Math.PI *
                        parameters.FrequencyX * t +
                        phaseX);

                double y =
                    parameters.AmplitudeY *
                    Math.Sin(
                        2 * Math.PI *
                        parameters.FrequencyY * t +
                        phaseY);

                points.Add(new PointD(x, y));
            }

            return points;
        }

        public void Validate(LissajousParameters parameters)
        {
            if (parameters.AmplitudeX <= 0 ||
                parameters.AmplitudeY <= 0)
            {
                throw new ArgumentException(
                    "Амплітуди Ax та Ay повинні бути додатними.");
            }

            if (parameters.FrequencyX <= 0 ||
                parameters.FrequencyY <= 0)
            {
                throw new ArgumentException(
                    "Частоти fx та fy повинні бути додатними.");
            }

            if (parameters.TimeStep <= 0)
            {
                throw new ArgumentException(
                    "Крок часу повинен бути додатним.");
            }

            double maxFrequency =
                Math.Max(
                    parameters.FrequencyX,
                    parameters.FrequencyY);

            
            double maximumAllowedStep =
                1.0 / (20.0 * maxFrequency);

            if (parameters.TimeStep > maximumAllowedStep)
            {
                throw new ArgumentException(
                    $"Крок часу надто великий. " +
                    $"Для заданих частот він має бути не більшим " +
                    $"за {maximumAllowedStep:F5} с.");
            }
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}