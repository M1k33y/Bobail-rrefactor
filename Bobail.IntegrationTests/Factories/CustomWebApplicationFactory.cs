using Bobail.Application.Interfaces.Services;
using Bobail.Infrastructure.Email;
using Bobail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bobail.IntegrationTests.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<GameDbContext>));

            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            services.AddDbContext<GameDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            var emailSenderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailSender));
            if (emailSenderDescriptor != null)
                services.Remove(emailSenderDescriptor);

            var emailOutboxDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailOutbox));
            if (emailOutboxDescriptor != null)
                services.Remove(emailOutboxDescriptor);

            var inMemoryEmailSenderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(InMemoryEmailSender));
            if (inMemoryEmailSenderDescriptor != null)
                services.Remove(inMemoryEmailSenderDescriptor);

            services.AddSingleton<InMemoryEmailSender>();
            services.AddSingleton<IEmailOutbox>(sp => sp.GetRequiredService<InMemoryEmailSender>());
            services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<InMemoryEmailSender>());
        });
    }
}
