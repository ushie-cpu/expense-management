using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWebApp.Dtos.ExpenseFormDto
{
    public class ExpenseFormApprovalDto
    {
        public string Note { get; set; }
        [Required]
        public string CacNumber { get; set; }

        [Required]
        public string ApprovedBy { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public bool IsApproved { get; set; }
        public bool IsDetailsRequired { get; set; }
    }
}
