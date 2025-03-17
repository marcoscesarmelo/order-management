using OrderGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddSingleton<OrderService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

app.MapControllers();

app.Run();