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

//builder.Services.AddHostedService<SyncCompetitionJob>();
//builder.Services.AddHostedService<SyncMatchesJob>();
builder.Services.AddHostedService<MatchCalculatorJob>();
//builder.Services.AddHostedService<LiveMatchCalculatorJob>();

Batteries.Init();
SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
