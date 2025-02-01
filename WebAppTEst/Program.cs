using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

STWebAdapter dt = new STWebAdapter();
DBManager db = new DBManager();

app.MapPost("/AddText",[Authorize] ([FromBody] TextStruct at) => {
    if (db.AddText(at.textindb)){
        db.AddRequest(at.login, "AddText");
        return Results.Ok($"Added new text: {at.textindb}");
    }
    else return Results.Problem("Something went wrong");
});
app.MapGet("/CheckOneText",[Authorize] ([FromBody] TextStruct cht) => {
    var text = db.CheckOneText(cht.login, cht.textid);
    if (text != null ){
        if (db.AddRequest(cht.login, "CheckText")){
          return Results.Ok(text);
        }
        else Results.Problem("Failed to check text");
    }
    return Results.Problem("Failed to check text");
});
app.MapPatch("/ChangeText",[Authorize] ([FromBody] TextStruct chgt) => {
    if(db.EditTextNewRC(chgt.textindb, int.Parse(chgt.textid))){
        db.AddRequest(chgt.login, "ChangeText");
        return Results.Ok($"Edited text: {chgt.textid}");
    } 
    else return Results.Problem("Something went wrong");
});
app.MapDelete("/DeleteText",[Authorize] ([FromBody] TextStruct delt) => {
    if (db.DeleteText(delt.login, delt.textid)){
        if (db.AddRequest(delt.login, "DeleteText")){
          return Results.Ok("Text deleted successfully");
        }
        else return Results.Problem("Failed to add text");
    }
    return Results.Problem("Failed to add text");
});
app.MapGet("/CheckAllTexts",[Authorize] ([FromBody] TextStruct challt) => {
    var texts = db.CheckAllTexts(challt.login);
    if (texts != null ){
        if (db.AddRequest(challt.login, "CheckAllText")){
          return Results.Ok(texts);
        }
        else Results.Problem("Failed to check all texts");
    }
    return Results.Problem("Failed to check all texts");
});
app.MapPost("/CryptedText",[Authorize] async ([FromBody] TextStruct crypt) => {
    if(db.getSingleWithRC != null){
        var result = db.getSingleWithRC(int.Parse(crypt.textid));
        string text = result.Value.Text;
        int rows = result.Value.Rows;
        int columns = result.Value.Columns;
        string enc = Codec.Encrypt(text, rows, columns);
        db.EditTextOldRC(enc, int.Parse(crypt.textid));
        db.AddRequest(crypt.login, "CryptedText");
        return Results.Ok($"Encrypted text: {enc}");
    }
    else return Results.Problem("Something went wrong");
});
app.MapPost("/DecryptedText", async ([FromBody] TextStruct decrypt) => {
    if(db.getSingleWithRC != null){
        var result = db.getSingleWithRC(int.Parse(decrypt.textid));
        string text = result.Value.Text;
        int rows = result.Value.Rows;
        int columns = result.Value.Columns;
        string dec = Codec.Decrypt(text, rows, columns);
        db.EditTextOldRC(dec, int.Parse(decrypt.textid));
        db.AddRequest(decrypt.login, "DecryptedText");
        return Results.Ok($"Decrypted text: {dec}");
    }
    else return Results.Problem("Something went wrong");
});
app.MapGet("/CheckAllRequests",[Authorize] ([FromBody] TextStruct chkr) => {
    var texts = db.CheckAllRequests(chkr.login);
    if (texts != null ){
        if (db.AddRequest(chkr.login, "CheckAllRequests")){
          return Results.Ok(texts);
        }
        else Results.Problem("Failed to check all requests");
    }
    return Results.Problem("Failed to check all requests");
});
app.MapDelete("/DeleteRequests",[Authorize] ([FromBody] TextStruct delr) => {
    if (db.DeleteRequests(delr.login) == true){
        return Results.Ok("Requests deleted successfully");
    }
    return Results.Problem("Failed to delete requests");
});
app.MapPost("/login", ([FromBody] UserStruct log, HttpContext context) =>
    {
        if (!db.CheckUser(log.login, log.currentpassword)){
            return Results.Unauthorized();
        }
        return dt.LogIn(log.login, log.currentpassword, context, db, "login").Result;
    });
app.MapPost("/signup", ([FromBody] UserStruct sign) => {
    if (db.AddUser(sign.login, sign.currentpassword)) {
        return Results.Ok("User " + sign.login + " registrated");
    }
    return Results.Problem("Failed to register user" + sign.login);
});
app.MapPatch("/UpdatePassword", [Authorize] ([FromBody] UserStruct upd, HttpContext context ) => {
    if (!db.CheckUser(upd.login, upd.currentpassword)) {
        return Results.Unauthorized();
    }
    if (!db.UpdatePassword(upd.login, upd.newpassword)) {
        return Results.Problem("Failed to update password");
    }
    return dt.UpdatePassword(upd.login, upd.newpassword, context).Result;

});

const string DB_PATH = "/home/Serguccio/Kurs/users.db";
if (!db.ConnectToDB(DB_PATH)){
    Console.WriteLine("Failed connection to DB " + DB_PATH);
    Console.WriteLine("Shutting down...");
    return;
}
app.Run();
db.Disconnect();
struct TextStruct 
{
    public string login {get; set; }
    public string textid {get; set; }
    public string textindb {get; set; }
}
struct UserStruct 
{
    public string login {get; set; }
    public string currentpassword {get; set; }
    public string newpassword {get; set; }
}


public struct TextData 
{
  public int texts_text_id {get; set;}
  public string? text_content {get; set;}
}
public struct RequestsData 
{
  public int request_id {get; set;}
  public string? operation_type {get; set;}
}
public struct STResult {
    public STResult(string message){
        input = message ?? throw new ArgumentNullException(nameof(message), "Message cannot be null");
        ColumnOrder = new int[0];
        RowOrder = new int[0];

    }
    public STResult(string message, int[] co, int[]?ro){
      input = message ?? throw new ArgumentNullException(nameof(message), "Message cannot be null");
      ColumnOrder = co ?? throw new ArgumentNullException(nameof(co), "Message cannot be null");
      RowOrder = ro ?? throw new ArgumentNullException(nameof(ro), "Message cannot be null");
    }
    public string input {get; set;}
    public int[] ColumnOrder {get; set;}
    public int[] RowOrder {get; set; }
}

public class AuthOptions {
    public const string ISSUER = "WebAppTest";
    public const string AUDIENCE = "WebAppTestAudience";
    public static SymmetricSecurityKey GetKey(){
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WebAppTestPasswordWebAppTestPasswordWebAppTestPasswordWebAppTestPassword"));
    }
}
public class STWebAdapter
{
    public async Task<IResult> LogIn(string login, string password, HttpContext context, DBManager db, string type)
    {
        if (!db.CheckUser(login, password))
        {
            return Results.Unauthorized();
        }
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, login) };
        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        return Results.Ok();
    }
    public async Task<IResult> UpdatePassword(string login, string password, HttpContext context)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, login) };
        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        return Results.Ok();
    }
}

public class Codec
{
    public static string Encrypt(string input, int rows, int columns)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input text cannot be null or empty.");

        if (rows * columns < input.Length)
            throw new ArgumentException("Table size is too small for the input text.");

        char[,] table = new char[rows, columns];
        int index = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                table[i, j] = index < input.Length ? input[index++] : ' ';
            }
        }

        StringBuilder encryptedText = new StringBuilder();

        for (int j = columns - 1; j >= 0; j--)
        {
            for (int i = 0; i < rows; i++)
            {
                encryptedText.Append(table[i, j]);
            }
        }

        return encryptedText.ToString();
    }

    public static string Decrypt(string input, int rows, int columns)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input text cannot be null or empty.");

        if (rows * columns < input.Length)
            throw new ArgumentException("Table size is too small for the input text.");

        char[,] table = new char[rows, columns];
        int index = 0;

        for (int j = columns - 1; j >= 0; j--)
        {
            for (int i = 0; i < rows; i++)
            {
                table[i, j] = index < input.Length ? input[index++] : ' ';
            }
        }

        StringBuilder decryptedText = new StringBuilder();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                decryptedText.Append(table[i, j]);
            }
        }

        return decryptedText.ToString().TrimEnd();
    }
}
