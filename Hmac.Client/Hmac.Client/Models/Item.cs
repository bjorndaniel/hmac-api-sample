using System;
using System.Text.Json.Serialization;

namespace Hmac.Client.Models
{
    public class Item
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}