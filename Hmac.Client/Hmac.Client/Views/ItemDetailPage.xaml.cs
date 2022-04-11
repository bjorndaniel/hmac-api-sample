using Hmac.Client.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Hmac.Client.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}