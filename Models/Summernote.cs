using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models
{
    public class Summernote
    {
        public Summernote(string id, bool loadLibrary = true, int height = 120)
        {
            Id = id;
            LoadLibrary = loadLibrary;
            Height = height;
        }

        public string Id { set; get; }
        public bool LoadLibrary { set; get; }
        public int Height { set; get; }
        public string Toolbar { set; get; } = @"
            [
                ['style', ['style']],
                ['font', ['bold', 'underline', 'clear']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['table', ['table']],
                ['insert', ['link', 'picture', 'video', 'elfinder']],
                ['view', ['fullscreen', 'codeview', 'help']]
            ]
        ";
    }
}