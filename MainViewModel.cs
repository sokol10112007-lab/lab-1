using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LissajousLab.Models;

namespace LissajousLab.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const double CanvasSize = 600.0;
        private const double GraphPadding = 25.0;

        private readonly LissajousModel _model = new();

        private string _amplitudeX = "1";
        private string _amplitudeY = "1";

        private string _frequencyX = "1";
        private string _frequencyY = "1";

        private string _phaseX = "0";
        private string _phaseY = "90";

        private string _timeStep = "0.002";

        private double _phaseYSlider = 90;

        private string _message = "Введіть параметри та натисніть «Побудувати».";
        private PointCollection _graphPoints = new();

        public MainViewModel()
        {
            BuildCommand = new RelayCommand(_ => Build());

            SetPresetCommand =
                new RelayCommand(SetPreset);

            Build();
        }

        public string AmplitudeX
        {
            get => _amplitudeX;
            set
            {
                if (SetField(ref _amplitudeX, value))
                    OnInputChanged();
            }
        }

        public string AmplitudeY
        {
            get => _amplitudeY;
            set
            {
                if (SetField(ref _amplitudeY, value))
                    OnInputChanged();
            }
        }

        public string FrequencyX
        {
            get => _frequencyX;
            set
            {
                if (SetField(ref _frequencyX, value))
                    OnInputChanged();
            }
        }

        public string FrequencyY
        {
            get => _frequencyY;
            set
            {
                if (SetField(ref _frequencyY, value))
                    OnInputChanged();
            }
        }

        public string PhaseX
        {
            get => _phaseX;
            set
            {
                if (SetField(ref _phaseX, value))
                    OnInputChanged();
            }
        }

        public string PhaseY
        {
            get => _phaseY;
            set
            {
                if (SetField(ref _phaseY, value))
                {
                    if (TryParseNumber(value, out double phase))
                    {
                        if (phase >= -180 && phase <= 180)
                        {
                            _phaseYSlider = phase;
                            OnPropertyChanged(nameof(PhaseYSlider));
                        }
                    }

                    OnInputChanged();
                }
            }
        }

        public string TimeStep
        {
            get => _timeStep;
            set
            {
                if (SetField(ref _timeStep, value))
                    OnInputChanged();
            }
        }

        public double PhaseYSlider
        {
            get => _phaseYSlider;

            set
            {
                if (Math.Abs(_phaseYSlider - value) < 0.0001)
                    return;

                _phaseYSlider = value;

                OnPropertyChanged();

                _phaseY =
                    value.ToString(
                        "F0",
                        CultureInfo.InvariantCulture);

                OnPropertyChanged(nameof(PhaseY));
            }
        }

        public PointCollection GraphPoints
        {
            get => _graphPoints;

            private set
            {
                _graphPoints = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;

            private set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public ICommand BuildCommand { get; }

        public ICommand SetPresetCommand { get; }

        private void Build()
        {
            try
            {
                if (!TryCreateParameters(
                        out LissajousParameters? parameters,
                        out string error))
                {
                    GraphPoints = new PointCollection();
                    Message = error;
                    return;
                }

                IReadOnlyList<PointD> modelPoints =
                    _model.CalculatePoints(parameters!);

                GraphPoints =
                    ConvertToScreenPoints(
                        modelPoints,
                        parameters!);

                Message =
                    $"Побудовано успішно. " +
                    $"Кількість точок: {modelPoints.Count}.";
            }
            catch (ArgumentException exception)
            {
                GraphPoints = new PointCollection();
                Message = exception.Message;
            }
            catch (Exception exception)
            {
                GraphPoints = new PointCollection();

                Message =
                    $"Не вдалося побудувати графік: " +
                    $"{exception.Message}";
            }
        }

        private PointCollection ConvertToScreenPoints(
            IReadOnlyList<PointD> points,
            LissajousParameters parameters)
        {
            var result = new PointCollection(points.Count);

            double centerX = CanvasSize / 2.0;
            double centerY = CanvasSize / 2.0;

            
            double maxAmplitude =
                Math.Max(
                    parameters.AmplitudeX,
                    parameters.AmplitudeY);

            double availableSize =
                CanvasSize - 2 * GraphPadding;

            double scale =
                availableSize /
                (2.0 * maxAmplitude);

            foreach (PointD point in points)
            {
                double screenX =
                    centerX + point.X * scale;

                
                double screenY =
                    centerY - point.Y * scale;

                result.Add(
                    new Point(screenX, screenY));
            }

            return result;
        }

        private bool TryCreateParameters(
            out LissajousParameters? parameters,
            out string error)
        {
            parameters = null;
            error = string.Empty;

            if (!TryParseNumber(AmplitudeX, out double ax))
            {
                error = "Ax повинно бути числом.";
                return false;
            }

            if (!TryParseNumber(AmplitudeY, out double ay))
            {
                error = "Ay повинно бути числом.";
                return false;
            }

            if (!TryParseNumber(FrequencyX, out double fx))
            {
                error = "fx повинно бути числом.";
                return false;
            }

            if (!TryParseNumber(FrequencyY, out double fy))
            {
                error = "fy повинно бути числом.";
                return false;
            }

            if (!TryParseNumber(PhaseX, out double phaseX))
            {
                error = "Фаза X повинна бути числом.";
                return false;
            }

            if (!TryParseNumber(PhaseY, out double phaseY))
            {
                error = "Фаза Y повинна бути числом.";
                return false;
            }

            if (!TryParseNumber(TimeStep, out double step))
            {
                error = "Крок часу повинен бути числом.";
                return false;
            }

            parameters = new LissajousParameters
            {
                AmplitudeX = ax,
                AmplitudeY = ay,

                FrequencyX = fx,
                FrequencyY = fy,

                PhaseXDegrees = phaseX,
                PhaseYDegrees = phaseY,

                TimeStep = step
            };

            return true;
        }

        private static bool TryParseNumber(
            string text,
            out double value)
        {
            string normalized =
                text.Trim().Replace(',', '.');

            return double.TryParse(
                normalized,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }

        private void SetPreset(object? parameter)
        {
            string preset =
                parameter?.ToString() ?? string.Empty;

            switch (preset)
            {
                case "1:1":
                    FrequencyX = "1";
                    FrequencyY = "1";
                    break;

                case "1:2":
                    FrequencyX = "1";
                    FrequencyY = "2";
                    break;

                case "2:3":
                    FrequencyX = "2";
                    FrequencyY = "3";
                    break;

                case "3:4":
                    FrequencyX = "3";
                    FrequencyY = "4";
                    break;
            }

            Build();
        }

        private void OnInputChanged()
        {
            Message =
                "Параметри змінено. Натисніть «Побудувати».";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
