using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ImageRestApi.Controllers
{
    public class RequestForm
    {
        /// <summary>
        /// json array of base64 strings
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