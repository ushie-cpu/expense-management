using ExpenseWebApp.Models;
using ExpenseWebApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWebApp.Core.Interfaces
{
    public interface IUpdateFormStatus
    {
        Task<Response<ExpenseForm>> SetApprovedStatusAsync(string formId, string approverName);
        Task<Response<ExpenseForm>> SetCancelledStatusAsync(string formId);
        Task<Response<ExpenseForm>> SetDisbursedStatusAsync(string formId);
        Task<Response<ExpenseForm>> SetFurtherInfoRequiredStatusAsync(string formId, string approverNote);
        Task<Response<ExpenseForm>> SetNewRequestStatusAsync(string formId);
        Task<Response<ExpenseForm>> SetPendingApprovalStatusAsync(string formId);
        Task<Response<ExpenseForm>> SetRejectedStatusAsync(string formId, string approverNote);
        Task<Response<ExpenseForm>> SetToBeSubmittedAsync(string formId);
    }
}
