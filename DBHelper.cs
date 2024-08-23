using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public static class DBHelper
{

    public const string DB_FILENAME = "Database.mdf";
    public const string DB_USER_TABLE = "RequestsTbl";
    //public const string DB_ADMIN_TABLE = "citiesTBL";

    public static SqlConnection ConnectToDb(string fileName)
    {
        string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString() ?? "~/" + "/App_Data/";
        string path = dataDirectory +  fileName;
        if (string.IsNullOrEmpty(dataDirectory))
        {
            throw new InvalidOperationException("The DataDirectory is not set.");
        }
        //string connString = @"Data Source = (localdb)\ProjectsV13;Initial Catalog=CactiDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        //string connString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = D:\Sam\Teaching\Internet\Code\CactiMaster\CactiMaster\App_Data\CactiDB.mdf; Integrated Security = True";
        string connString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\גלעד\Desktop\Codes\WebDev\GoFish\GoFish\App_Data\TestDB.mdf;Integrated Security=True";
        // ללחוץ על יוזרס-די-בי.אמ-די-אף ללכת ל'פרופרטיז' ואז להעתיק 
        // את 'קונקשן סטרינג' לפה כל פעם שאני עובד על זה ממחשב אחר


        //string connString = @"Data Source=.\SQLEXPRESS;AttachDbFileName=" + path + ";Integrated Security=True;User Instance=True";
        //string connString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = |DataDirectory|\" + fileName + " Integrated Security = True";
        //string connString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = " + path + " Integrated Security = True";

        //string s =          @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = D:\Sam\Teaching\Internet\Code\CactiMaster\CactiMaster\App_Data\CactiDB.mdf; Integrated Security = True";

        //string connString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + path + ";Integrated Security=True;Connect Timeout=30";

        SqlConnection conn = new SqlConnection(connString);
        return conn;
    }

    // Used for UPDATE, DELETE - ExecuteNonQuery

    public static int DoQuery(string fileName, string sql)
    {
        SqlConnection connection = ConnectToDb(fileName);
        connection.Open();
        SqlCommand com = new SqlCommand(sql, connection);

        int numRowsAffected = com.ExecuteNonQuery();
        connection.Close();
        connection.Dispose();
        return numRowsAffected;
    }





    // Parametrized ExecuteNonQuery - Update first name, last name by Id
    // string sql: parameter to function not used in this version!
    // TBD:  Change to use Dictionary and more generic UPDATE and add WHERE parameter to function
    public static void UpdateUserNameById(string fileName, string sql, params string[] parameters)
    {

        using (SqlConnection connection = ConnectToDb(fileName))
        {
            connection.Open();

            string updateSql = "UPDATE UsersTbl SET fName = @fName, lName = @lName WHERE uName = @uName";
            using (SqlCommand command = new SqlCommand(updateSql, connection))
            {
                command.Parameters.AddWithValue("@fName", parameters[0]);
                command.Parameters.AddWithValue("@lName", parameters[1]);
                command.Parameters.AddWithValue("@uName", parameters[2]);
                command.ExecuteNonQuery();
            }

            // USING code-block automatically closes the connection
            //connection.Close();
            //connection.Dispose();
        }
    }

    // IsExists() ==> Changed name to Find()
    public static bool Find(string fileName, string sql)
    {
        SqlConnection conn = ConnectToDb(fileName);
        conn.Open();
        SqlCommand com = new SqlCommand(sql, conn);
        SqlDataReader data = com.ExecuteReader();

        bool found = Convert.ToBoolean(data.Read());
        conn.Close();
        conn.Dispose();
        return found;
    }

    // Normal Query: Returns a table with rows as per sql query
    public static DataTable ExecuteDataTable(string fileName, string sql)
    {
        SqlConnection conn = ConnectToDb(fileName);
        conn.Open();

        DataTable dataTable = new DataTable();

        SqlDataAdapter tableAdapter = new SqlDataAdapter(sql, conn);

        tableAdapter.Fill(dataTable);

        conn.Close();
        conn.Dispose();

        return dataTable;
    }

    // https://visualstudiomagazine.com/articles/2017/07/01/parameterized-queries.aspx
    // https://www.c-sharpcorner.com/UploadFile/a20beb/why-should-always-use-the-parameterized-query-to-avoid-sql-i/
    //    using (SqlConnection connection = new SqlConnection(connectionString))
    //{
    //    connection.Open();

    //    string selectSql = "SELECT * FROM users WHERE id = @id";
    //    using (SqlCommand command = new SqlCommand(selectSql, connection))
    //    {
    //        command.Parameters.AddWithValue("@id", 1);
    //        using (SqlDataReader reader = command.ExecuteReader())
    //        {
    //            while (reader.Read())
    //            {
    //                Console.WriteLine("ID: {0}, Name: {1} {2}", reader["id"], reader["fname"], reader["lname"]);
    //            }
    //        }
    //    }
    //}

    // Advanced
    // using DAPPER
    //    using (SqlConnection connection = new SqlConnection(connectionString))
    //{
    //    string selectSql = "SELECT * FROM users WHERE id = @id";
    //    IEnumerable<User> users = connection.Query<User>(selectSql, new { id = 1 });
    //    foreach (User user in users)
    //    {
    //        Console.WriteLine("ID: {0}, Name: {1} {2}", user.Id, user.FirstName, user.LastName);
    //    }
    //}

    // Parametrized Login Query
    public static DataTable Login(string fileName, string sql, params string[] parameters)
    {
        SqlConnection conn = ConnectToDb(fileName);
        conn.Open();

        DataTable dataTable = new DataTable();

        //string constr = System.Configuration.ConfigurationManager.ConnectionStrings["Constr"].ConnectionString;
        //SqlConnection con = new SqlConnection(constr);

        // use sql with parameters
        string safeSQL = $"SELECT * FROM {DBHelper.DB_USER_TABLE} WHERE uname=@uname and password=@password";

        //string safeSQL = $"SELECT COUNT(uName) FROM {DBHelper.DB_USER_TABLE} WHERE uName=@uName AND password=@password";

        SqlCommand cmd = new SqlCommand(safeSQL, conn);

        SqlParameter[] param = new SqlParameter[2];
        param[0] = new SqlParameter("@uName", parameters[0]);
        param[1] = new SqlParameter("@password", parameters[1]);
        cmd.Parameters.Add(param[0]);
        cmd.Parameters.Add(param[1]);


        SqlDataAdapter tableAdapter = new SqlDataAdapter();
        tableAdapter.SelectCommand = cmd;
        tableAdapter.Fill(dataTable);


        // OR THIS ???
        // object res = cmd.ExecuteReader(); // .ExecuteScalar();
        // loop the reader to fill the table
        //  TBD

        conn.Close();
        conn.Dispose();
        return dataTable;
    }

    //    //if (Convert.ToInt32(res) > 0) Response.Redirect("Home.aspx");
    //    //else
    //    //{
    //    //    Response.Write("Invalid Credentials");
    //    //    return;
    //    //}
    //}


    // Stored Procedure
    // https://social.technet.microsoft.com/wiki/contents/articles/53361.sql-server-stored-procedures-for-c-windows-forms.aspx
    public static DataTable Login(string fileName, params string[] parameters)
    {
        DataTable dataTable = new DataTable();
        using (SqlConnection connection = ConnectToDb(fileName)) // new SqlConnection(connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.Login";

                command.Parameters.AddWithValue("@uName", parameters[0]);
                command.Parameters.AddWithValue("@password", parameters[1]);

                // 3 types of EXECUTE: command ExeceuteNonQuery, ExecuteReader, ExecuteScalar
                // or user DataAdapter to return a table for SELECT
                SqlDataAdapter tableAdapter = new SqlDataAdapter(command);

                tableAdapter.Fill(dataTable);
            }
        }
        return dataTable;
    }
}