using BlazorUrl.Data;
using BlazorUrl.Public;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddTransient<ILinkService, LinkService>();

var app = builder.Build();


app.UseHttpsRedirection();

app.MapGet("{shortCode}", async (string shortCode, ILinkService linkService) =>
{
    var longUrl = await linkService.GetLongUrlByShortCodeAsync(shortCode);
    if(string.IsNullOrWhiteSpace(longUrl))
    {
        return Results.NotFound();
    }

    return Results.Redirect(longUrl);
});

app.Run();


