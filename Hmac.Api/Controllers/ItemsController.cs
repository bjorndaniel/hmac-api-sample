using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Hmac.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : BaseController
    {
        [Route(""),HttpGet]
        public IEnumerable<Item> Get()
        {
            //var principal = Request.GetRequestContext().Principal as ClaimsPrincipal;

            //var Name = ClaimsPrincipal.Current.Identity.Name;

            return Item.CreateItems();
        }

        [Route(""),HttpPost]
        public Item Post(Item item) =>
            item;
    }

    public class Item
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;


        public static List<Item> CreateItems()
        {
            var items = new List<Item>
            {
                new Item {Id = Guid.NewGuid().ToString(), Text = "First item", Description = "This is a nice item"},
                new Item {Id = Guid.NewGuid().ToString(), Text = "Second item", Description = "This is a nicer item"},
                new Item {Id = Guid.NewGuid().ToString(), Text = "Third item", Description = "This is the nicest item"},
            };

            return items;
        }
    }
}