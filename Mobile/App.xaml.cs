using Carmen.Mobile.Views;

namespace Carmen.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Login());
        }
    }
}