using FluentEmail.Core;
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
    private readonly IFluentEmail _email;

    public EmailService(ILogger<EmailService> logger, IFluentEmail email)
    {
        _logger = logger;
        _email = email;
    }

    public async Task<Result> SendEmail(SendEmailRequest request)
    {
        _logger.LogInformation($"Sending an email to {request.Email} with title: {request.Title}");

        var email = await _email
            .To(request.Email)
            .Subject(request.Title)
            .Body(request.Content, request.IsHtml)
            .SendAsync();

        if (!email.Successful)
        {
            throw new Exception(string.Join("; ", email.ErrorMessages));
            return new Result
            {
                Succeeded = false,
                Error = string.Join(", ", email.ErrorMessages)
            };
        }

        return Result.Success;
    }
}