using MassTransit;
using NotificationService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<LeaveEventConsumer>(); // Register the consumer

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("leave-notification-queue", e =>
        {
            e.ConfigureConsumer<LeaveEventConsumer>(context);
        });
    });
});
var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
