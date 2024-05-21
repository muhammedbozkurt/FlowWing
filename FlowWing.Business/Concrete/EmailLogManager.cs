using FlowWing.Business.Abstract;
using FlowWing.DataAccess.Abstract;
using FlowWing.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowWing.Business.Concrete
{
    public class EmailLogManager : IEmailLogService
    {
        private IEmailLogRepository _emailLogRepository;
        private IScheduledEmailRepository _scheduledEmailRepository;

        public EmailLogManager(IEmailLogRepository emailLogRepository, IScheduledEmailRepository scheduledEmailRepository)
        {
            _emailLogRepository = emailLogRepository;
            _scheduledEmailRepository = scheduledEmailRepository;
        }

        public async Task<EmailLog> CreateEmailLogAsync(EmailLog emailLog)
        {
            if (await _emailLogRepository.GetEmailLogByIdAsync(emailLog.Id) != null)
            {
                throw new Exception("EmailLog already exists");
            }
            else
            {
                await _emailLogRepository.CreateEmailLogAsync(emailLog);
                return emailLog;
            }
        }
        public EmailLog CreateEmailLog(EmailLog emailLog)
        {
            _emailLogRepository.CreateEmailLog(emailLog);
            return emailLog;
        }

        public async Task<EmailLog> DeleteEmailSenderLogAsync(int id)
        {
            if (await _emailLogRepository.GetEmailLogByIdAsync(id) == null)
            {
                throw new Exception("EmailLog does not exist");
            }
            else
            {
                var emailLog = await _emailLogRepository.GetEmailLogByIdAsync(id);

                if (emailLog.repeatingLogId != null)
                {
                    var repeatingScheduledEmail = _scheduledEmailRepository.GetRepeatingScheduledMailByRepeatingLogId((int)emailLog.repeatingLogId).Result;
                    RecurringJob.RemoveIfExists("repeatingemailjob-" + repeatingScheduledEmail.EmailLogId);
                    repeatingScheduledEmail.SenderDeletionDate = DateTime.Now.AddDays(30).ToUniversalTime();
                    await _scheduledEmailRepository.UpdateScheduledEmailAsync(repeatingScheduledEmail);
                    BackgroundJob.Schedule(() => _scheduledEmailRepository.DeleteScheduledEmailAsync(repeatingScheduledEmail), TimeSpan.FromDays(30));
                }
                else
                {
                    var scheduledEmail = _scheduledEmailRepository.GetScheduledEmailByEmailLogId(emailLog.Id).Result;
                    if (scheduledEmail != null)
                    {
                        scheduledEmail.SenderDeletionDate = DateTime.Now.AddDays(30).ToUniversalTime();
                        await _scheduledEmailRepository.UpdateScheduledEmailAsync(scheduledEmail);
                        BackgroundJob.Schedule(() => _scheduledEmailRepository.DeleteScheduledEmailAsync(scheduledEmail), TimeSpan.FromDays(30));
                        BackgroundJob.Delete(emailLog.HangfireJobId);
                    }
                }

                emailLog.SenderDeletionDate = DateTime.Now.AddDays(30).ToUniversalTime();
                await _emailLogRepository.UpdateEmailLogAsync(emailLog);

                BackgroundJob.Schedule(() => _emailLogRepository.DeleteEmailLogAsync(emailLog), TimeSpan.FromDays(30));
                
                return emailLog;
            }
        }
        public async Task<EmailLog> DeleteEmailRecieverLogAsync(int id)
        {
            if (await _emailLogRepository.GetEmailLogByIdAsync(id) == null)
            {
                throw new Exception("EmailLog does not exist");
            }
            else
            {
                var emailLog = await _emailLogRepository.GetEmailLogByIdAsync(id);
                emailLog.RecieverDeletionDate = DateTime.Now.AddDays(30).ToUniversalTime();
                await _emailLogRepository.UpdateEmailLogAsync(emailLog);

                BackgroundJob.Schedule(() => _emailLogRepository.DeleteEmailLogAsync(emailLog), TimeSpan.FromDays(30));

                return emailLog;
            }
        }
        public async Task<IEnumerable<EmailLog>> GetEmailLogsByUserIdDeletedAsync(int userId)
        {
            return await _emailLogRepository.GetEmailLogsByUserIdDeletedAsync(userId);
        }
        public async Task<IEnumerable<EmailLog>> GetEmailLogsByRecipientsEmailDeletedAsync(string recipientEmail)
        {
            return await _emailLogRepository.GetEmailLogsByRecipientsEmailDeletedAsync(recipientEmail);
        }
        public async Task<IEnumerable<EmailLog>> GetAllEmailLogsAsync()
        {
            return await _emailLogRepository.GetAllEmailLogsAsync();
        }
        public async Task<IEnumerable<EmailLog>> GetEmailLogsByRepeatingLogIdAsync(int repeatingLogId)
        {
            return await _emailLogRepository.GetEmailLogsByRepeatingLogIdAsync(repeatingLogId);
        }
        public async Task<EmailLog> GetEmailLogByIdAsync(int? id)
        { 
          return await _emailLogRepository.GetEmailLogByIdAsync(id);
        }

        public async Task<EmailLog> GetEmailLogByScheduledEmailIdAsync(int scheduledEmailId)
        { 
            return await _emailLogRepository.GetEmailLogByScheduledEmailIdAsync(scheduledEmailId);
        }
        public async Task<IEnumerable<EmailLog>> GetEmailLogsByUserIdAsync(int userId)
        {
            return await _emailLogRepository.GetEmailLogsByUserIdAsync(userId);
        }
        
        public async Task<IEnumerable<EmailLog>> GetEmailLogsByRecipientsEmailAsync(string recipientEmail)
        {   
            return await _emailLogRepository.GetEmailLogsByRecipientsEmailAsync(recipientEmail);
        }

        public async Task<IEnumerable<EmailLog>> GetNotDeletedEmailLogsByRecipientsEmailAsync(string recipientEmail)
        {
            return await _emailLogRepository.GetNotDeletedEmailLogsByRecipientsEmailAsync(recipientEmail);
        }

        public async Task<IEnumerable<EmailLog>> GetNotDeletedEmailLogsAsync(int userId)
        {
            return await _emailLogRepository.GetNotDeletedEmailLogsAsync(userId);
        }
        public async Task<EmailLog> UpdateEmailLogAsync(EmailLog emailLog)
        {
            
            if (await _emailLogRepository.GetEmailLogByIdAsync(emailLog.Id) == null)
            {
                throw new Exception("EmailLog does not exist");
            }
            else
            {
                await _emailLogRepository.UpdateEmailLogAsync(emailLog);
                return emailLog;
            }
        }

        public EmailLog UpdateEmailLog(EmailLog emailLog)
        {
            if (_emailLogRepository.GetEmailLogById(emailLog.Id) == null)
            {
                throw new Exception("EmailLog does not exist");
            }
            else
            {
                _emailLogRepository.UpdateEmailLog(emailLog);
                return emailLog;
            }
        }
    }
}
