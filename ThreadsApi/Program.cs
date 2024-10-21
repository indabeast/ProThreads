using ThreadsApi.Data;
using Microsoft.EntityFrameworkCore;
using ThreadsApi.Service;
using Thread = shared.Thread;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;



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

builder.Services.AddDbContext<ThreadsContext>(options =>
    options.UseSqlite("Data Source=Thread.db"));  // Example for SQLite
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DataService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This should prevent the cycle by using reference handling
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
    dataService.SeedData(); // Seed the data if the database is empty.
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/threads", async (DataService dataService) => 
    await dataService.GetThreadsAsync());
app.MapGet("/threads/{id}", async (int id, DataService dataService) =>
{
    var thread = await dataService.GetThreadByIdAsync(id);
    return thread is not null ? Results.Ok(thread) : Results.NotFound();
});

app.MapPost("/threads", async (Thread thread, DataService dataService) =>
{
    await dataService.CreateThreadAsync(thread);
    return Results.Created($"/threads/{thread.Id}", thread);
});
app.MapPost("/threads/{threadId}/comments", async (int threadId, shared.Comment comment, DataService dataService) =>
{
    await dataService.AddCommentAsync(threadId, comment);
    return Results.Created($"/threads/{threadId}/comments/{comment.Id}", comment);
});
app.UseCors("AllowAllOrigins"); 
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();