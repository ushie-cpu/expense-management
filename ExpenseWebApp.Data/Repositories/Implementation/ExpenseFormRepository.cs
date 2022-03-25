using ExpenseWebApp.Data.ContextClass;
using ExpenseWebApp.Data.Repositories.Interfaces;
using ExpenseWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseWebApp.Data.Repositories.Implementation
{
    public class ExpenseFormRepository : GenericRepository<ExpenseForm>, IExpenseFormRepository
    {
        private readonly ExpenseDbContext _dbContext;
        public ExpenseFormRepository(ExpenseDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;

        }



        /// <summary>
        /// Gets all expense forms
        /// </summary>
        /// <returns>all expense forms and their status</returns>
        public List<ExpenseForm> GetAllExpenseForms()
        {
            return _dbContext.ExpenseForms
                                   .Include(x => x.ExpenseAdvance)
                                    .Include(x => x.ExpenseStatus)
                                   .Include(x => x.AdvanceRetirement)
                                   .Include(x => x.ExpenseFormDetails).ThenInclude(x => x.ExpenseCategory)
                                   .ToList();
        }

        /// <summary>
        /// Gets all expense forms
        /// </summary>
        /// <returns>all expense forms and their status</returns>
        public IQueryable<ExpenseForm> GetAllExpenseFormsByStatus(string status)
        {
            return _dbContext.ExpenseForms
                                   .Include(x => x.ExpenseAdvance)
                                   .Include(x => x.ExpenseStatus)
                                   .Include(x => x.AdvanceRetirement)
                                   .Include(x => x.ExpenseFormDetails).ThenInclude(x => x.ExpenseCategory)
                                   .Where(x => x.ExpenseStatus.Description.ToLower().Trim() == status.ToLower().Trim());
        }

        /// <summary>
        /// Gets all expense forms that belongs to the provided user
        /// </summary>
        /// <returns>all expense forms and their status</returns>
        public IQueryable<ExpenseForm> GetUserExpenseForms(int userId)
        {
            return _dbContext.ExpenseForms
                                   .Include(x => x.ExpenseAdvance)
                                   .Include(x => x.ExpenseStatus)
                                   .Include(x => x.AdvanceRetirement)
                                   .Include(x => x.ExpenseFormDetails).ThenInclude(x => x.ExpenseCategory)
                                   .Where(x => x.UserId == userId)
                                   .OrderByDescending(x => x.DateCreated);
        }

        /// <summary>
        /// Gets an expense form by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A single expense form</returns>
        public async Task<ExpenseForm> GetExpenseFormByIdAsync(string formId)
        {

            return await _dbContext.ExpenseForms
                                    .Include(x => x.ExpenseAdvance)
                                    .Include(x => x.ExpenseStatus)
                                    .Include(x => x.AdvanceRetirement)
                                    .Include(x => x.ExpenseFormDetails).ThenInclude(x => x.ExpenseCategory)
                                    .FirstOrDefaultAsync(x => x.ExpenseFormId == formId);
        }

        public IQueryable<ExpenseForm> GetExpenseFormsByCompanyId(int companyId)
        {
            return _dbContext.ExpenseForms
                             .Include(x => x.ExpenseStatus)
                             .Include(x => x.ExpenseAdvance)
                             .Include(x => x.ExpenseFormDetails).ThenInclude(x => x.ExpenseCategory)
                             .Where(x => x.CompanyId == companyId);
        }

        public async Task<ExpenseForm> GetExpenseFormAsync(string formId)
        {
            return await _dbContext.ExpenseForms
                .Include(x => x.ExpenseStatus)
                .Include(x => x.ExpenseAdvance)
                .Include(x => x.ExpenseFormDetails).ThenInclude(x => x.ExpenseCategory)
                .SingleOrDefaultAsync(x => x.ExpenseFormId == formId);
        }

        public void UpdateFormStatus(ExpenseForm expenseForm)
        {
            _dbContext.Update(expenseForm.ExpenseStatus);
            _dbContext.Entry(expenseForm).Reference(x => x.ExpenseStatus).IsModified = true;

            _dbContext.SaveChanges();
        }

        public async Task<ExpenseForm> GetExpenseFormByIdWithoutOthersAsync(string formId)
        {
            return await _dbContext.ExpenseForms
                                    .FirstOrDefaultAsync(x => x.ExpenseFormId == formId);
        }
    }
}