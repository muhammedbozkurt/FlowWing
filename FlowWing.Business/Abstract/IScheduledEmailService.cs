using FlowWing.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowWing.Business.Abstract
{
    public interface IScheduledEmailService
    {
        Task<IEnumerable<ScheduledEmail>> GetRepeatingScheduledMailsAsync();
        Task<ScheduledEmail> GetScheduledEmailByIdAsync(int id);
        Task<ScheduledEmail> GetScheduledEmailByEmailLogId(int id);
        Task<IEnumerable<ScheduledEmail>> GetAllScheduledEmailsAsync();
        Task <ScheduledEmail> GetRepeatingScheduledMailByRepeatingLogId(int repeatingLogId);
        Task<ScheduledEmail> CreateScheduledEmailAsync(ScheduledEmail scheduledEmail);
        Task<ScheduledEmail> UpdateScheduledEmailAsync(ScheduledEmail scheduledEmail);
        Task<ScheduledEmail> DeleteScheduledEmailSenderAsync(int id);
        Task<ScheduledEmail> DeleteScheduledEmailRecieverAsync(int id);
        Task<ScheduledEmail> DeleteScheduledRepeatingEmailSenderAsync(int id);
    }
}
