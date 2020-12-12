using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ImageProcessor;
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
        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
            _imageHttpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,image/jpeg,*/*;q=0.8");

        }

        [HttpPost]
        public async Task<IActionResult> PostImage([FromForm]RequestForm request)
        {
            await SaveFormFiles(request);
            await DownloadImage(request.ImageUrl);
            await SaveBase64Image(request.Json);
        
            return Ok();
        }

        private async Task SaveFormFiles(RequestForm request)
        {
            if (request.Files?.Any() == true)
            {
                _logger.LogInformation($"Downloading {request.Files.Count} files");
            }

            request.Files?.ForEach(
                async formFile =>
                {
                    if (formFile.Length > 0)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(formFile.FileName));
                        using var imageFactory = new ImageFactory();
                        imageFactory
                            .Load(formFile.OpenReadStream())
                            .Resize(new Size(100, 100))
                            .Save(GetPreviewFilePath(filePath));
                        await using var stream = System.IO.File.Create(filePath); 
                        await formFile.CopyToAsync(stream);
                    }
                }
            );
        }

        private async Task SaveBase64Image(string requestJson)
        {
            if (string.IsNullOrEmpty(requestJson))
            {
                return;
            }
            var base64Values = JsonConvert.DeserializeObject<List<string>>(requestJson);
            _logger.LogInformation($"Saving {base64Values.Count} base64 images");

            base64Values.ForEach(async base64Value =>
                {
                    if (base64Value.Contains(','))
                    {
                        base64Value = base64Value.Substring(base64Value.IndexOf(",") + 1);
                    }
                    var bytes = Convert.FromBase64String(base64Value);
                    await SaveImage(bytes, "base64");
                }
            );
        }

        private async Task DownloadImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            _logger.LogInformation($"Downloading image by url {imageUrl}");
            var bytes =await _imageHttpClient.GetByteArrayAsync(imageUrl);

            await SaveImage(bytes, "url");
        }

        private async Task SaveImage(byte[] bytes, string downloadType)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName($"{downloadType}-{Guid.NewGuid().ToString()}.jpg"));
            await using var memoryStream = new MemoryStream(bytes);
            await using var stream = System.IO.File.Create(filePath); 
            using var imageFactory = new ImageFactory();
            imageFactory.Load(memoryStream)
                .Resize(new Size(100, 100))
                .Save(GetPreviewFilePath(filePath));
            await memoryStream.CopyToAsync(stream);
        }

        private string GetPreviewFilePath(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            return $"{filePath.Substring(0, filePath.Length - extension.Length)}-preview{extension}";
        }
    }
}