using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entity
{
    public class MyDocuments : BaseEntity
    {
        public string? Authors { get; set; }
        public string? Title { get; set; }
        public string? Publisher { get; set; }
        public string? Year { get; set; }
        public byte[]? FileData { get; set; } // BLOB to store pdf
    }
}
