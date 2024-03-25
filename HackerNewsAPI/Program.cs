using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Added a rate limit policy
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    //Fixed rate limiter against a specific ip address
    options.AddPolicy("fixed", HttpContext =>
    RateLimitPartition.GetFixedWindowLimiter
    (partitionKey: HttpContext.Connection.RemoteIpAddress.ToString(),
    factory: partition => new FixedWindowRateLimiterOptions
    {
        PermitLimit = 10,
        Window = TimeSpan.FromSeconds(10)
    }));

    //Fixed rate limiter for a specific window
    //options.AddFixedWindowLimiter("fixed", options =>
    //{
    //    options.PermitLimit = 10;
    //    options.Window = TimeSpan.FromSeconds(10);
    //});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();