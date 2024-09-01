

namespace BaseLibrary.Entities
{
    public class SanctionType : BaseEntity
    {
        //Many to one relationship with Sanction
        public List<Sanction>? Sanctions { get; set; }

    }
}
