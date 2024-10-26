using RingInterceptorMaui.ViewModels;

namespace RingInterceptorMaui.Views
{
    public partial class MainPageView : ContentPage
    {
        public MainPageView()
        {
            BindingContext = new MainPageViewModel();
            InitializeComponent();
        }
    }

}
