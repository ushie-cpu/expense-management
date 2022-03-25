using ExpenseWebApp.Dtos.CompanyDtos;
using ExpenseWebApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWebApp.Core.Interfaces
{
    public interface ICompanyService
    {
        Task<ClientResponse<GetCompanyResponseDto>> GetCompanyAsync(string cacNumber, string token = null);
        Task<ClientResponse<IEnumerable<GetCompanyUsersDto>>> GetCompanyUsersAsync(string cacNumber, string token = null);
    }
}
