using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary.Entities
{
    public class Vacation : OtherBaseEntity
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public int NumberOfDays { get; set; }
        public DateTime DateTime => StartDate.AddDays(NumberOfDays);

        //Many to one relationship with Vacation Type
        public VacaionType? VacaionType { get; set; }
        [Required]
        public int VacationTypeId { get; set; }
    }
}
