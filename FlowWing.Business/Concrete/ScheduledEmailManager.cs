using FlowWing.Business.Abstract;
using FlowWing.DataAccess.Abstract;
using FlowWing.Entities;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowWing.Business.Concrete
{
    public class ScheduledEmailManager : IScheduledEmailService
    {
        private readonly IScheduledEmailRepository _scheduledEmailRepository;
        private readonly IEmailLogRepository _emailLogRepository;
        public ScheduledEmailManager(IScheduledEmailRepository scheduledEmailRepository, IEmailLogRepository emailLogRepository) 
        { 
            _scheduledEmailRepository = scheduledEmailRepository;
            _emailLogRepository = emailLogRepository;
        }

        public async Task<ScheduledEmail> CreateScheduledEmailAsync(ScheduledEmail scheduledEmail)
        {
            return await _scheduledEmailRepository.CreateScheduledEmailAsync(scheduledEmail);
        }
        public async Task<ScheduledEmail> DeleteScheduledEmailSenderAsync(int id)
        {
            ScheduledEmail scheduledEmail = await _scheduledEmailRepository.GetScheduledEmailByIdAsync(id);
            EmailLog emailLog = await _emailLogRepository.GetEmailLogByIdAsync(scheduledEmail.EmailLogId);
            if (scheduledEmail != null)
            {
                emailLog.SenderDeletionDate = DateTime.Now.AddDays(30).ToUniversalTime();
                scheduledEmail.SenderDeletionDate = DateTime.Now.AddDays(30).ToUniversalTime();
                await _scheduledEmailRepository.UpdateScheduledEmailAsync(scheduledEmail);
                await _emailLogRepository.UpdateEmailLogAsync(emailLog);

                BackgroundJob.Delete(emailLog.HangfireJobId);
                BackgroundJob.Schedule(() => _emailLogRepository.DeleteEmailLogAsync(emailLog), TimeSpan.FromDays(30));
                BackgroundJob.Schedule(() => _scheduledEmailRepository.DeleteScheduledEmailAsync(scheduledEmail), TimeSpan.FromDays(30));
                return scheduledEmail;
            }
            else
            {
                return null;
            }
        }
        public async Task<ScheduledEmail> DeleteScheduledEmailRecieverAsync(int id)
        {
            ScheduledEmail scheduledEmail = await _scheduledEmailRepository.GetScheduledEmailByIdAsync(id);
            EmailLog emailLog = await _emailLogRepository.GetEmailLogByIdAsync(scheduledEmail.EmailLogId);
            if (scheduledEmail != null)
            {
                emailLog.RecieverDeletionDate = DateTime.Now.AddDays(30).ToUniversalTime();
                scheduledEmail.RecieverDeletionDate = DateTime.Now.AddDays(30).ToUniversalTime();
                await _scheduledEmailRepository.UpdateScheduledEmailAsync(scheduledEmail);
                await _emailLogRepository.UpdateEmailLogAsync(emailLog);

                BackgroundJob.Delete(emailLog.HangfireJobId);
                BackgroundJob.Schedule(() => _emailLogRepository.DeleteEmailLogAsync(emailLog), TimeSpan.FromDays(30));
                BackgroundJob.Schedule(() => _scheduledEmailRepository.DeleteScheduledEmailAsync(scheduledEmail), TimeSpan.FromDays(30));
                return scheduledEmail;
            }
            else
            {
                return null;
            }
        }

        public async Task<ScheduledEmail> DeleteScheduledRepeatingEmailSenderAsync(int id)
        {
            // Get the ScheduledEmail object based on the provided ID
            var scheduledEmail = await _scheduledEmailRepository.GetScheduledEmailByIdAsync(id);
            if (scheduledEmail == null)
            {
                return null; // If not found, return null
            }

            // Fetch all EmailLog objects with the same RepeatingLogId
            var emailLogs = await _emailLogRepository.GetEmailLogsByRepeatingLogIdAsync(scheduledEmail.Id);

            // UTC time for 30 days later
            var deletionDate = DateTime.Now.AddDays(30).ToUniversalTime();

            // Update DeletionDate for each EmailLog and schedule its deletion
            foreach (var emailLog in emailLogs)
            {
                emailLog.SenderDeletionDate = deletionDate; // Set deletion date to 30 days later
                await _emailLogRepository.UpdateEmailLogAsync(emailLog); // Update the EmailLog in the repository

                // Schedule the deletion of the EmailLog after 30 days
                BackgroundJob.Schedule(() => _emailLogRepository.DeleteEmailLogAsync(emailLog), TimeSpan.FromDays(30));

                // Remove any existing recurring job associated with this EmailLog
                RecurringJob.RemoveIfExists(emailLog.HangfireJobId);
            }

            // Update DeletionDate for the ScheduledEmail
            scheduledEmail.SenderDeletionDate = deletionDate;
            await _scheduledEmailRepository.UpdateScheduledEmailAsync(scheduledEmail);

            // Schedule the deletion of the ScheduledEmail after 30 days
            BackgroundJob.Schedule(() => _scheduledEmailRepository.DeleteScheduledEmailAsync(scheduledEmail), TimeSpan.FromDays(30));

            // Remove the recurring job associated with this ScheduledEmail
            RecurringJob.RemoveIfExists("repeatingemailjob-" + id.ToString());

            // Return the updated ScheduledEmail
            return scheduledEmail;
        }


        public async Task<IEnumerable<ScheduledEmail>> GetRepeatingScheduledMailsAsync()
        {
            return await _scheduledEmailRepository.GetRepeatingScheduledMailsAsync();
        }

        public async Task<ScheduledEmail> GetScheduledEmailByEmailLogId(int id)
        {
            return await _scheduledEmailRepository.GetScheduledEmailByEmailLogId(id);
        }

        public async Task<IEnumerable<ScheduledEmail>> GetAllScheduledEmailsAsync()
        {
            return await _scheduledEmailRepository.GetAllScheduledEmailsAsync();
        }

        public async Task<ScheduledEmail> GetScheduledEmailByIdAsync(int id)
        {
            return await _scheduledEmailRepository.GetScheduledEmailByIdAsync(id);
        }

        public async Task<ScheduledEmail> UpdateScheduledEmailAsync(ScheduledEmail scheduledEmail)
        {
            return await _scheduledEmailRepository.UpdateScheduledEmailAsync(scheduledEmail);
        }
    }
}
