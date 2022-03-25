using ExpenseWebApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseWebApp.Data.Repositories.Interfaces
{
    public interface IExpenseFormDetailsRepository : IGenericRepository<ExpenseFormDetails>
    {
        List<ExpenseFormDetails> GetAllExpenseFormDetails();
        Task<ExpenseFormDetails> GetExpenseFormDetailsByIdAsync(string formId);
        Task<List<ExpenseFormDetails>> GetAllExpenseFormDetailsByFormIdAsync(string formId);
    }
}
