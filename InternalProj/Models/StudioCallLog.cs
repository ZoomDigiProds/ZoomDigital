using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InternalProj.Models
{
    public class StudioCallLog
    {
        [Key]
        public int CallId { get; set; }

        // FK to Customer
        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        [ValidateNever]
        public CustomerReg? Customer { get; set; }

        // Flattened fields
        [StringLength(255)]
        public string? StudioName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }
        [StringLength(255)]
        public string? Region { get; set; }
        public DateTime CallTime { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? UpdatedCallTime { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("Y|N")]
        public string? Active { get; set; }
    }
}