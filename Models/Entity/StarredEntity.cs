using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entity
{
    public class StarredEntity : BaseEntity
    {

        public Guid? PaperEntityId { get; set; }
        public PaperEntity? PaperEntity { get; set; }

        public ColorFilters ColorFilter { get; set; }

    }
}
