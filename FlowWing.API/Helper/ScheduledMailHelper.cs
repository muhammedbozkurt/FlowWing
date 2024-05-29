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
            DateTime currentTime = DateTime.UtcNow;

            var parts = userInput.Split('-');
            if (parts.Length != 4)
            {
                throw new ArgumentException("Input must be in the format '00-00-00-00'");
            }
            //00-00-00-00
            int months = int.Parse(parts[0]);
            int days = int.Parse(parts[1]);
            int hours = int.Parse(parts[2]);
            int minutes = int.Parse(parts[3]);

            currentTime = currentTime.AddMonths(months);
            currentTime = currentTime.AddDays(days);
            currentTime = currentTime.AddHours(hours);
            currentTime = currentTime.AddMinutes(minutes);

            return $"{currentTime.Minute} {currentTime.Hour} {currentTime.Day} {currentTime.Month} *";
        }
    }
}
