﻿using ExpenseWebApp.Data.ContextClass;
using ExpenseWebApp.Data.Repositories.Interfaces;
using ExpenseWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWebApp.Data.Repositories.Implementation
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly ExpenseDbContext _context;
        private readonly DbSet<Notification> _dbSet;
        public NotificationRepository(ExpenseDbContext context) : base(context)
        {
            _context = context;
            _dbSet =  _context.Set<Notification>();
        }
        public async Task<List<Notification>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetAllByCompanyIdAsync(int companyId)
        {
            return await _dbSet.Where(q => q.CompanyId == companyId).ToListAsync();
        }
    }
}
