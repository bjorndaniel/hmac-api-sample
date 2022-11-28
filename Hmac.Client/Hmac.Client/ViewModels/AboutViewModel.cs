using Hmac.Client.Models;
using MvvmHelpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Hmac.Client.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        
        private string _infoText = "Nothing sent";
        
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await SendApiRequestAsync());
        }

        public ICommand OpenWebCommand { get; }

        public string InfoText
        {
            get => _infoText;
            set => SetProperty(ref _infoText, value);
        }
        
        private async Task SendApiRequestAsync()
        {
            InfoText = "";
            var handler = new CustomDelegatingHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; 
            var client = HttpClientFactory.Create(handler);

            var response = await client.PostAsJsonAsync("http://...:3000/items", new Item
            {
                Text = "This is an item",
                Description = "This is an item description.",
                Id = Guid.NewGuid().ToString()
            });
            response = await client.GetAsync("http://192.168.9.164:3000/items");

            if (response.IsSuccessStatusCode)
            {
                InfoText = $"Success {DateTime.Now.TimeOfDay}";
            }
            else
            {
                InfoText = $"Failed {DateTime.Now.TimeOfDay}";
            }
        }
    }
}