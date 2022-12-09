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
            return Results.Ok();
        else return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong, try again later");
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
            return Results.Ok(content);
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
            return Results.Ok();
        return Results.Conflict();
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.MapPost("/like", async (bool isLike, string activity_types, int userid) =>
{
    try
    {
        int update_type = isLike ? 0 : 1;

        UriBuilder builder = new UriBuilder(recommenderUrl);
        builder.Path = "/UpdateUserInterests";
        builder.Query = $"user_ID='{userid}'" +
                        $"&activity_types='{activity_types}'" +
                        $"&update_type='{update_type}'";

        StringContent content = new StringContent("");
        HttpResponseMessage responseMessage = await client.PostAsync(builder.Uri.AbsoluteUri, content);
        if (responseMessage.IsSuccessStatusCode)
            return Results.Ok();
        return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong");
    }

});

app.MapPost("/login", async (Login login) =>
{
    try
    {
        UriBuilder builder = new UriBuilder(userManagerUrl);
        builder.Query = $"username='{login.Username}'&password='{login.Password}'";
        builder.Path = "/login";
        StringContent content = new StringContent("");
        HttpResponseMessage answer = await client.PostAsync(requestUri: builder.Uri.AbsoluteUri, content: content);
        
        if (answer.IsSuccessStatusCode)
        {
            string answerContent = await answer.Content.ReadAsStringAsync();
            return Results.Ok(answerContent);
        }
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
            return Results.Ok(content);

        //Calculate recommendation
        builder = new UriBuilder(recommenderUrl);
        builder.Query = $"User_ID={userid}";
        builder.Path = "/CalculateRecomendation";
        response = await client.GetAsync(builder.Uri.AbsoluteUri);
        if (!response.IsSuccessStatusCode)
            return Results.Problem("something went wrong");

        content = await response.Content.ReadAsStringAsync();
        return Results.Ok(content);
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
            return Results.Ok(await answer.Content.ReadAsStringAsync());
        }
        else
            return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong, try again later");
    }
});

app.MapGet("/getUserActivties", async (int userid) =>
{
    try
    {
        UriBuilder builder = new UriBuilder(activityUrl);
        builder.Path = "/GetActivities";
        builder.Query = $"userID={userid}";

        var response = await client.GetAsync(builder.Uri.AbsoluteUri);
        if (response.IsSuccessStatusCode)
        {
            return Results.Ok(await response.Content.ReadAsStringAsync());
        }
        else return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.MapDelete("/RemoveActivity", async (string activityList) =>
{
    try
    {
        UriBuilder builder = new UriBuilder(activityUrl);
        builder.Path = "/RemoveActivities";
        builder.Query = $"jsonActivityList='{activityList}'";

        var response = await client.DeleteAsync(builder.Uri.AbsoluteUri);
        if (response.IsSuccessStatusCode)
        {
            return Results.Ok();
        }
        else return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("soemthing went wrong");
    }
});

app.Run();
