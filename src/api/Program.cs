using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Azure Table Storage — uses Managed Identity in production (AZURE_STORAGE_ACCOUNT_NAME set),
// falls back to Azurite / connection string for local development.
TableClient MakeTableClient(string tableName, IConfiguration config)
{
    var accountName = config["AZURE_STORAGE_ACCOUNT_NAME"];
    if (!string.IsNullOrEmpty(accountName))
    {
        var endpoint = new Uri($"https://{accountName}.table.core.windows.net");
        return new TableClient(endpoint, tableName, new DefaultAzureCredential());
    }
    var connectionString = config.GetConnectionString("AzureStorage") ?? "UseDevelopmentStorage=true";
    return new TableClient(connectionString, tableName);
}

builder.Services.AddSingleton<TableClient>(sp =>
    MakeTableClient("Ratings", builder.Configuration));

builder.Services.AddKeyedSingleton<TableClient>("users", (sp, _) =>
    MakeTableClient("Users", builder.Configuration));

builder.Services.AddKeyedSingleton<TableClient>("beers", (sp, _) =>
    MakeTableClient("Beers", builder.Configuration));

var app = builder.Build();

var tableClient = app.Services.GetRequiredService<TableClient>();
await tableClient.CreateIfNotExistsAsync();

var usersTableClient = app.Services.GetRequiredKeyedService<TableClient>("users");
await usersTableClient.CreateIfNotExistsAsync();

var beersTableClient = app.Services.GetRequiredKeyedService<TableClient>("beers");
await beersTableClient.CreateIfNotExistsAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// Admin key check — if ADMIN_KEY is set in config, all /api/admin/* requests must supply it
// via the X-Admin-Key header. In local development (no key configured) the check is skipped.
var adminKey = app.Configuration["ADMIN_KEY"];
bool IsAuthorized(HttpContext ctx) =>
    string.IsNullOrEmpty(adminKey) || ctx.Request.Headers["X-Admin-Key"] == adminKey;

app.MapPost("/api/register", async ([FromKeyedServices("users")] TableClient users) =>
{
    var userId = Guid.NewGuid().ToString();
    var registeredAt = DateTimeOffset.UtcNow;
    await users.UpsertEntityAsync(new UserEntity
    {
        PartitionKey   = "user",
        RowKey         = userId,
        RegisteredAt   = registeredAt,
    });
    return Results.Ok(new { userId });
})
.WithName("Register");

app.MapGet("/api/summary", async (TableClient ratings, [FromKeyedServices("users")] TableClient users, [FromKeyedServices("beers")] TableClient beers) =>
{
    var userEntities = new List<UserEntity>();
    await foreach (var u in users.QueryAsync<UserEntity>())
        userEntities.Add(u);

    var ratedBeerIds = new HashSet<string>();
    var totalRatings = 0;
    await foreach (var r in ratings.QueryAsync<RatingEntity>(select: ["RowKey"]))
    {
        ratedBeerIds.Add(r.RowKey);
        totalRatings++;
    }

    var totalBeers = 0;
    await foreach (var _ in beers.QueryAsync<BeerEntity>(select: ["RowKey"]))
        totalBeers++;

    return Results.Ok(new
    {
        TotalUsers        = userEntities.Count,
        FirstRegistered   = userEntities.Count > 0 ? userEntities.Min(u => u.RegisteredAt) : (DateTimeOffset?)null,
        LastRegistered    = userEntities.Count > 0 ? userEntities.Max(u => u.RegisteredAt) : (DateTimeOffset?)null,
        TotalRatings      = totalRatings,
        BeersRated        = ratedBeerIds.Count,
        TotalBeers        = totalBeers,
    });
})
.WithName("GetSummary");

app.MapGet("/api/beers", async ([FromKeyedServices("beers")] TableClient beers) =>
{
    var all = new List<Beer>();
    await foreach (var e in beers.QueryAsync<BeerEntity>())
    {
        all.Add(new Beer(int.Parse(e.RowKey), e.BrewersName, e.BeerName, e.Style, e.Abv, e.Description, e.ServingMethod));
    }
    all.Sort((a, b) => a.Id.CompareTo(b.Id));
    return Results.Ok(all);
})
.WithName("GetBeers");

app.MapGet("/api/ratings", async (string userId, TableClient table) =>
{
    var userRatings = new Dictionary<int, object>();
    await foreach (var entity in table.QueryAsync<RatingEntity>(e => e.PartitionKey == userId))
    {
        if (int.TryParse(entity.RowKey, out var beerId))
            userRatings[beerId] = new { entity.Rating, entity.Notes };
    }
    return Results.Ok(userRatings);
})
.WithName("GetRatings");

app.MapPost("/api/ratings", async (RatingRequest request, TableClient table) =>
{
    if (request.Rating < 1 || request.Rating > 10)
        return Results.BadRequest(new { error = "Rating must be between 1 and 10." });

    var entity = new RatingEntity
    {
        PartitionKey = request.UserId,
        RowKey       = request.BeerId.ToString(),
        Rating       = request.Rating,
        Notes        = request.Notes,
    };
    await table.UpsertEntityAsync(entity);
    return Results.Ok(new { request.UserId, request.BeerId, request.Rating, request.Notes });
})
.WithName("SubmitRating");

app.MapDelete("/api/ratings", async (string userId, int beerId, TableClient table) =>
{
    await table.DeleteEntityAsync(userId, beerId.ToString());
    return Results.NoContent();
})
.WithName("ClearRating");

app.MapGet("/api/results", async (TableClient table) =>
{
    var grouped = new Dictionary<int, List<int>>();
    await foreach (var entity in table.QueryAsync<RatingEntity>())
    {
        if (int.TryParse(entity.RowKey, out var beerId))
        {
            if (!grouped.ContainsKey(beerId))
                grouped[beerId] = [];
            grouped[beerId].Add(entity.Rating);
        }
    }

    var results = grouped
        .Select(kvp => new
        {
            BeerId  = kvp.Key,
            Average = Math.Round(kvp.Value.Average(), 1),
            Count   = kvp.Value.Count,
        })
        .OrderByDescending(r => r.Average)
        .ThenByDescending(r => r.Count);

    return Results.Ok(results);
})
.WithName("GetResults");

app.MapGet("/api/results/{beerId}", async (int beerId, TableClient table) =>
{
    var ratings = new List<(int Rating, string? Notes)>();
    await foreach (var entity in table.QueryAsync<RatingEntity>(e => e.RowKey == beerId.ToString()))
    {
        ratings.Add((entity.Rating, entity.Notes));
    }

    var sorted = ratings
        .OrderByDescending(r => r.Rating)
        .Select(r => new { r.Rating, r.Notes });

    return Results.Ok(sorted);
})
.WithName("GetBeerRatings");

// ---------------------------------------------------------------------------
// Admin endpoints — require X-Admin-Key header when ADMIN_KEY is configured
// ---------------------------------------------------------------------------

app.MapPost("/api/admin/beers", async (HttpContext ctx, Beer[] beersToImport, [FromKeyedServices("beers")] TableClient beers) =>
{
    if (!IsAuthorized(ctx)) return Results.Unauthorized();

    foreach (var beer in beersToImport)
    {
        await beers.UpsertEntityAsync(new BeerEntity
        {
            PartitionKey  = "beer",
            RowKey        = beer.Id.ToString(),
            BrewersName   = beer.BrewersName,
            BeerName      = beer.BeerName,
            Style         = beer.Style,
            Abv           = beer.Abv,
            Description   = beer.Description,
            ServingMethod = beer.ServingMethod,
        });
    }

    return Results.Ok(new { imported = beersToImport.Length });
})
.WithName("ImportBeers");

app.MapDelete("/api/admin/beers", async (HttpContext ctx, [FromKeyedServices("beers")] TableClient beers) =>
{
    if (!IsAuthorized(ctx)) return Results.Unauthorized();

    var entities = new List<BeerEntity>();
    await foreach (var e in beers.QueryAsync<BeerEntity>())
        entities.Add(e);

    // Table Storage batch operations require all entities in the same partition (max 100 per batch)
    foreach (var batch in entities.Chunk(100))
    {
        var actions = batch.Select(e => new TableTransactionAction(TableTransactionActionType.Delete, e));
        await beers.SubmitTransactionAsync(actions);
    }

    return Results.Ok(new { deleted = entities.Count });
})
.WithName("ClearBeers");

app.Run();

record Beer(int Id, string BrewersName, string BeerName, string Style, decimal Abv, string Description, string ServingMethod);

record RatingRequest(string UserId, int BeerId, int Rating, string? Notes);

class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "user";
    public string RowKey { get; set; } = string.Empty; // userId
    public DateTimeOffset RegisteredAt { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }
}

class RatingEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // userId
    public string RowKey       { get; set; } = string.Empty; // beerId
    public int    Rating       { get; set; }
    public string? Notes       { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }
}

class BeerEntity : ITableEntity
{
    public string PartitionKey  { get; set; } = "beer";
    public string RowKey        { get; set; } = string.Empty; // beerId
    public string BrewersName   { get; set; } = string.Empty;
    public string BeerName      { get; set; } = string.Empty;
    public string Style         { get; set; } = string.Empty;
    public decimal Abv          { get; set; }
    public string Description   { get; set; } = string.Empty;
    public string ServingMethod { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }
}
