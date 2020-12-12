using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ImageRestApi.Controllers;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Newtonsoft.Json;

namespace ImageRestApi.Tests
{
    public class DownloadingTests
    {
        [Fact]
        public async Task SaveBase64Image()
        {
            var fs = Path.Combine(Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf("bin")), "base64image.txt");
            var imageController = new ImageController(new Mock<ILogger<ImageController>>().Object);
            var base64 = await File.ReadAllTextAsync(fs);
            
            await imageController.PostImage(new RequestForm
            {
                Json = JsonConvert.SerializeObject(new List<string> {base64})
            });
            var files = Directory.GetFiles(Directory.GetCurrentDirectory())
                .Where(x => x.Contains("base64"))
                .ToList();
            try
            {
                files.Count.Should().Be(2);
                var preview = Image.FromFile(files.First(x => x.Contains("preview")));
                preview.Height.Should().Be(100);
                preview.Width.Should().Be(100);
                preview.Dispose();
            }
            finally
            {
                GC.Collect(); 
                GC.WaitForPendingFinalizers(); 
                files.ForEach(File.Delete);
            }
        }
    }
}