using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseWebApp.Models;

namespace ExpenseWebApp.Data.Repositories.Interfaces
{
    public interface IExpenseAdvanceRepository : IGenericRepository<ExpenseAdvance>
    {
        List<ExpenseAdvance> GetAllExpenseAdvanceForms();
        Task<ExpenseAdvance> GetExpenseAdvanceByFormNoAsync(string formNo);
        Task<ExpenseAdvance> GetExpenseAdvanceByIdAsync(string formId);
        IQueryable<ExpenseAdvance> GetUserExpenseAdvanceForms(int userId);
    }
}
