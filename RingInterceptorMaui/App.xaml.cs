using RingInterceptorMaui.Views;

namespace RingInterceptorMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPageView());
        }
    }
}
