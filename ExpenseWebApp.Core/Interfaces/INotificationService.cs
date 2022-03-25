using ExpenseWebApp.Dtos.NotificationDtos;
using ExpenseWebApp.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseWebApp.Core.Interfaces
{
    public interface INotificationService
    {
        Task<Response<IEnumerable<NotificationDto>>> GetApproverNotificationsAsync(int companyId);
        Task<Response<IEnumerable<NotificationDto>>> GetDisburserNotificationsAsync(int companyId);
        Task<Response<IEnumerable<NotificationDto>>> GetFormCreatorNotificationsAsync(int userId, int companyId);
        Task SendNotificationToFormCreatorAsync(NotificationCreateDto notificationCreateDto, string cacNumber, string token = null);
        Task SendNotificationToDisburserAsync(NotificationCreateDto notificationCreateDto, string cacNumber, string token = null);
        Task SendNotificationToApproverAsync(NotificationCreateDto notificationCreateDto, string cacNumber, string token = null);
    }
}
