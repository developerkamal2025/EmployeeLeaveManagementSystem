using BuildingBlock.Shared;
using BuildingBlock.Shared.Models;
using MassTransit;

namespace NotificationService.Consumers
{
    public class LeaveEventConsumer : IConsumer<LeaveEvent>
    {
        public Task Consume(ConsumeContext<LeaveEvent> context)
        {
            var message = context.Message;
            string body = "Leave " + message.Status;
            CommonService.SendMail(message.ToUser, "Leave", body);

            return Task.CompletedTask;
        }
    }
}
