using Dapper;
using important_game.infrastructure;
using important_game.web.Handlers;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

#if DEBUG
builder.Services.AddSassCompiler();
#endif

builder.Services.MatchImportanceInfrastructure(builder.Configuration);

builder.Services.AddHostedService<SyncCompetitionJob>();
builder.Services.AddHostedService<SyncFinishedMatchesJob>();
builder.Services.AddHostedService<SyncUpcomingMatchesJob>();
builder.Services.AddHostedService<MatchCalculatorJob>();

Batteries.Init();
SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
SqlMapper.AddTypeHandler(new NullableDateTimeOffsetHandler());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
