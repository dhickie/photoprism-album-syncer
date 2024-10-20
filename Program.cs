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
                    var yearDirectory = Path.GetRelativePath(_basePath, year);
                    var albumName = $"{yearDirectory} - {Path.GetRelativePath(year, album)}";
                    var files = Directory.GetFiles(album);
                    await CreateAlbum(yearDirectory, albumName, files);
                }
            }
        }

        private static async Task CreateAlbum(string year, string albumName, string[] files)
        {
            var photoIds = new List<string>();
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var yearInt = int.Parse(year);
                var photoPath = GeneratePhotoPath(yearInt, fileName);

                Console.WriteLine($"Getting photo ID for path {photoPath}");
                var id = await _client.GetPhotoUid(photoPath);
                if (id == null)
                {
                    // Try one year before and after for albums that span years
                    id = await _client.GetPhotoUid(GeneratePhotoPath(yearInt - 1, fileName));

                    if (id == null)
                    {
                        id = await _client.GetPhotoUid(GeneratePhotoPath(yearInt + 1, fileName));
                    }
                }

                if (id != null)
                {
                    photoIds.Add(id);
                }
                else
                {
                    Console.WriteLine($"WARN: Unable to find photo UID for filename {photoPath}");
                }
            }

            Console.WriteLine($"Creating album for name {albumName}");
            var albumId = await _client.CreateAlbum(albumName);
            await _client.AddPhotosToAlbum(albumId, photoIds.ToArray());
        }

        private static string GeneratePhotoPath(int year, string fileName)
        {
            return $"{year}/{fileName}";
        }
    }
}
