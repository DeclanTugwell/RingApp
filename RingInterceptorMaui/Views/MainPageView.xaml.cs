using RingInterceptorMaui.ViewModels;

namespace RingInterceptorMaui.Views
{
    public partial class MainPageView : ContentPage
    {
        public MainPageView()
        {
            BindingContext = new MainPageViewModel(ScrollOutputToBottom);
            InitializeComponent();
        }

        private void ScrollOutputToBottom()
        {
            OutputScrollView.ScrollToAsync(OutputLabel, ScrollToPosition.End, true);
        }
    }

}
