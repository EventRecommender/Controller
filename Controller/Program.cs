using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Controller.model;

var builder = WebApplication.CreateBuilder(args);

// TODO add port
string activityUrl = "http://activity-service";
string recommenderUrl = "http://recommender-service";
string userManagerUrl = "http://usermanager-service";

HttpClient client = new HttpClient();

// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
}


app.UseStaticFiles();
app.UseRouting();

app.MapFallbackToFile("index.html");


string removeSlashes(string input)
{
    return input.Replace("\\", "");
}


app.MapPost("/createActivity", async (activityCreation act) =>
{
    try
    {
        UriBuilder uriBuilder = new UriBuilder(activityUrl);
        uriBuilder.Query = $"title={act.title}" +
                           $"&host={act.host}" +
                           $"&location={act.city}" +
                           $"&date={act.date}&imageurl={act.image}" +
                           $"&url=&description={act.description}" +
                           $"&type={act.type}";

        uriBuilder.Path = "/AddActivity";
        StringContent content = new StringContent("");
        HttpResponseMessage answer = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, content);
        if (answer.IsSuccessStatusCode)
            return Results.Ok();
        return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong, try again later");
    }
});

app.MapPost("/createUser", async (Tuple<accountCreation,List<string>> acc) =>
{
    try
    {
        UriBuilder uriBuilder = new UriBuilder(userManagerUrl);
        uriBuilder.Path = "/Create";

        //Create user
        StringContent content = new StringContent(JsonSerializer.Serialize(acc.Item1), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PutAsync(requestUri: uriBuilder.Uri.AbsoluteUri, content: content);
        if (!response.IsSuccessStatusCode)    
            return Results.Conflict();

        int id = int.Parse(await response.Content.ReadAsStringAsync());

        //Create interest
        string initial_types = JsonSerializer.Serialize(acc.Item2);
        uriBuilder = new UriBuilder(recommenderUrl);
        uriBuilder.Path = "/CreateUserInterests";
        uriBuilder.Query = $"userid={id}&initial_types={initial_types}";

        response = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, new StringContent(""));
        if (!response.IsSuccessStatusCode)
            return Results.Problem("something went wrong");
        return Results.Ok(id);
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

        UriBuilder uriBuilder = new UriBuilder(recommenderUrl);
        uriBuilder.Path = "/UpdateUserInterests";
        uriBuilder.Query = $"userid={userid}" +
                           $"&activity_types={activity_types}" +
                           $"&update_type={update_type}";

        StringContent content = new StringContent("");
        HttpResponseMessage responseMessage = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, content);
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
        UriBuilder uriBuilder = new UriBuilder(userManagerUrl);
        uriBuilder.Path = "/login";
        StringContent content = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
        HttpResponseMessage answer = await client.PostAsync(requestUri: uriBuilder.Uri.AbsoluteUri, content: content);
        
        if (answer.IsSuccessStatusCode)
        {
            var answerObj = await answer.Content.ReadFromJsonAsync<dynamic>();
            return Results.Json(answerObj);
        }
        else return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.MapGet("/getUsers", async (string token) => 
{
    try
    {
        UriBuilder uriBuilder = new UriBuilder(userManagerUrl);
        uriBuilder.Path = "/FetchAllUsers";

        //Making client with token
        HttpClient clientWithToken = new HttpClient();
        clientWithToken.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await clientWithToken.GetAsync(uriBuilder.Uri.AbsoluteUri);
        if (response.IsSuccessStatusCode)
        {
            var answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
            return Results.Json(answerObj);
        }
        return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.MapGet("/getRecommendations", async (int userid) =>
{
    try
    {
        UriBuilder uriBuilder = new UriBuilder(recommenderUrl);
        uriBuilder.Query = $"userid={userid}";
        uriBuilder.Path = "/GetRecommendation";
        var response = await client.GetAsync(uriBuilder.Uri.AbsoluteUri);
        
        if (!response.IsSuccessStatusCode)
        {
            uriBuilder.Path = "/CalculateRecommendation";
            response = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, new StringContent(""));
            if (!response.IsSuccessStatusCode)
                return Results.Problem("something went wrong");

            uriBuilder.Path = "/GetRecommendation";
            response = await client.GetAsync(uriBuilder.Uri.AbsoluteUri);
            if (!response.IsSuccessStatusCode)
            {
                return Results.BadRequest();
            }
        }

        //Getting activities
        uriBuilder = new UriBuilder(activityUrl);
        uriBuilder.Path = "/GetActivitiesByPreference";

        var answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
        uriBuilder.Query = $"&jsonPreferenceList={JsonSerializer.Serialize(answerObj)}";

        response = await client.GetAsync(uriBuilder.Uri.AbsoluteUri);
        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(response.Content.ToString());
        }
        answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
        return Results.Json(answerObj);
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.MapGet("/getIncommingActivities", async (int monthsForward, string area) =>
{
    try
    {
        UriBuilder uriBuilder = new UriBuilder(activityUrl);
        uriBuilder.Query = $"function=areaTime&area={area}&monthsForward={monthsForward}&jsonActivityList=";
        uriBuilder.Path = "/GetActivities";

        HttpResponseMessage answer = await client.GetAsync(requestUri: uriBuilder.Uri.AbsoluteUri);
        if (answer.IsSuccessStatusCode)
        {
            var answerObj = await answer.Content.ReadFromJsonAsync<dynamic>();
            return Results.Json(answerObj);
        }
        else
            return Results.BadRequest();
    }
    catch 
    {
        return Results.Problem("something went wrong"); 
    }
});

app.MapGet("/getUserActivties", async (int userid) =>
{
    try
    {
        UriBuilder uriBuilder = new UriBuilder(activityUrl);
        uriBuilder.Path = "/GetUserActivities";
        uriBuilder.Query = $"userID={userid}";

        var response = await client.GetAsync(uriBuilder.Uri.AbsoluteUri);
        if (response.IsSuccessStatusCode)
        {
            var answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
            return Results.Json(answerObj);
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
        UriBuilder uriBuilder = new UriBuilder(activityUrl);
        uriBuilder.Path = "/RemoveActivities";
        uriBuilder.Query = $"jsonActivityList={activityList}";

        var response = await client.DeleteAsync(uriBuilder.Uri.AbsoluteUri);
        if (response.IsSuccessStatusCode)
        {
            return Results.Ok();
        }
        else return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.Run();
