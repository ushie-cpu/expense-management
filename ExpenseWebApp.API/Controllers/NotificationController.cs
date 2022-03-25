using ExpenseWebApp.Core.Interfaces;
using ExpenseWebApp.Dtos.NotificationDtos;
using ExpenseWebApp.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseWebApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// The Endpoint to retrieve approver notifications
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet("Approver-Notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Response<IEnumerable<NotificationDto>>>> GetApproverNotifications(int companyId)
        {
           var result = await _notificationService.GetApproverNotificationsAsync(companyId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// The Endpoint to retrieve disburser notification
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet("Disburser-Notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Response<IEnumerable<NotificationDto>>>> GetDisburserNotifications(int companyId)
        {
            var result = await _notificationService.GetDisburserNotificationsAsync(companyId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// The Endpoint to retrieve notification of a form creator
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet("FormCreator-Notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Response<IEnumerable<NotificationDto>>>> GetFormCreatorNotifications(int userId, int companyId)
        {
            var result = await _notificationService.GetFormCreatorNotificationsAsync(userId, companyId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
