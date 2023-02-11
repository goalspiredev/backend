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
    Task<Result> Notify(SendNotificationRequest request, bool test);
    Task<Result> Register(NotificationSubscriptionRegisterRequest request);
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

    public async Task<Result> Notify(SendNotificationRequest request, bool test)
    {
        // Get the subscription details from 
        List<NotificationSubscription>? subscriptions = null;
        if (test)
        {
            //for testing, send a notification to all the user's subscriptions
            subscriptions = _dataContext.NotificationSubscriptions.Where(x => x.UserId == request.UserId).ToList();
        }
        else
        {
            subscriptions = _dataContext.NotificationSubscriptions.Where(x => x.UserId == request.UserId && x.Id == request.Id).ToList();
        }

        if (subscriptions == null)
        {
            return Result.Failure("Notification subscription not found.");
        }

        foreach(var sub in subscriptions)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.p256dh, sub.Auth);

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

        // the foreach didn't fire even once
        return Result.Failure("Notification subscription not found.");
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
