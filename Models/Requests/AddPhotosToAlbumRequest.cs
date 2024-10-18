namespace PhotoPrismAlbumSyncer.Models.Requests
{
    public class AddPhotosToAlbumRequest
    {
        public bool All { get; set; }
        public string[] Photos { get; set; }
    }
}
