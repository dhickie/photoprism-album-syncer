using Microsoft.Extensions.Configuration;

namespace PhotoPrismAlbumSyncer
{
    internal class Program
    {
        private static PhotoPrismClient _client;
        private static string _basePath;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false);
            var configurationRoot = builder.Build();
            var config = configurationRoot.Get<Config>();

            var httpClient = new HttpClient();
            _client = new PhotoPrismClient(httpClient, config);
            
            _basePath = Directory.GetCurrentDirectory();
            await IterateYears();
        }

        private static async Task IterateYears()
        {
            var years = Directory.GetDirectories(_basePath);
            foreach (var year in years)
            {
                var albums = Directory.GetDirectories(year);
                foreach (var album in albums)
                {
                    var albumName = Path.GetRelativePath(year, album);
                    var files = Directory.GetFiles(album);
                    await CreateAlbum(year, albumName, files);
                }
            }
        }

        private static async Task CreateAlbum(string year, string albumName, string[] files)
        {
            var photoIds = new List<string>();
            var yearDirectory = Path.GetRelativePath(_basePath, year);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var photoPath = $"{yearDirectory}/{fileName}";

                Console.WriteLine($"Getting photo ID for path {photoPath}");
                var id = await _client.GetPhotoUid(photoPath);
                if (id != null)
                {
                    photoIds.Add(id);
                }
            }

            Console.WriteLine($"Creating album for name {albumName}");
            var albumId = await _client.CreateAlbum(albumName);
            await _client.AddPhotosToAlbum(albumId, photoIds.ToArray());
        }
    }
}
