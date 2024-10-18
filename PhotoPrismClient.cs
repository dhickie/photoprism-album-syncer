using System.Text.Json;

namespace PhotoPrismAlbumSyncer
{
    public class PhotoPrismClient
    {
        private HttpClient _httpClient;
        private Config _config;

        public PhotoPrismClient(HttpClient httpClient, Config config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> CreateAlbum(string albumName)
        {
            return Guid.NewGuid().ToString();
            //var path = "api/v1/albums";
            //var body = new CreateAlbumRequest()
            //{
            //    Title = albumName
            //};
            //
            //var response = await MakeRequest<CreateAlbumRequest, CreateAlbumResponse>(body, path, HttpMethod.Post);
            //return response.UID;
        }

        public async Task<string> GetPhotoUid(string photoPath)
        {
            return Guid.NewGuid().ToString();
            //var path = $"api/v1/photos?count=1&q=filename:{photoPath}";
            //
            //var response = await MakeRequest<GetPhotoResponse>(path, HttpMethod.Get);
            //return response.UID;
        }

        public async Task<bool> AddPhotosToAlbum(string albumUid, string[] photoUids)
        {
            return true;
            //var path = $"api/v1/albums/{albumUid}/photos";
            //var body = new AddPhotosToAlbumRequest()
            //{
            //    All = true,
            //    Photos = photoUids
            //};
            //
            //var response = await MakeRequest<AddPhotosToAlbumRequest, AddPhotosToAlbumResponse>(body, path, HttpMethod.Post);
            //if (response.Photos.Length == photoUids.Length)
            //{
            //    return true;
            //}
            //
            //return false;
        }

        private async Task<TResponse> MakeRequest<TRequest,TResponse>(TRequest body, string path, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_config.PhotoPrismUrl}/{path}"),
                Content = new StringContent(JsonSerializer.Serialize(body))
            };

            return await MakeRequest<TResponse>(request);
        }

        private async Task<TResponse> MakeRequest<TResponse>(string path, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_config.PhotoPrismUrl}/{path}")
            };

            return await MakeRequest<TResponse>(request);
        }

        private async Task<TResponse> MakeRequest<TResponse>(HttpRequestMessage request)
        {
            request.Headers.Add("Authorization", $"Bearer {_config.AuthToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseBody);
        }
    }
}
