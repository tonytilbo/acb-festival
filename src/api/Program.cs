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

var app = builder.Build();

var tableClient = app.Services.GetRequiredService<TableClient>();
await tableClient.CreateIfNotExistsAsync();

var usersTableClient = app.Services.GetRequiredKeyedService<TableClient>("users");
await usersTableClient.CreateIfNotExistsAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

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

app.MapGet("/api/summary", async (TableClient ratings, [FromKeyedServices("users")] TableClient users) =>
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

    return Results.Ok(new
    {
        TotalUsers        = userEntities.Count,
        FirstRegistered   = userEntities.Count > 0 ? userEntities.Min(u => u.RegisteredAt) : (DateTimeOffset?)null,
        LastRegistered    = userEntities.Count > 0 ? userEntities.Max(u => u.RegisteredAt) : (DateTimeOffset?)null,
        TotalRatings      = totalRatings,
        BeersRated        = ratedBeerIds.Count,
        TotalBeers        = BeerData.All.Length,
    });
})
.WithName("GetSummary");

app.MapGet("/api/beers", () => Results.Ok(BeerData.All))
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

static class BeerData
{
    public static readonly Beer[] All =
    [
        new(1,  "Thornbridge Brewery",      "Jaipur",               "India Pale Ale",       5.9m, "A bold, citrus-forward IPA bursting with grapefruit, mango and pine resin. A clean bitter finish that keeps you coming back.",              "Cask"),
        new(2,  "Fuller's Brewery",         "London Pride",         "Extra Special Bitter", 4.7m, "A rich, balanced ESB with toffee malt, earthy Fuggle hops and a hint of orange peel. The definitive London classic.",                       "Cask"),
        new(3,  "Dark Star Brewing Co.",    "Hophead",              "Golden Ale",           3.8m, "Crisp and refreshing with a floral, grassy hop aroma and subtle fruit notes. Dangerously easy to drink.",                                    "Cask"),
        new(4,  "Brew By Numbers",          "01|01 Saison",         "Saison",               6.5m, "Hazy straw-yellow with spicy, peppery yeast character. Light stone fruit, lemon zest and a dry, champagne-like finish.",                    "Keg"),
        new(5,  "Beavertown Brewery",       "Gamma Ray",            "American Pale Ale",    5.4m, "A West Coast-inspired pale with huge tropical fruit aromas — passion fruit, mango and pineapple — underpinned by a firm bitterness.",        "Keg"),
        new(6,  "Oakham Ales",              "Citra",                "Golden Ale",           4.2m, "Single-hopped with Citra for an explosion of lychee, gooseberry and grapefruit. Pale, bright and supremely quaffable.",                     "Cask"),
        new(7,  "Marble Beers",             "Manchester Bitter",    "Bitter",               4.2m, "A modern take on the classic Northern bitter — light copper colour, biscuit malt base and a snappy, grassy hop finish.",                    "Cask"),
        new(8,  "Magic Rock Brewing",       "Cannonball",           "India Pale Ale",       7.4m, "Punchy, resinous and tropical. Loads of grapefruit and pine from generous dry-hopping. Full-bodied with a long, warming finish.",            "Keg"),
        new(9,  "St Austell Brewery",       "Proper Job",           "India Pale Ale",       5.5m, "A Cornish IPA with bags of citrus and stone fruit flavour, a malt backbone and a satisfyingly dry, bitter aftertaste.",                     "Cask"),
        new(10, "Tiny Rebel Brewing Co.",   "Cwtch",                "Red Ale",              4.6m, "Welsh for cuddle — warm, malty and inviting. Toffee and caramel upfront with a gentle floral hop and a smooth, soft finish.",               "Cask"),
        new(11, "Siren Craft Brew",         "Broken Dream",         "Breakfast Stout",      6.5m, "Rich and indulgent with roasted coffee, dark chocolate and a whisper of vanilla. Smooth, creamy body and a lingering bitter finish.",        "Cask"),
        new(12, "Wild Beer Co.",            "Bibble",               "Pale Ale",             4.2m, "Hazy and hop-forward with fresh lemon, white peach and herbal notes. Light-bodied and wonderfully aromatic.",                               "Cask"),
        new(13, "BrewDog",                  "Punk IPA",             "India Pale Ale",       5.6m, "The beer that rewrote the rulebook. Subversive, hoppy and attitudinal. Grapefruit, pineapple and lychee with a clean, dry finish.",         "Keg"),
        new(14, "Timothy Taylor's",         "Landlord",             "Strong Pale Ale",      4.3m, "A four-times CAMRA Champion Beer of Britain. Floral Styrian hops, crystal malt sweetness and a perfectly balanced bitter finish.",           "Cask"),
        new(15, "Cloudwater Brew Co.",      "DIPA",                 "Double IPA",           8.0m, "Season-defining double IPA. Soft, pillowy mouthfeel with intense tropical fruit, melon and a deceptively gentle bitterness for the strength.", "Keg"),
        new(16, "Harvey's Brewery",         "Sussex Best Bitter",   "Best Bitter",          4.0m, "A true traditional bitter. Rich amber colour, full malt character with hints of dried fruit and a long, dry, hoppy finish.",                 "Cask"),
        new(17, "Kernel Brewery",           "Table Beer",           "Session Pale Ale",     3.3m, "Low alcohol, high flavour — citrus zest, fresh herbs and a subtle earthiness. The perfect session companion.",                              "Cask"),
        new(18, "Moor Beer Company",        "Revival",              "Session IPA",          3.8m, "Unfiltered and unfined. Hazy gold with bold tropical and citrus hops punching well above the sessionable ABV.",                             "Cask"),
        new(19, "Hawkshead Brewery",        "Windermere Pale",      "Pale Ale",             3.5m, "Light, clean and refreshing. Delicate floral and grassy hops with a hint of lemon, reflecting the freshness of the Lake District.",         "Cask"),
        new(20, "Meantime Brewing Company", "London Lager",         "Lager",                4.5m, "Crisp European-style lager brewed in Greenwich. Noble hops, soft bready malt and a clean, refreshing finish that honours London's brewing heritage.", "Keg"),
    ];
}
