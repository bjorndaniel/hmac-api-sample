using Hmac.Client.Models;
using Hmac.Client.Services;
using Xamarin.Forms;

namespace Hmac.Client
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<IDataStore<Item>, MockDataStore>();
            DependencyService.Register<CustomDelegatingHandler>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
