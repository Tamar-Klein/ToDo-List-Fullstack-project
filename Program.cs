using Microsoft.EntityFrameworkCore;
using TodoApi;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("https://todo-list-fullstack-client-1kqz.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// DB Context
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        new MySqlServerVersion(new Version(8, 0, 33))
    )
);

var app = builder.Build();

// חובה ב-.NET למיפוי תקין
app.UseRouting();

// הפעלת CORS *לפני* המיפוי
app.UseCors("AllowClient");

// Swagger תמיד מופעל
app.UseSwagger();
app.UseSwaggerUI();

// Endpoints
app.MapGet("/", () => "Hello World!");

app.MapGet("/tasks", async (ToDoDbContext context) =>
{
    return await context.Items.ToListAsync();
});

app.MapPost("/tasks", async (ToDoDbContext context, Item newItem) =>
{
    context.Items.Add(newItem);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{newItem.Id}", newItem);
});

app.MapPut("/tasks/{id}", async (ToDoDbContext context, int id, Item updatedItem) =>
{
 var item = await context.Items.FindAsync(id);
if (item == null) return Results.NotFound();
 item.IsComplete = updatedItem.IsComplete;
await context.SaveChangesAsync();
return Results.Ok(item);
});
app.MapDelete("/tasks/{id}", async (ToDoDbContext context, int id) =>
{
    var item = await context.Items.FindAsync(id);
    if (item == null) return Results.NotFound();

    context.Items.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
