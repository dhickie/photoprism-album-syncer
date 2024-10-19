using System.Text.Json.Serialization;

namespace PhotoPrismAlbumSyncer.Models.Responses
{
    public class AddPhotosToAlbumResponse
    {
        [JsonPropertyName("photos")]
        public string[] Photos { get; set; }
    }
}
