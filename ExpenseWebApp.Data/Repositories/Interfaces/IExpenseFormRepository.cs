using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseWebApp.Models;

namespace ExpenseWebApp.Data.Repositories.Interfaces
{
    public interface IExpenseFormRepository : IGenericRepository<ExpenseForm>
    {
        List<ExpenseForm> GetAllExpenseForms();
        Task<ExpenseForm> GetExpenseFormByIdAsync(string formId);
        Task<ExpenseForm> GetExpenseFormByIdWithoutOthersAsync(string formId);
        IQueryable<ExpenseForm> GetExpenseFormsByCompanyId(int companyId);
        Task<ExpenseForm> GetExpenseFormAsync(string formId);
        void UpdateFormStatus(ExpenseForm expenseForm);
        IQueryable<ExpenseForm> GetUserExpenseForms(int userId);
        IQueryable<ExpenseForm> GetAllExpenseFormsByStatus(string status);
    }
}
