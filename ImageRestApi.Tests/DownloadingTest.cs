using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ImageRestApi.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ImageRestApi.Tests
{
    public class Base64Tests{
        [Fact]
        public async Task SaveImageFromUrl()
        {
            var imageController = new ImageController(new Mock<ILogger<ImageController>>().Object);
            await imageController.PostImage(new RequestForm
            {
                ImageUrl = "https://i.ytimg.com/vi/1Ne1hqOXKKI/maxresdefault.jpg"
            });
            var files = Directory.GetFiles(Directory.GetCurrentDirectory())
                .Where(x => x.Contains("url"))
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