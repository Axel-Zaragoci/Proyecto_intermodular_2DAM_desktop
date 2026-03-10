using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktop_app.Models
{
    public class UploadFileDto
    {
        public string Id { get; set; } = "";
        public string OriginalName { get; set; } = "";
        public string Mimetype { get; set; } = "";
        public long Size { get; set; }
        public string Url { get; set; } = ""; // "/uploads/xxx.jpg"
    }

    public class UploadManyResponse
    {
        public List<UploadFileDto> Files { get; set; } = new();
    }

}

