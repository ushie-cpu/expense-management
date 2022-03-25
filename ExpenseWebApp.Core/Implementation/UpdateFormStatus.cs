using ExpenseWebApp.Core.Interfaces;
using ExpenseWebApp.Data.ContextClass;
using ExpenseWebApp.Data.UnitOfWork.Abstractions;
using ExpenseWebApp.Models;
using ExpenseWebApp.Utilities;
using ExpenseWebApp.Utilities.ResourceFiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWebApp.Core.Implementation
{
    public class UpdateFormStatus : IUpdateFormStatus
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ExpenseDbContext _dbContext;

        public UpdateFormStatus(IUnitOfWork unitOfWork, ExpenseDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        /// <summary>
        /// This method update's the status of the form to approve
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>bool</returns>
        public async Task<Response<ExpenseForm>> SetApprovedStatusAsync(string formId, string approverName)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormByIdWithoutOthersAsync(formId);
            if (expenseForm == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            var formStatus = await _unitOfWork.ExpenseStatus.GetStatusByIdAsync(expenseForm.ExpenseStatusId);
            if (formStatus == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            if (formStatus.Description.ToLower() == FormStatus.PendingApproval.ToLower()
                || formStatus.Description.ToLower() == FormStatus.FurtherInfoRequired.ToLower())
            {
                var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.Approved);

                if (status == null) return Response<ExpenseForm>.Fail(ResourceFile.StatusNotFound);

                expenseForm.ApprovedBy = approverName;

                return await UpdateStatus(expenseForm, status);
            }

            return Response<ExpenseForm>.Fail("Cannot perform this operation on the form", 400);
            
        }

        /// <summary>
        /// This method update's the status of the form to cancelled
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>bool</returns>
        public async Task<Response<ExpenseForm>> SetCancelledStatusAsync(string formId)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormAsync(formId);
            if (expenseForm == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.Cancelled);

            if (status == null) return Response<ExpenseForm>.Fail(ResourceFile.StatusNotFound);

            return await UpdateStatus(expenseForm, status);
        }

        /// <summary>
        /// This method update's the status of the form to disbursed
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>bool</returns>
        public async Task<Response<ExpenseForm>> SetDisbursedStatusAsync(string formId)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormAsync(formId);
            if (expenseForm == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            if (expenseForm.ExpenseStatus.Description.ToLower() != FormStatus.Approved.ToLower())
                return Response<ExpenseForm>.Fail("Cannot perform this operation on the form");

            var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.Disbursed);

            if (status == null) return Response<ExpenseForm>.Fail(ResourceFile.StatusNotFound);

            return await UpdateStatus(expenseForm, status);
        }

        /// <summary>
        /// This method update's the status of the form to further info required
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>bool</returns>
        public async Task<Response<ExpenseForm>> SetFurtherInfoRequiredStatusAsync(string formId, string approverNote)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormAsync(formId);
            if (expenseForm == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            if (expenseForm.ExpenseStatus.Description.ToLower() != FormStatus.PendingApproval.ToLower())
                return Response<ExpenseForm>.Fail("Cannot perform this operation on the form");

            var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.FurtherInfoRequired);

            if (status == null) return Response<ExpenseForm>.Fail(ResourceFile.StatusNotFound);

            return await UpdateStatus(expenseForm, status, approverNote);

        }

        /// <summary>
        /// This method update's the status of the form to new request
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>bool</returns>
        public async Task<Response<ExpenseForm>> SetNewRequestStatusAsync(string formId)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormAsync(formId);
            if (expenseForm == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.NewRequest);

            if (status == null) return Response<ExpenseForm>.Fail(ResourceFile.StatusNotFound);

            return await UpdateStatus(expenseForm, status);
        }

        /// <summary>
        /// This method update's the status of the form to pending approval
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>bool</returns>
        public async Task<Response<ExpenseForm>> SetPendingApprovalStatusAsync(string formId)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormAsync(formId);
            if (expenseForm == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.PendingApproval);

            if (status == null) return Response<ExpenseForm>.Fail(ResourceFile.StatusNotFound);

            return await UpdateStatus(expenseForm, status);
        }

        /// <summary>
        /// This method update's the status of the form to rejected
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>bool</returns>
        public async Task<Response<ExpenseForm>> SetRejectedStatusAsync(string formId, string approverNote)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormAsync(formId);
            if (expenseForm == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            if (expenseForm.ExpenseStatus.Description.ToLower() == FormStatus.PendingApproval.ToLower()
                || expenseForm.ExpenseStatus.Description.ToLower() == FormStatus.FurtherInfoRequired.ToLower())
            {
                var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.Rejected);

                if (status == null) return Response<ExpenseForm>.Fail(ResourceFile.StatusNotFound);

                return await UpdateStatus(expenseForm, status, approverNote);
            }
                return Response<ExpenseForm>.Fail("Cannot perform this operation on the form");

            
        }

        /// <summary>
        /// This method update's the status of the form to "to be submitted"
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>bool</returns>
        public async Task<Response<ExpenseForm>> SetToBeSubmittedAsync(string formId)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormAsync(formId);
            if (expenseForm == null) return Response<ExpenseForm>.Fail(ResourceFile.FormNotFound);

            var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.ToBeSubmitted);

            if (status == null) return Response<ExpenseForm>.Fail(ResourceFile.StatusNotFound);

            return await UpdateStatus(expenseForm, status);
        }

        private async Task<Response<ExpenseForm>> UpdateStatus(ExpenseForm expenseForm, ExpenseStatus status, string approverNote = null)
        {
            expenseForm.ExpenseStatusId = status.ExpenseStatusId;
            expenseForm.ExpenseStatus = status;

            if(!string.IsNullOrEmpty(approverNote))
            {
                expenseForm.ApproverNote = approverNote;
            }
            
            bool result = await UpdateDatabase(expenseForm);

            if (!result) return Response<ExpenseForm>.Fail(ResourceFile.FormUpdateFailed);

            return Response<ExpenseForm>.Success(ResourceFile.FormUpdateSuccess, expenseForm);
        }

        /// <summary>
        /// This method updates the DB with the changes made to the update column in the database
        /// </summary>
        /// <returns>int</returns>
        private async Task<bool> UpdateDatabase(ExpenseForm expenseForm)
        {
            try
            {
                _dbContext.Entry(expenseForm).State = EntityState.Modified;

                _unitOfWork.ExpenseForm.Update(expenseForm); //Throws an exception because the ExpenseCategory is already been tracked.
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}

                    


