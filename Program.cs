using Azure.Storage.Blobs;
using AzurePOC;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:4200");
        });
});

builder.Services.AddDbContext<AzurePocContext>();

builder.Services.AddSingleton(serviceProvider =>
{
    var uri = builder.Configuration.GetValue<string>("Blob:Container:SasUrl") ?? throw new InvalidOperationException("Blob:Container:SasUrl is required");
    return new BlobContainerClient(new Uri(uri));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/pokemon", (AzurePocContext context) =>
{
    return context.Pokemon.ToListAsync();
})
.WithName("GetPokemon");

app.MapGet("/pokemon/images", async (BlobContainerClient blobContainerClient) =>
{
    var names = new List<string>();
    await foreach (var blob in blobContainerClient.GetBlobsAsync())
    {
        names.Add(blob.Name);
    }
    return names;
})
.WithName("AvailablePokemonImages");

app.MapGet("/pokemon/images/{*name}", async (string name, BlobContainerClient blobContainerClient, HttpContext context) =>
{
    var client = blobContainerClient.GetBlobClient(name);
    if (!await client.ExistsAsync())
    {
        context.Response.StatusCode = 404;
        return;
    }

    try
    {
        var downloadResult = await client.DownloadAsync();
        var properties = await client.GetPropertiesAsync();
        context.Response.ContentType = properties.Value.ContentType;
        await downloadResult.Value.Content.CopyToAsync(context.Response.Body);
    }
    catch (Exception ex)
    {
        // Handle exceptions (e.g., blob not found) 
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"Something went wrong: {ex}");
    }
})
.WithName("GetPokemonImage");

app.UseCors();
app.Run();