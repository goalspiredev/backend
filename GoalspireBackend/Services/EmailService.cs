using GoalspireBackend.Common;
using GoalspireBackend.Dto.Requests.Email;

namespace GoalspireBackend.Services;

public interface IEmailService
{
    Task<Result> SendEmail(SendEmailRequest request);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task<Result> SendEmail(SendEmailRequest request)
    {
        _logger.LogInformation($"Sending an email to {request.Email} with title: {request.Title}");
        _logger.LogInformation("Email body: {0}", request.Content);

        return Task.FromResult(Result.Success);
    }
}