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

app.UseStaticFiles();
app.UseRouting();

app.MapFallbackToFile("index.html");

app.MapGet("/getActivities", () =>
{
    return Results.Problem("not implemented");
});

app.MapPost("/createActivity", async (activityCreation act) =>
{
    try
    {
        UriBuilder builder = new UriBuilder(activityUrl);
        builder.Query = $"title='{act.title}'" +
                        $"&host='{act.host}'" +
                        $"&location='{act.city}'" +
                        $"&date='{act.date}'&imageurl='{act.image}'" +
                        $"&url=''&description={act.description}";
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

app.MapPost("/createUser", async (accountCreation acc) =>
{
    try
    {
        UriBuilder builder = new UriBuilder(userManagerUrl);
        builder.Path = "/Create";
        builder.Query = $"username='{acc.Username}'" +
                        $"&password='{acc.Password}'" +
                        $"&city='{acc.City}'" +
                        $"&role='{acc.Role}'";
        StringContent content = new StringContent("");
        HttpResponseMessage response = await client.PutAsync(requestUri: builder.Uri.AbsoluteUri, content: content);
        if (response.IsSuccessStatusCode)
            return Results.Accepted();
        return Results.Conflict();
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.MapPost("/like", async (bool isLike, int userid) =>
{
    return Results.Problem("not implemented");
});

app.MapPost("/login", async (Login login) =>
{
    try
    {
        //TODO FIX
        UriBuilder builder = new UriBuilder(userManagerUrl);
        builder.Query = $"username='{login.Username}'&password='{login.Password}'";
        builder.Path = "/login";
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

app.MapGet("/getRecommendations", async (int userid) =>
{
    try
    {
        //Get recommendation
        UriBuilder builder = new UriBuilder(recommenderUrl);
        builder.Query = $"User_ID={userid}";
        builder.Path = "/GetRecommendation";
        var response = await client.GetAsync(builder.Uri.AbsoluteUri);
        if (!response.IsSuccessStatusCode)
            return Results.Problem("something went wrong");

        string content = await response.Content.ReadAsStringAsync();
        if (content != "") // TODO OR STATUS CODE
            return Results.Accepted(content);

        //Calculate recommendation
        builder = new UriBuilder(recommenderUrl);
        builder.Query = $"User_ID={userid}";
        builder.Path = "/CalculateRecomendation";
        response = await client.GetAsync(builder.Uri.AbsoluteUri);
        if (!response.IsSuccessStatusCode)
            return Results.Problem("something went wrong");

        content = await response.Content.ReadAsStringAsync();
        return Results.Accepted(content);
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.MapGet("/getIncommingActivities", async (string monthsForward, string area) =>
{
    try
    {
        UriBuilder builder = new UriBuilder(activityUrl);
        builder.Query = $"function='areaTime'&area='{area}'&jsonActivityList=''";

        HttpResponseMessage answer = await client.GetAsync(requestUri: builder.Uri.AbsoluteUri);
        if (answer.IsSuccessStatusCode)
        {
            return Results.Accepted(await answer.Content.ReadAsStringAsync());
        }
        else
            return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong, try again later");
    }
});

app.Run();
