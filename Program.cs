using System.Reflection;

namespace PhotoPrismAlbumSyncer
{
    internal class Program
    {
        private static Config _config;
        private static PhotoPrismClient _client;
        private static string _basePath;

        static async Task Main(string[] args)
        {
            _config = new Config();
            var httpClient = new HttpClient();
            _client = new PhotoPrismClient(httpClient, _config);

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
                photoIds.Add(await _client.GetPhotoUid(photoPath));
            }

            Console.WriteLine($"Creating album for name {albumName}");
            var albumId = await _client.CreateAlbum(albumName);
            await _client.AddPhotosToAlbum(albumId, photoIds.ToArray());
        }
    }
}
