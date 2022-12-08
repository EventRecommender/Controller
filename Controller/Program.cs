using System.Text.Json;
using Controller.model;

var builder = WebApplication.CreateBuilder(args);

// TODO add port
string activityUrl = "http://activity";
string recommenderUrl = "http://recommender";
string userManagerUrl = "http://usermanager";

HttpClient client = new HttpClient();

// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
}


//TODO 
// - endpoints
// - auth
// - smoke test
// - global smoke test


/*
ActivityList.js
CreateActivity.js
CreatedActivities.js
CreateUser.js
Loginpage.js
UserList.js
RecommendedActivities.js
Calendar?
*/

app.MapGet("/getActivities", () =>
{
    return Results.Problem("not implemented");

});

app.MapGet("/createActivity", async (activityCreation act) =>
{
    try
    {
        UriBuilder builder = new UriBuilder(activityUrl);
        builder.Query = $"title='{act.title}'&host='{act.host}'&location='{act.city}'&date='{act.date}'&imageurl='{act.image}'&url=''&description={act.description}";
        HttpResponseMessage answer = await client.GetAsync(builder.Uri.AbsoluteUri);
        if (answer.IsSuccessStatusCode)
            return Results.Accepted();
        else return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong try again later");
    }
});

app.MapGet("/getCreatedActivities", async (int userid) =>
{
    try
    {
        HttpResponseMessage answer = await client.GetAsync(activityUrl + "/GetUserActivities");
        if (answer.IsSuccessStatusCode)
        {
            string content = await answer.Content.ReadAsStringAsync();
            return Results.Accepted(content);
        }
        else return Results.BadRequest();
}
    catch
    {
        return Results.Problem("something went wrong, try again later");
    }

});

app.MapPost("/createUser", (accountCreation acc) =>
{

    return JsonSerializer.Serialize(acc);
});

app.MapPost("/login", async (Login login) =>
{
    try
    {
        //TODO FIX
        UriBuilder builder = new UriBuilder(activityUrl);
        builder.Query = $"username='{login.Username}'&password='{login.Password}'";
        StringContent content = new StringContent("");
        HttpResponseMessage answer = await client.PostAsync(requestUri: builder.Uri.AbsoluteUri, content: content);
        if (answer.IsSuccessStatusCode)
            //TODO GET TOKEN
            return Results.Accepted();
        else return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong, try again later");
    }
});

app.MapGet("/getUsers", () => { // <- this boy weird
    return Results.Problem("not implemented");

});

app.MapGet("/getRecommendations", () =>
{
    return Results.Problem("not implemented");
});

app.MapGet("/getIncommingActivities", () =>
{
    return Results.Problem("not implemented");
});

//app.UseStaticFiles();
//app.UseRouting();
//app.MapFallbackToFile("index.html"); ;

app.Run();
