using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

public class DBManager {
    private SqliteConnection? connection = null;

    private string HashPassword(string password) {
        using (var algorithm = SHA256.Create()) {
            var bytes_hash = algorithm.ComputeHash(Encoding.Unicode.GetBytes(password));
            return Encoding.Unicode.GetString(bytes_hash);
        }
    }
    public bool ConnectToDB(string path){
        
        Console.WriteLine("Connection to database");
        try
        {
            connection = new SqliteConnection("Data Source=" + path);
            connection.Open();

            if (connection.State != System.Data.ConnectionState.Open) {
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp){
            Console.WriteLine(exp.Message);
            return false;
        }
        Console.WriteLine("Done");
        return true;
    }
    public void Disconnect(){
        if (null == connection) 
            return;

        if (connection.State != System.Data.ConnectionState.Open){
            return;
        }
        connection.Close();
        Console.WriteLine("Disconnect");
    }

    public bool AddUser(string login, string password){
        
        if (null == connection) 
            return false;

        if (connection.State != System.Data.ConnectionState.Open){
            return false;
        }
        string REQUEST = "INSERT INTO users (Login, password) VALUES ('" + login + "', '" + HashPassword(password) + "')";
        var command = new SqliteCommand(REQUEST, connection);
        
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();
        }
        catch (Exception exp){
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result)
            return true;
        else 
            return false;
    }
    public bool CheckUser(string login, string password){
        if (null == connection) 
            return false;
        if (connection.State != System.Data.ConnectionState.Open){
            return false;
        }
        string REQUEST =  "SELECT Login, password FROM users WHERE Login='" + login + "' AND password='" + HashPassword(password) + "'";
        var command = new SqliteCommand(REQUEST, connection);
        try
        {
            var reader = command.ExecuteReader();
            if (reader.HasRows) return true;
            else return false;
        }
        catch (Exception exp){
            Console.WriteLine(exp.Message);
            return false;
        }
    }
    public bool AddTextInDB(string login, string textindb, string text_state){
        
        if (null == connection) 
            return false;

        if (connection.State != System.Data.ConnectionState.Open){
            return false;
        }
        string REQUEST = "INSERT INTO texts (texts_user_login, text_state, text_content) VALUES ('" + login + "', '" + text_state + "', '" + textindb + "')";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        { 
            result = command.ExecuteNonQuery();
        }
        catch (Exception exp){
            Console.WriteLine(exp.Message);
            return false;
        }
        if (1 == result)
            return true;
        else 
            return false;
    }
    public bool ChangeText(string login, string textid, string textindb){
        
        if (null == connection) 
            return false;

        if (connection.State != System.Data.ConnectionState.Open)
            return false;

        string REQUEST = "UPDATE texts SET text_content='" + textindb + "' WHERE texts_user_login='" + login + "' AND texts_text_id='" + textid + "'";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result)
            return true; 
        else 
            return false;
    }
    public bool DeleteText(string login, string textid){
        
        if (null == connection) 
            return false;

        if (connection.State != System.Data.ConnectionState.Open)
            return false;

        string REQUEST = "DELETE FROM texts WHERE texts_user_login='" + login + "' AND texts_text_id='" + textid + "'";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result)
            return true;
        else 
            return false;
    }
    public bool AddRequest(string login, string request_type){
        
        if (null == connection) 
            return false;

        if (connection.State != System.Data.ConnectionState.Open)
            return false;

        string REQUEST = "INSERT INTO requests (user_login, operation_type) VALUES ('" + login + "', '" + request_type + "')";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result)
            return true;
        else 
            return false;
    }
    public string? CheckOneText(string login, string text_id){
        
        if (null == connection) 
            return null;

        if (connection.State != System.Data.ConnectionState.Open)
            return null;
        
        string REQUEST = "SELECT text_content FROM texts WHERE texts_user_login='" + login + "' AND texts_text_id='" + Convert.ToInt32(text_id) + "'";
        var command = new SqliteCommand(REQUEST, connection);
        string? main_text = null;
        try
        {
            
            object? resultObject = command.ExecuteScalar();
            Console.WriteLine(resultObject);
            if (resultObject != null) {
                main_text = Convert.ToString(resultObject);
                Console.WriteLine(main_text);
            }
            else {
                main_text = null;
            }

        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return null;
        }

        if (main_text != null)
            return main_text;
        else 
            return null;
    }
    public List<TextData> CheckAllTexts(string login)
    {
        
        if (null == connection) 
            return new List<TextData>();

        if (connection.State != System.Data.ConnectionState.Open)
            return new List<TextData>();

        var textslist = new List<TextData>();
        string REQUEST = "SELECT texts_text_id, text_content FROM texts";
        var command = new SqliteCommand(REQUEST, connection);
        try
        {
            
            using (var reader = command.ExecuteReader()){
                while (reader.Read()){
                    textslist.Add(new TextData {
                        texts_text_id = reader.GetInt32(0),
                        text_content = reader.GetString(1)
                    });
                }
            }

        }
        catch (Exception exp){
            Console.WriteLine(exp.Message);
            return new List<TextData>();
        }

        return textslist;
    }
    public List<RequestsData > CheckAllRequests(string login){
        
        if (null == connection) return new List<RequestsData >();

        if (connection.State != System.Data.ConnectionState.Open){
            return new List<RequestsData >();
        }
        var textslist = new List<RequestsData >();
        string REQUEST = "SELECT request_id, operation_type FROM requests WHERE user_login='" + login + "'";;
        var command = new SqliteCommand(REQUEST, connection);
        try
        {
            
            using (var reader = command.ExecuteReader()){
                while (reader.Read()){
                    textslist.Add(new RequestsData  {
                        request_id = reader.GetInt32(0),
                        operation_type = reader.GetString(1)
                    });
                }
            }

        }
        catch (Exception exp){
            Console.WriteLine(exp.Message);
            return new List<RequestsData >();
        }

        return textslist;
    }
    public bool DeleteRequests(string login){
        
        if (null == connection) 
            return false;

        if (connection.State != System.Data.ConnectionState.Open){
            return false;
        }
        string REQUEST = "DELETE FROM requests WHERE user_login='" + login + "'";
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();

        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }

        return true;
    }
    public bool UpdatePassword(string login, string newPassword)
    {
        if (null == connection) 
            return false;

        if (connection.State != System.Data.ConnectionState.Open)
        {
            return false;
        }

        string updateRequest = "UPDATE users SET password = '" + HashPassword(newPassword) + "' WHERE Login = '" + login + "'";
        
        var command = new SqliteCommand(updateRequest, connection);

        try
        {
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool AddText(string newText){
        (int rows, int columns) = TableDimensionCalculator.DetermineDimensions(newText.Length);
        string REQUEST = $"INSERT into texts (text_content, Rows, Columns) VALUES ('{newText}', {rows}, {columns})";
        return validateResponse(REQUEST);
    }
    
    public bool EditTextOldRC(string newText, int ID){
        string REQUEST = $"UPDATE texts SET text_content = '{newText}' WHERE texts_text_id = {ID}";
        return validateResponse(REQUEST);
    }

    public bool EditTextNewRC(string newText, int ID){
        (int rows, int columns) = TableDimensionCalculator.DetermineDimensions(newText.Length);
        string REQUEST = $"UPDATE texts SET text_content = '{newText}', Rows = {rows}, Columns = {columns} WHERE texts_text_id = {ID}";
        return validateResponse(REQUEST);
    }

    public (string Text, int Rows, int Columns)? getSingleWithRC(int ID) {
        string REQUEST = $"SELECT text_content, Rows, Columns FROM texts WHERE texts_text_id = '{ID}'";
        var command = new SqliteCommand(REQUEST, connection);
        try {
            var reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                string text = reader["Text"].ToString();
                int rows = Convert.ToInt32(reader["Rows"]);
                int columns = Convert.ToInt32(reader["Columns"]);
                return (text, rows, columns);
            } else {
                return null;
            }
        }
        catch (Exception exp) {
            Console.WriteLine(exp.Message);
            return null;
        }
    }

    public string getSingle(int ID){
        string REQUEST = $"SELECT Text FROM Texts WHERE ID = {ID}";
        var command = new SqliteCommand(REQUEST, connection);
        try {
            var reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                return reader["Text"].ToString();
            } else {
                return null;
            }
        }
        catch (Exception exp) {
            Console.WriteLine(exp.Message);
            return null;
        }
        
    }

    public bool validateResponse(string REQUEST){
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try{
            result = command.ExecuteNonQuery();
        }
        catch(Exception exp){
            Console.WriteLine(exp.Message);
            return false;
        }

        if(1 == result) return true;
        else return false;
    }

}
public class TableDimensionCalculator
{
    public static (int rows, int columns) DetermineDimensions(int inputLength)
    {
        if (inputLength <= 0)
            throw new ArgumentException("Input length must be greater than zero.");

        int columns = (int)Math.Ceiling(Math.Sqrt(inputLength));

        int rows = (int)Math.Ceiling((double)inputLength / columns);

        return (rows, columns);
    }
}
