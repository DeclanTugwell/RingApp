using RingInterceptorMaui.Enums;
using RingInterceptorMaui.Models;
using System.Windows.Input;

namespace RingInterceptorMaui.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        public delegate void ScrollOutput();
        public event ScrollOutput ScrollOutputEvent;

        private FormattedString _outputText = "";
        public FormattedString OutputText
        {
            get
            {
                return _outputText;
            }
            set
            {
                if (value != _outputText)
                {
                    _outputText = value;
                    OnPropertyChanged(nameof(OutputText));
                }
            }
        }

        private double _timerCount = 0;
        public double? TimerCount
        {
            get
            {
                double? returnValue = null;
                if (_timerCount != 0)
                {
                    returnValue = _timerCount;
                }
                return returnValue;
            }
            set
            {
                if (value != _timerCount)
                {
                    _timerCount = value ?? 0;
                    OnPropertyChanged(nameof(TimerCount));
                }
            }
        }

        private bool _entryEnabled = true;
        public bool EntryEnabled
        {
            get
            {
                return _entryEnabled;
            }

            set
            {
                if (value != _entryEnabled)
                {
                    _entryEnabled = value;
                    OnPropertyChanged(nameof(EntryEnabled));
                }
            }
        }

        public double DisableBtnOpacity { get; set; } = 1;
        public string DisableBtnText { get; set; } = "Disable";
        public ICommand BtnClickedCommand { get; set; }
        public ICommand BackgroundClickedCommand { get; set; }

        private RingClient? _ringClient = null;

        private bool _ringDoorbellEnabled = true;

        private static readonly Dictionary<OutputType, Color> OutputTextColours = new()
        {
            { OutputType.Negative, Colors.IndianRed },
            { OutputType.Neutral, Colors.LightGray },
            { OutputType.Positive, Colors.DarkGreen }
        };

        public MainPageViewModel(ScrollOutput scrollOutput)
        {
            BtnClickedCommand = new Command(DisableBtnPressed);
            BackgroundClickedCommand = new Command(BackgroundClicked);
            ScrollOutputEvent = scrollOutput;
        }

        private async void DisableBtnPressed()
        {
            if (_ringDoorbellEnabled)
            {
                if (IsTimerCountValueValid())
                {
                    await DisableRingDoorbell();
#pragma warning disable CS4014 
                    Task.Run(async () =>
                    {
                        while (TimerCount > 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(10));
                            int count = (int)(_timerCount * 100);
                            count -= 1;
                            TimerCount = count / 100.0;
                        }
                        await EnableRingDoorbell();
                    });
#pragma warning restore CS4014 
                }
            }
            else
            {
                TimerCount = 0;
            }
        }

        private void SetOutputText(string textValue, OutputType outputType)
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                OutputText.Spans.Add(new Span() { Text = $"{textValue}\n", TextColor = OutputTextColours[outputType] });
                OnPropertyChanged(nameof(OutputText));
                ScrollOutputEvent.Invoke();
            });
        }

        private async Task DisableRingDoorbell()
        {
            if (_ringClient == null)
            {
                await InitialiseRingClient();
            }
            if (_ringClient != null)
            {
                var targetDevice = (await _ringClient.FetchDevices())?.DoorBots.FirstOrDefault();
                if (targetDevice != null)
                {
                    var result = await _ringClient.DisableMotionDetection(targetDevice.Id.ToString());
                    if (result == System.Net.HttpStatusCode.OK)
                    {
                        SetOutputText("Ring Camera Disabled.", OutputType.Positive);
                        DisableBtnOpacity = 0.5;
                        DisableBtnText = "Cancel";
                        _ringDoorbellEnabled = false;
                        OnPropertyChanged(nameof(DisableBtnText));
                        OnPropertyChanged(nameof(DisableBtnOpacity));
                    }
                    else
                    {
                        SetOutputText($"Failed to Disable Ring Doorbell: {result.ToString()}", OutputType.Negative);
                    }
                }
                else
                {
                    SetOutputText("Ring Device Not Found.", OutputType.Negative);
                }
            }
        }

        private async Task EnableRingDoorbell()
        {
            if (_ringClient == null)
            {
                await InitialiseRingClient();
            }
            if (_ringClient != null)
            {
                var targetDevice = (await _ringClient.FetchDevices())?.DoorBots.FirstOrDefault();
                if (targetDevice != null)
                {
                    var result = await _ringClient.EnableMotionDetection(targetDevice.Id.ToString());
                    if (result == System.Net.HttpStatusCode.OK)
                    {
                        SetOutputText("Ring Camera Enabled.", OutputType.Negative);
                        DisableBtnOpacity = 1;
                        DisableBtnText = "Disable";
                        OnPropertyChanged(nameof(DisableBtnText));
                        OnPropertyChanged(nameof(DisableBtnOpacity));
                        TimerCount = null;
                        _ringDoorbellEnabled = true;
                    }
                    else
                    {
                        SetOutputText($"Failed to Enable Ring Doorbell: {result.ToString()}", OutputType.Negative);
                    }
                }
                else
                {
                    SetOutputText("Ring Device Not Found.", OutputType.Negative);
                }
            }
        }

        private bool IsTimerCountValueValid()
        {
            var timerValid = false;
            if (TimerCount != null && TimerCount >= 10)
            {
                timerValid = true;
            }
            else
            {
                Application.Current!.MainPage!.DisplayAlert("", "Timer has a minimum of 10s.", "Calm");
            }
            return timerValid;
        }

        private async Task InitialiseRingClient()
        {
            _ringClient = await RingClient.Create(FetchAccountCredentials, new OutputWrapper(SetOutputText));
        }

        private void BackgroundClicked()
        {
            EntryEnabled = false;
            EntryEnabled = true;
        }

        private async Task<AccountCredentials> FetchAccountCredentials()
        {
            var credentials = new AccountCredentials();
            credentials.Email = await Application.Current.MainPage.DisplayPromptAsync("Email", "Enter Email", "Ok");
            credentials.Password = await Application.Current.MainPage.DisplayPromptAsync("Password", "Enter Password", "Ok");
            if (string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
            {
                SetOutputText("Email or Password input incorrectly.", OutputType.Negative);
                credentials = await FetchAccountCredentials();
            }
            else
            {
                SetOutputText("Received Email and Password.", OutputType.Neutral);
            }

            return credentials;
        }
    }
}
