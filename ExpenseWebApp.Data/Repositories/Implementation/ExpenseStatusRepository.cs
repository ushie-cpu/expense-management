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
    public class ExpenseStatusRepository : GenericRepository<ExpenseStatusRepository>, IExpenseStatusRepository
    {
        private readonly ExpenseDbContext _dbContext;

        public ExpenseStatusRepository(ExpenseDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ExpenseStatus> GetStatusByIdAsync(string id)
        {
            return await _dbContext.ExpenseStatus
                                    .SingleOrDefaultAsync(x => x.ExpenseStatusId == id);
        }


        /// <summary>
        /// Gets all advance retirement forms
        /// </summary>
        /// <returns>all advance retirement forms</returns>
        public async Task<ExpenseStatus> GetExpenseStatusByDescriptionAsync(string description)
        {
            return await _dbContext.ExpenseStatus                            
                                    .SingleOrDefaultAsync(x => x.Description == description);

        }

    }
}
