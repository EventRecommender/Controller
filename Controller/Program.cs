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

async Task<IResult> HandleProblemFromService(HttpResponseMessage response)
{
    Console.WriteLine(await response.Content.ReadAsStringAsync());
    return Results.Problem("something went wrong");
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
        HttpResponseMessage response = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, content);
        if (response.IsSuccessStatusCode)
            return Results.Ok();
        if ((int)response.StatusCode >= 500)
        {
            return await HandleProblemFromService(response);
        }
        return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong");
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
        {
            if ((int)response.StatusCode == 409)
            {
                return Results.Conflict();
            }
            else if ((int)response.StatusCode >= 500)
            {
                return await HandleProblemFromService(response);
            }
            else
            {
                return Results.BadRequest();
            }
        }
        

        int id = int.Parse(await response.Content.ReadAsStringAsync());

        //Create interest
        string initial_types = JsonSerializer.Serialize(acc.Item2);
        uriBuilder = new UriBuilder(recommenderUrl);
        uriBuilder.Path = "/CreateUserInterests";
        uriBuilder.Query = $"userid={id}&initial_types={initial_types}";

        response = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, new StringContent(""));
        if (!response.IsSuccessStatusCode)
        {
            if((int)response.StatusCode >= 500)
            {
                return await HandleProblemFromService(response);
            }
            else
            {
                return Results.BadRequest();
            }

        }
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
        {
            return Results.Ok();
        }
        else if ((int)responseMessage.StatusCode >= 500)
        {
            return await HandleProblemFromService(responseMessage);
        }
        else
        {
            return Results.BadRequest();
        }
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
        HttpResponseMessage response = await client.PostAsync(requestUri: uriBuilder.Uri.AbsoluteUri, content: content);
        
        if (response.IsSuccessStatusCode)
        {
            var answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
            return Results.Json(answerObj);
        }
        if ((int)response.StatusCode >= 500)
        {
            return await HandleProblemFromService(response);
        }
        else
        {
            return Results.BadRequest();
        }
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
        else if ((int)response.StatusCode >= 500)
        {
            return await HandleProblemFromService(response);
        }
        else
        {
            return Results.BadRequest();
        }
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
        
        if ((int)response.StatusCode == 400)
        {
            uriBuilder.Path = "/CalculateRecommendation";
            response = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, new StringContent(""));
            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode >= 500)
                {
                    return await HandleProblemFromService(response);
                }
                else
                {
                    return Results.BadRequest();
                }
            }

            uriBuilder.Path = "/GetRecommendation";
            response = await client.GetAsync(uriBuilder.Uri.AbsoluteUri);
            if (!response.IsSuccessStatusCode)
            {
                return Results.BadRequest();
            }
        }
        else if ((int)response.StatusCode >= 500)
        {
            return await HandleProblemFromService(response);
        }

        //Getting activities
        uriBuilder = new UriBuilder(activityUrl);
        uriBuilder.Path = "/GetActivitiesByPreference";

        var answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
        uriBuilder.Query = $"&jsonPreferenceList={JsonSerializer.Serialize(answerObj)}";

        response = await client.GetAsync(uriBuilder.Uri.AbsoluteUri);
        if (!response.IsSuccessStatusCode)
        {
            return await HandleProblemFromService(response);
        }
        answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
        return Results.Json(answerObj);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/getIncommingActivities", async (int monthsForward, string area) =>
{
    try
    {
        UriBuilder uriBuilder = new UriBuilder(activityUrl);
        uriBuilder.Query = $"function=areaTime&area={area}&monthsForward={monthsForward}&jsonActivityList=";
        uriBuilder.Path = "/GetActivities";

        HttpResponseMessage respons = await client.GetAsync(requestUri: uriBuilder.Uri.AbsoluteUri);
        if (respons.IsSuccessStatusCode)
        {
            var answerObj = await respons.Content.ReadFromJsonAsync<dynamic>();
            return Results.Json(answerObj);
        }
        else if ((int) respons.StatusCode >= 500)
        {
            return await HandleProblemFromService(respons);
        }
        else
            return Results.BadRequest();
    }
    catch 
    {
        return Results.Problem("something went wrong"); 
    }
});

app.MapGet("/getUserActivties", async (string username) =>
{
    try
    {
        UriBuilder uriBuilder = new UriBuilder(activityUrl);
        uriBuilder.Path = "/GetUserActivities";
        uriBuilder.Query = $"username={username}";

        var response = await client.GetAsync(uriBuilder.Uri.AbsoluteUri);
        if (response.IsSuccessStatusCode)
        {
            var answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
            return Results.Json(answerObj);
        }
        else if ((int)response.StatusCode >= 500)
        {
            return await HandleProblemFromService(response);
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
        else if ((int)response.StatusCode >= 500)
        {
            return await HandleProblemFromService(response);
        }
        else return Results.BadRequest();
    }
    catch
    {
        return Results.Problem("something went wrong");
    }
});

app.MapDelete("/removeUser", async (string token, int userid) => {
    try{
        UriBuilder uriBuilder = new UriBuilder(userManagerUrl);
        uriBuilder.Path = "/delete";
        uriBuilder.Query = $"&userId={userid}";

        //Making client with token
        HttpClient clientWithToken = new HttpClient();
        clientWithToken.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await clientWithToken.GetAsync(uriBuilder.Uri.AbsoluteUri);
        if (response.IsSuccessStatusCode)
        {
            var answerObj = await response.Content.ReadFromJsonAsync<dynamic>();
            return Results.Json(answerObj);
        }
        if ((int)response.StatusCode >= 500)
        {
            return await HandleProblemFromService(response);
        }
        return Results.BadRequest();
    }
    catch{
        return Results.Problem("Something went wrong");
    }
});

app.Run();
