using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Models.Entity
{
    public class PaperEntity
    {

        [Key]
        public Guid Id { get; set; }
        public string? Authors { get; set; } 
        public string? Title { get; set; }
        public string? Publisher { get; set; }
        public string? DOI { get; set; } 
        public string? FullTextLink { get; set; }
        public string? Abstract { get; set; }
        public string? Year { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Inserted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime LastUpdated { get; set; }

    }
}
