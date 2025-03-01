using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entity
{
    public class StarredEntity : BaseEntity
    {

        public Guid? PaperEntityId { get; set; }
        public PaperEntity? PaperEntity { get; set; }

    }
}
