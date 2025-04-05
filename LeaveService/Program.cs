using MassTransit;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://auth-service"; // Replace with your actual Auth Service URL
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Set to true and define ValidAudience if needed
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddMassTransit(x =>
//{
//    x.UsingRabbitMq((ctx, cfg) =>
//    {
//        cfg.Host("rabbitmq://localhost"); // Replace with your RabbitMQ config
//    });
//});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ❗ Correct order is important:
app.UseAuthentication(); // 👈 Required before authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
