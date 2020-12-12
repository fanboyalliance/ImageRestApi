using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ImageRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private static HttpClient _imageHttpClient = new HttpClient();
        private static MagickGeometry _resizer = new MagickGeometry(100, 100) {IgnoreAspectRatio = true};

        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
            _imageHttpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,image/jpeg,*/*;q=0.8");

        }

        [HttpPost]
        public async Task<IActionResult> PostImage([FromForm]RequestForm request)
        {
            try
            {
                await SaveFormFiles(request);
                await DownloadImage(request.ImageUrl);
                await SaveBase64Image(request.Json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
                throw;
            }
        
            return Ok();
        }

        private async Task SaveFormFiles(RequestForm request)
        {
            if (request.Files == null)
            {
                return;
            }
            _logger.LogInformation($"Downloading {request.Files.Count} files");

            foreach (var formFile in request.Files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Upload-{Guid.NewGuid().ToString()}.jpg");
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    
                    using (var magickImage = new MagickImage(formFile.OpenReadStream()))
                    {
                        magickImage.Resize(_resizer);
                        magickImage.Write(GetPreviewFilePath(filePath));
                    }
                }
            }
        }

        private async Task SaveBase64Image(string requestJson)
        {
            if (string.IsNullOrEmpty(requestJson))
            {
                return;
            }
            var base64Values = JsonConvert.DeserializeObject<List<string>>(requestJson);
            _logger.LogInformation($"Saving {base64Values.Count} base64 images");

            foreach (var base64Value in base64Values)
            {
                var normalizeBase64 = base64Value;
                if (normalizeBase64.Contains(','))
                {
                    normalizeBase64 = base64Value.Substring(base64Value.IndexOf(",") + 1);
                }
                var bytes = Convert.FromBase64String(normalizeBase64);
                await SaveImage(bytes, "base64");
            }
        }

        private async Task DownloadImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            _logger.LogInformation($"Downloading image by url {imageUrl}");
            var bytes = await _imageHttpClient.GetByteArrayAsync(imageUrl);

            await SaveImage(bytes, "url");
        }

        private async Task SaveImage(byte[] bytes, string downloadType)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName($"{downloadType}-{Guid.NewGuid().ToString()}.jpg"));
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var stream = System.IO.File.Create(filePath))
                {
                    await memoryStream.CopyToAsync(stream);
                }
            }

            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var magickImage = new MagickImage(memoryStream))
                {
                    magickImage.Resize(_resizer);
                    magickImage.Write(GetPreviewFilePath(filePath));
                }
            }
        }

        private string GetPreviewFilePath(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            return $"{filePath.Substring(0, filePath.Length - extension.Length)}-preview{extension}";
        }
    }
}