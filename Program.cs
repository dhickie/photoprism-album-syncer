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

            _basePath = Assembly.GetExecutingAssembly().Location;
            await WalkDirectoryTree(_basePath);
        }

        private static async Task WalkDirectoryTree(string currentPath)
        {
            // Walk the directory tree until we hit one that contains files
            var directories = Directory.GetDirectories(currentPath);
            var files = Directory.GetFiles(currentPath);

            Console.WriteLine($"Scanning directory {currentPath}");

            if (files.Length > 0)
            {
                Console.WriteLine($"Found {files.Length} files");
                var albumName = Path.GetDirectoryName(currentPath);
                await CreateAlbum(albumName, files);
            }

            Console.WriteLine($"Scanning {directories.Length} sub-directories");
            foreach (var directory in directories)
            {
                await WalkDirectoryTree(directory);
            }
        }

        private static async Task CreateAlbum(string albumName, string[] files)
        {
            var photoIds = new List<string>();
            foreach (var file in files)
            {
                var photoPath = Path.GetRelativePath(_basePath, file);
                photoPath = photoPath.TrimStart('\\');

                Console.WriteLine($"Getting photo ID for path {photoPath}");
                photoIds.Add(await _client.GetPhotoUid(photoPath));
            }

            Console.WriteLine($"Creating album for name {albumName}");
            var albumId = await _client.CreateAlbum(albumName);
            await _client.AddPhotosToAlbum(albumId, photoIds.ToArray());
        }
    }
}
