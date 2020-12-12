using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ImageRestApi.Controllers
{
    public class RequestForm
    {
        /// <summary>
        /// json of base64 string arrays
        /// </summary>
        public string Json { get; set; }

        /// <summary>
        /// uploading files
        /// </summary>
        public List<IFormFile> Files { get; set; }
        
        /// <summary>
        /// downloading image url
        /// </summary>
        public string ImageUrl { get; set; }
    }
}