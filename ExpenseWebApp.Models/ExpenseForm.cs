﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ExpenseWebApp.Models
{
    public class ExpenseForm
    {
        [Key]
        public string ExpenseFormId { get; set; } = Guid.NewGuid().ToString();
        public int UserId { get; set; }
        public string ExpenseFormNo { get; set; }
        public string Description { get; set; }
        public decimal ReimburseableAmount { get; set; }
        public DateTime? ReimbursementDate { get; set; }
        public DateTime DateCreated { get; set; }
        public string PaidBy { get; set; }
        public int CompanyId { get; set; }
        public string FundedAccount { get; set; }
        public string ApprovedBy { get; set; }
        public string ApproverNote { get; set; }
        

        // Navigation Properties
        public string ExpenseStatusId { get; set; }
        public ExpenseStatus ExpenseStatus { get; set; }

        // Navigation Properties
        public string ExpenseAdvanceId { get; set; }
        public ExpenseAdvance ExpenseAdvance { get; set; }
        public ICollection<AdvanceRetirement> AdvanceRetirement { get; set; }
        public ICollection<ExpenseFormDetails> ExpenseFormDetails { get; set; }

    }
}
