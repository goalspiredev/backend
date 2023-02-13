using System.Text.Json;
using GoalspireBackend.Common;
using GoalspireBackend.Data;
using GoalspireBackend.Dto.Requests.Notifications;
using GoalspireBackend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebPush;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GoalspireBackend.Services;

public interface INotificationService
{
    Task<Result> Register(NotificationSubscriptionRegisterRequest request);
    Task<Result> Notify(SendNotificationRequest request);
}


public class NotificationService : INotificationService
{
    private readonly DataContext _dataContext;
    private readonly IConfiguration _configuration;

    public NotificationService(DataContext dataContext, IConfiguration configuration)
    {
        _dataContext = dataContext;
        _configuration = configuration;
    }

    public async Task<Result> Notify(SendNotificationRequest request)
    {
        // Get the subscription details from DB
        var notifSub = await _dataContext.NotificationSubscriptions.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);
        if (notifSub == null)
        {
            return Result.Failure("Notification subscription not found.");
        }

        var subscription = new PushSubscription(notifSub.Endpoint, notifSub.p256dh, notifSub.Auth);

        var subject = _configuration["VAPID:Subject"];
        var publicKey = _configuration["VAPID:PublicKey"];
        var privateKey = _configuration["VAPID:PrivateKey"];
        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        var webPushClient = new WebPushClient();
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            await webPushClient.SendNotificationAsync(subscription, JsonSerializer.Serialize(request, options), vapidDetails);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> Register(NotificationSubscriptionRegisterRequest request)
    {
        var notifSub = await _dataContext.NotificationSubscriptions.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);
        if (notifSub != null)
        {
            // Remove the old subscription for this user && device
            _dataContext.NotificationSubscriptions.Remove(notifSub);
        }

        //add sub to the db
        await _dataContext.NotificationSubscriptions.AddAsync(new NotificationSubscription
        {
            Id = request.Id,
            UserId = request.UserId,
            Auth = request.Auth,
            Endpoint = request.Endpoint,
            p256dh = request.p256dh
        });

        await _dataContext.SaveChangesAsync();

        return Result.Success();
    }
}
