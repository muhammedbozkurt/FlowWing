using System.Xml;
using FlowWing.API.Models;
using FlowWing.Business.Abstract;
using FlowWing.Entities;
using Hangfire;
using Microsoft.Extensions.Options;

namespace FlowWing.API.Helpers
{
    public class ScheduledMailHelper
    {
        private readonly EmailSenderService _emailSenderService;
        private readonly IScheduledEmailService _scheduledEmailService;
        private readonly IEmailLogService _emailLogService;

        public ScheduledMailHelper(EmailSenderService emailSenderService, IScheduledEmailService scheduledEmailService, IEmailLogService emailLogService)
        {
            _emailSenderService = emailSenderService;
            _scheduledEmailService = scheduledEmailService;
            _emailLogService = emailLogService;
        }

        public void ScheduleRepeatingEmail(EmailLog emailLog, ScheduledRepeatingEmailModel scheduledRepeatingEmailModel)
        {
            BackgroundJob.Schedule(() => SendRepeatingEmail(emailLog), scheduledRepeatingEmailModel.NextSendingDate);
        }

        public async Task ScheduleScheduledEmail(EmailLog createdEmailLog, ScheduledEmailLogModel scheduledEmail)
        {
            string jobId = BackgroundJob.Schedule(() => _emailSenderService.UpdateStatus(createdEmailLog), scheduledEmail.SentDateTime);

            createdEmailLog.HangfireJobId = jobId;
            _emailLogService.UpdateEmailLog(createdEmailLog);
        }

        public async Task SendRepeatingEmail(EmailLog emailLog)
        {
            ScheduledEmail scheduledEmail = await _scheduledEmailService.GetScheduledEmailByEmailLogId(emailLog.Id);
            await _emailSenderService.CreateRepeatingEmailLog(emailLog, scheduledEmail.Id);

            if (DateTime.UtcNow < scheduledEmail.RepeatEndDate)
            {
                await _emailSenderService.UpdateStatus(emailLog);
                scheduledEmail.LastSendingDate = DateTime.UtcNow;
                await _scheduledEmailService.UpdateScheduledEmailAsync(scheduledEmail);

                string cronExpression = GetCronExpression(scheduledEmail.RepeatInterval);
                RecurringJob.AddOrUpdate($"repeatingemailjob-{emailLog.Id}", () => SendRepeatingEmail(emailLog), cronExpression);
            }
            else
            {
                RecurringJob.RemoveIfExists($"repeatingemailjob-{emailLog.Id}");
            }
        }

        private string GetCronExpression(string userInput)
        {
            string[] parts = userInput.Split('-');

            if (parts.Length != 4)
            {
                throw new ArgumentException("Invalid date format. Correct format: Month-Day-Hour-Minute");
            }

            if (!int.TryParse(parts[0], out int month) ||
                !int.TryParse(parts[1], out int day) ||
                !int.TryParse(parts[2], out int hour) ||
                !int.TryParse(parts[3], out int minute))
            {
                throw new ArgumentException("Invalid date format.");
            }

            if (minute > 0)
            {
                return Cron.MinuteInterval(minute); // Every X minutes
            }
            else if (hour > 0)
            {
                return Cron.HourInterval(hour); // Every X hours
            }
            else if (day > 0)
            {
                return day >= 7 ? "0 0 * * 0" : "0 0 * * *"; // Weekly or daily
            }
            else if (month > 0)
            {
                return "0 0 1 * *"; // Monthly
            }
            else
            {
                throw new ArgumentException("Invalid interval values.");
            }
        }
    }
}
