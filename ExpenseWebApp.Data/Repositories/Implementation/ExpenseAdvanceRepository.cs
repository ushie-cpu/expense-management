using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseWebApp.Data.ContextClass;
using ExpenseWebApp.Data.Repositories.Interfaces;
using ExpenseWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseWebApp.Data.Repositories.Implementation
{
    public class ExpenseAdvanceRepository : GenericRepository<ExpenseAdvance>, IExpenseAdvanceRepository
    {
        private readonly ExpenseDbContext _dbContext;
        public ExpenseAdvanceRepository(ExpenseDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Gets an expense advance form
        /// </summary>
        /// <param name="id"></param>
        /// <returns>a single form</returns>
        public async Task<ExpenseAdvance> GetExpenseAdvanceByIdAsync(string formId)
        {
            return await _dbContext.ExpenseAdvance
                                   .Include(x => x.AdvanceRetirement)
                                   .Include(x => x.PaidFrom)
                                   .Include(x => x.ExpenseStatus)
                                   .FirstOrDefaultAsync(x => x.AdvanceFormId == formId);
        }

        /// <summary>
        /// Gets Advance Form By Form Number
        /// </summary>
        /// <param name="formNo"></param>
        /// <returns></returns>
        public async Task<ExpenseAdvance> GetExpenseAdvanceByFormNoAsync(string formNo)
        {
            return await _dbContext.ExpenseAdvance
                                   .FirstOrDefaultAsync(x => x.AdvanceFormNo == formNo);
        }

        /// <summary>
        /// Gets all expense advance forms
        /// </summary>
        /// <returns>all expense advance forms</returns>
        public List<ExpenseAdvance> GetAllExpenseAdvanceForms()
        {
            return _dbContext.ExpenseAdvance
                                   .Include(x => x.ExpenseStatus)
                                   .Include(x => x.AdvanceRetirement)
                                   .Include(x => x.PaidFrom)
                                   .ToList();
        }

        public IQueryable<ExpenseAdvance> GetUserExpenseAdvanceForms(int userId)
        {
            return _dbContext.ExpenseAdvance
                .Include(x => x.ExpenseStatus)
                .Include(x => x.AdvanceRetirement)
                .Include(x => x.PaidFrom)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AdvanceDate);
        }


    }
}
