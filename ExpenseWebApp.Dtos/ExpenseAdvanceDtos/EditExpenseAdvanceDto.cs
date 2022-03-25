using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseWebApp.Dtos.ExpenseAdvanceDtos
{
    public class EditExpenseAdvanceDto
    {
        [Required]
        public string AdvanceDescription { get; set; }
        [Required]
        public decimal AdvanceAmount { get; set; }
        public string AdvanceNote { get; set; }
        [Required]
        public DateTime AdvanceDate { get; set; }
        [Required]
        public string Attachments { get; set; }
    }
}
