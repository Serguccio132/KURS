
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

CookieContainer cookies = new CookieContainer();
HttpClient client = new HttpClient();
HttpClientHandler handler = new HttpClientHandler();

bool LoginOnServer(string? username, string? password){
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)){
        Console.WriteLine("Username and password cannot be empty.");
        return false;
    }

    string request = "/login";
    var json_data = new {
        login = username,
        currentpassword = password
    };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");
    try 
    {
        var response = client.PostAsync(request, content).Result;
        if (response.IsSuccessStatusCode) {
            Console.WriteLine("Authorization successful");
            IEnumerable<Cookie> response_Cookies = cookies.GetAllCookies();
            foreach (Cookie cookie in response_Cookies)
            {
                Console.WriteLine($"{cookie.Name}: {cookie.Value}");
            }
            return true;
        }
        else {
            Console.WriteLine($"Authorization failed:{response.StatusCode}");
            return false;
    }
    }
    catch (HttpRequestException ex)
        {
            Console.WriteLine($"Login request failed:{ex.Message}");
            return false;
        }
    catch (JsonException ex)
        {
            Console.WriteLine($"JSON Deserialization failed:{ex.Message}");
            return false;
        }
}
async Task<string> SignUp(string? username, string? password){
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)){
        return "Failed to sign up";
    }

    string request = "/signup";
    var json_data = new {
        login = username,
        currentpassword = password
    };


    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(request, content);
    if (response.IsSuccessStatusCode) {
        Console.WriteLine($"Successful registration");
        return await response.Content.ReadAsStringAsync();
    }
    else 
    {
        return $"Failed to register:{response.StatusCode}";
    }
    }
async Task<string> AddText(string? username, string? AddedText){
    string request = "/AddText";

    var json_data = new {
        login = username,
        textindb = AddedText
    };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    var response =  await client.PostAsync(request, content);
    if (response.IsSuccessStatusCode){
        return await response.Content.ReadAsStringAsync();
    }
    else {
        return $"Failed to add text:{response.StatusCode}";
    }
}
async Task<string> CheckOneText(string? username, string? text_id)
{
    string request = "/CheckOneText";
    var json_data = new { login = username, textid = text_id };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    var requestMessage = new HttpRequestMessage(HttpMethod.Get, request)
    {
        Content = content
    };
    var response = await client.SendAsync(requestMessage);
    if (response.IsSuccessStatusCode)
    {
        return await response.Content.ReadAsStringAsync();
    }
    else
    {
        return $"Failed to check one text:{response.StatusCode}";
    }
}
async Task<string> ChangeText(string? username, string? text_id, string? textindb)
{
    string request = "/ChangeText";
    var json_data = new
    {
        login = username,
        textid = text_id,
        textindb = textindb
    };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    var response = await client.PatchAsync(request, content);

    if (response.IsSuccessStatusCode)
    {
        return await response.Content.ReadAsStringAsync();
    }
    else
    {
        return $"Failed to change the text:{response.StatusCode}";
    }
}
async Task<string> DeleteText(string? username, string? textid)
{
    string request = "/DeleteText";
    var json_data = new
    {
        login = username,
        textid = textid
    };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");
    var requestMessage = new HttpRequestMessage(HttpMethod.Delete, request)
    {
        Content = content
    };
    
    try
    {
        var response = await client.SendAsync(requestMessage);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            return $"Failed to delete text: {response.StatusCode}";
        }
    }
    catch (Exception ex)
    {
        return $"Error during DELETE request: {ex.Message}";
    }
}
async Task<string> CheckAllTexts(string? username)
{
    string request = "/CheckAllTexts";
    var json_data = new { login = username };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");
    var requestMessage = new HttpRequestMessage(HttpMethod.Get, request)
    {
        Content = content
    };
    var response = await client.SendAsync(requestMessage);
    if (response.IsSuccessStatusCode)
    {
        return await response.Content.ReadAsStringAsync();
    }
    else
    {
        return $"Failed to check all texts:{response.StatusCode}";
    }
}
async Task<string> CryptedText(string? username, string? textid){
    string request = "/CryptedText";
    var json_data = new {
        login = username,
        textid = textid
    };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    var response =  await client.PostAsync(request, content);
    if (response.IsSuccessStatusCode){
        return await response.Content.ReadAsStringAsync();
    }
    else {
        return $"Failed to encrypt:{response.StatusCode}";
    }
}
async Task<string> DecryptedText(string? username, string? textid){
    string request = "/DecryptedText";
    var json_data = new {
        login = username,
        textid = textid
    };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");
    var response =  await client.PostAsync(request, content);
    if (response.IsSuccessStatusCode){
        return await response.Content.ReadAsStringAsync();
    }
    else {
        return $"Failed to decrypt:{response.StatusCode}";
    }
}
async Task<string> CheckAllRequests(string? username)
{
    string request = "/CheckAllRequests";
    var json_data = new { login = username };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    var requestMessage = new HttpRequestMessage(HttpMethod.Get, request)
    {
        Content = content
    };
    var response = await client.SendAsync(requestMessage);
    if (response.IsSuccessStatusCode)
    {
        return await response.Content.ReadAsStringAsync();
    }
    else
    {
        return $"Failed to check all requests:{response.StatusCode}";
    }
}
async Task<string> DeleteRequests(string? username){
    string request = "/DeleteRequests";
    var json_data = new {
        login = username
    };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    var requestMessage = new HttpRequestMessage(HttpMethod.Delete, request){
        Content = content
    };
    var response =  await client.SendAsync(requestMessage);
    if (response.IsSuccessStatusCode){
        return await response.Content.ReadAsStringAsync();
    }
    else {
        return $"Failed to delete all requests:{response.StatusCode}";
    }
}
async Task<string> UpdatePassword(string? username, string?  current_password, string? new_password){
    string request = "/UpdatePassword";
    var json_data = new {
        login = username,
        currentpassword = current_password,
        newpassword = new_password
    };
    string JsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    var response =  await client.PatchAsync(request, content);
    if (response.IsSuccessStatusCode){
        return await response.Content.ReadAsStringAsync();
    }
    else {
        return $"Faild to update the password:{response.StatusCode}";
    }
}

const string DEFAULT_SERVER_URL = "http://localhost:5000";
Console.WriteLine("Write server URL (http://localhost:5000 default):");
string? server_url = Console.ReadLine();
if (server_url == null || server_url.Length == 0) {
    server_url = DEFAULT_SERVER_URL;
} 
try
{
    client.BaseAddress = new Uri(server_url);
    while (true)
    {
    Console.WriteLine("Sign up or Log in:");
    Console.WriteLine("1. Sign up");
    Console.WriteLine("2. Log in");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
        Console.WriteLine("Write a login and password");
        string? username = Console.ReadLine();
        string? password = Console.ReadLine();
        Console.WriteLine(SignUp(username, password).Result);
        break;

        case "2":
        Console.WriteLine("Enter your login and password");
        while (true)
        {
            username = Console.ReadLine();
            password = Console.ReadLine();
            if (!LoginOnServer(username, password))
            {
                return;
            }
                while (true)
                    {
                    Console.WriteLine("Select a function:");
                    Console.WriteLine("1. AddText (POST)");
                    Console.WriteLine("2. ChangeText (PATCH)");
                    Console.WriteLine("3. DeleteText (DELETE)");
                    Console.WriteLine("4. CheckOneText (GET)");
                    Console.WriteLine("5. CheckAllTexts (GET)");
                    Console.WriteLine("6. CryptedText (POST)");
                    Console.WriteLine("7. DecryptedText (POST)");
                    Console.WriteLine("8. CheckRequests (GET)");
                    Console.WriteLine("9. DeleteRequests (DELETE)");
                    Console.WriteLine("10. UpdatePassword (PATCH)");
                    choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "1":
                        Console.WriteLine("Write a text");
                        Console.WriteLine(AddText(username, Console.ReadLine()).Result);
                        break;

                        case "2":
                        Console.WriteLine("Write an ID of the text and enter new text");
                        string? text_id = Console.ReadLine();
                        string? textindb = Console.ReadLine();
                        Console.WriteLine(ChangeText(username, text_id, textindb).Result);
                        break;

                        case "3":
                        Console.WriteLine("Write an ID of the text to delete it");
                        Console.WriteLine(DeleteText(username, Console.ReadLine()).Result);
                        break;

                        case "4":
                        Console.WriteLine("Write an ID of text to check it");
                        text_id = Console.ReadLine();
                        Console.WriteLine(CheckOneText(username, text_id).Result);
                        break;

                        case "5":
                        Console.WriteLine(CheckAllTexts(username).Result);
                        break;

                        case "6":
                        Console.WriteLine("Write an ID of the text to encrypt it");
                        Console.WriteLine(CryptedText(username, Console.ReadLine()).Result);
                        break;

                        case "7":
                        Console.WriteLine("Write an ID of the text to decrypt it");
                        Console.WriteLine(DecryptedText(username, Console.ReadLine()).Result);
                        break;

                        case "8":
                        Console.WriteLine(CheckAllRequests(username).Result);
                        break;

                        case "9":
                        Console.WriteLine(DeleteRequests(username).Result);
                        break;

                        case "10":
                        Console.WriteLine("Write your current password and a new password");
                        string? current_password = Console.ReadLine();
                        string? new_password = Console.ReadLine();
                        Console.WriteLine(UpdatePassword(username, current_password, new_password).Result);
                        break;
                    }
                    Console.WriteLine();
                    }
        }
        
    }
    Console.WriteLine();
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"An error occurred while connecting to the server: {ex.Message}");
}
catch (UriFormatException ex)
{
    Console.WriteLine($"An error occurred while parsing the server URL: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"An unexpected error occurred: {ex.GetType().Name} - {ex.Message}");
}

