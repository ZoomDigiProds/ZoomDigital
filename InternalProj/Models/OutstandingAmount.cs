using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.Models
{
    public class OutstandingAmount
    {
        [Key]
        public int Id { get; set; }
                
        public int CustomerId { get; set; }
        public CustomerReg Customer { get; set; }

        public decimal TotalOutstanding { get; set; }
    }

}
