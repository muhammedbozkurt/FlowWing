﻿using FlowWing.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowWing.DataAccess.Abstract
{
    public interface IEmailLogRepository
    {
        Task<EmailLog> CreateEmailLogAsync(EmailLog emailLog);
        EmailLog CreateEmailLog(EmailLog emailLog);
        Task<EmailLog> UpdateEmailLogAsync(EmailLog emailLog);
        EmailLog UpdateEmailLog(EmailLog emailLog);
        Task<EmailLog> DeleteEmailLogAsync(EmailLog emailLog);
        Task<IEnumerable<EmailLog>> GetEmailLogsByRepeatingLogIdAsync(int repeatingLogId);
        Task<EmailLog> GetEmailLogByIdAsync(int? id);
        EmailLog GetEmailLogById(int id);
        Task<IEnumerable<EmailLog>> GetAllEmailLogsAsync();
        Task<IEnumerable<EmailLog>> GetEmailLogsByUserIdAsync(int userId);
        Task <EmailLog> GetEmailLogByScheduledEmailIdAsync(int scheduledEmailId);
        Task<IEnumerable<EmailLog>> GetEmailLogsByRecipientsEmailAsync(string recipientEmail);
        Task<IEnumerable<EmailLog>> GetNotDeletedEmailLogsByRecipientsEmailAsync(string recipientEmail);
        Task<IEnumerable<EmailLog>> GetEmailLogsByUserIdDeletedAsync(int userId);
        Task<IEnumerable<EmailLog>> GetEmailLogsByRecipientsEmailDeletedAsync(string recipientEmail);
        Task<IEnumerable<EmailLog>> GetNotDeletedEmailLogsAsync(int userId);

    }
}
