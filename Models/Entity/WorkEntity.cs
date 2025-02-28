using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Models.Entity
{
    public class WorkEntity
    {

        [Key]
        public Guid Id { get; set; }
        public string Authors { get; set; }

        public string Title { get; set; }

        public string Publisher { get; set; }

        public string DOI { get; set; }

        public string FullTextLink { get; set; }

        public string Abstract {  get; set; }   

        public string Year { get; set; }

    }
}
