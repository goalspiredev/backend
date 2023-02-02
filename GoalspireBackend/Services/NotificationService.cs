using GoalspireBackend.Common;
using GoalspireBackend.Data;
using GoalspireBackend.Dto.Requests.Notifications;
using GoalspireBackend.Models;
using Microsoft.EntityFrameworkCore;
using WebPush;

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
        //get the subscription details from db
        NotificationSubscription? notifSub = await _dataContext.NotificationSubscriptions.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);
        if (notifSub == null)
        {
            return Result.Failure("Notification subcription not found.");
        }


        PushSubscription subscription = new PushSubscription(notifSub.Endpoint, notifSub.p256dh, notifSub.Auth);

        var subject = _configuration["VAPID:Subject"];
        var publicKey = _configuration["VAPID:PublicKey"];
        var privateKey = _configuration["VAPID:PrivateKey"];
        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        var webPushClient = new WebPushClient();

        try
        {
            webPushClient.SendNotification(subscription, request.Message, vapidDetails);
        }
        catch (Exception ex)
        {
            // tbf not sure what could go wrong, but the sample code has it this way too
            Console.WriteLine(ex);
        }


        throw new NotImplementedException();
    }

    public async Task<Result> Register(NotificationSubscriptionRegisterRequest request)
    {
        var subscription = new PushSubscription(request.Endpoint, request.p256dh, request.Auth);

        NotificationSubscription? notifSub = await _dataContext.NotificationSubscriptions.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);
        if (notifSub != null)
        {
            //remove the old subscription for this user && device
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

        return Result.Success();
    }
}
