using System.Xml;
using FlowWing.API.Models;
using FlowWing.Business.Abstract;
using FlowWing.Entities;
using Hangfire;
using Microsoft.Extensions.Options;
using FlowWing.API.Models;
namespace FlowWing.API.Helpers;

public class ScheduledMailHelper
{
    private readonly EmailSenderService _emailSenderService;
    private readonly IScheduledEmailService _scheduledEmailService;
    private IEmailLogService _emailLogService;

    public ScheduledMailHelper(EmailSenderService emailSenderService, IScheduledEmailService scheduledEmailService, IEmailLogService emailLogService)
    {
        _emailSenderService = emailSenderService;
        _scheduledEmailService = scheduledEmailService;
        _emailLogService = emailLogService;
    }
    
    public void ScheduleRepeatingEmail(EmailLog emailLog, ScheduledRepeatingEmailModel scheduledRepeatingEmailModel)
    {
        //_emailSenderService.UpdateStatus(emailLog);
        BackgroundJob.Schedule(() => SendRepeatingEmail(emailLog),scheduledRepeatingEmailModel.NextSendingDate);
    }
    public async Task ScheduleScheduledEmail(EmailLog createdEmailLog, ScheduledEmailLogModel scheduledEmail)
    {
        // Hangfire'da işi planla
        string jobId = BackgroundJob.Schedule(() => _emailSenderService.UpdateStatus(createdEmailLog),scheduledEmail.SentDateTime);
        
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

            // Kullanıcıdan alınan tekrar aralığını Hangfire için uygun zamanlama ile eşleştir
            string cronExpression = GetCronExpression(scheduledEmail.RepeatInterval);

            // Cron'a göre tekrarlama işini oluştur
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
            throw new ArgumentException("Geçersiz tarih formatı. Doğru format: Month-Day-Hour-Minute");
        }

        if (!int.TryParse(parts[0], out int month) ||
            !int.TryParse(parts[1], out int day) ||
            !int.TryParse(parts[2], out int hour) ||
            !int.TryParse(parts[3], out int minute))
        {
            throw new ArgumentException("Geçersiz tarih formatı.");
        }

        if (month > 0)
        {
            return $"0 0 1 * *"; 
        }
        else if (day > 0)
        {
            if (day >= 7)
            {
                return $"0 0 * * 0";
            }
            else
            {
                return $"0 0 * * *"; 
            }
        }
        else if (hour > 0)
        {
            return Cron.HourInterval(hour); // Her belirlenen saatte bir
        }
        else
        {
            // Sadece dakikalık tekrar için
            return Cron.MinuteInterval(minute); // Belirlenen dakika aralığında tekrar
        }
    }
    
}