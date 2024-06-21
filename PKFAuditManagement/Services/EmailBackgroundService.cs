using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PKFAuditManagement.Data;
using PKFAuditManagement.Interface;
using PKFAuditManagement.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EmailBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                var dueEmails = context.SignedFSForm
                    .Where(e => e.ScheduleDate <= DateTime.Now && !e.IsProcessed)
                    .ToList();

                foreach (var email in dueEmails)
                {
                    await emailSender.SendEmailAsync(email.PartnerEmail, email.EmailType, email.EmailBody);
                    email.IsProcessed = true;
                    context.Update(email);
                }

                await context.SaveChangesAsync();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(30), stoppingToken); // Check every minute
        }
    }
}
