using BuildingBlock.Shared.Models;
using MassTransit;

namespace NotificationService.Consumers
{
    public class LeaveEventConsumer : IConsumer<LeaveEvent>
    {
        public Task Consume(ConsumeContext<LeaveEvent> context)
        {
            var message = context.Message;
            // send email/SMS
            Console.WriteLine($"Sending notification to {message.UserEmail}");
            return Task.CompletedTask;
        }
    }
}
