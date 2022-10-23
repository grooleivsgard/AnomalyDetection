/**
 * Src for connecting to DB: https://www.guru99.com/c-sharp-access-database.html
 */

using System.Data;
using Npgsql;

namespace IntrusionDetectionSystem.Statistics;

class DBquery

{
    static void Main (string[] args)
    {
        ReadData();
        
        //Wait for key press to end process
        //Console.ReadKey();
    }

    private static void ReadData()
    {
        using (NpgsqlConnection con = GetConnection())
        {   
            /* ---- RETRIEVE DATA ---- */ 
            
            //Open connection
            con.Open();
            
            // Print success connection message
            if (con.State == ConnectionState.Open)
            {
                Console.WriteLine("Connected");
            }

            //Make variable of type NpgsqlCommand
            NpgsqlCommand command;
            
            //DataReader retrieves data specified in the SQL query
            NpgsqlDataReader dataReader;

            //Define two string variables, "sql" holds command string and "Output" which contains table values. 
            String sql, Output = "";

            //Define SQL query
            sql = "SELECT * FROM endpoint_connection;";
    
            //Command statement used to execute SQL statement against the database, passing connection object (con) and string (sql). 
            command = new NpgsqlCommand(sql, con);
    
            //Execute the dataReader which fetches all the rows in the table
            dataReader = command.ExecuteReader();

            //Access the rows one by one with a while loop
            while (dataReader.Read())
            {   
                //Use GetValue method to get the values of each column for evert row
                Output = Output + dataReader.GetValue(0) + " - " + dataReader.GetValue(1) + " - " + dataReader.GetValue(2) +"\n";
            }
            
            //Display output
            Console.WriteLine(Output);
            
            //Close all objects - good practice
            dataReader.Close();
            command.Dispose();
            con.Close();
            /*
             if (con.State == ConnectionState.Closed && dataReader.IsClosed)
            {
                Console.WriteLine("Disconnected");
            }
            */
        }
    }
    
    private static void InsertData()
    {
        using (NpgsqlConnection con = GetConnection())
        {   
            /* ---- INSERT DATA ---- */ 
            
            //Open connection
            con.Open();
            
            // Print success connection message
            if (con.State == ConnectionState.Open)
            {
                Console.WriteLine("Connected");
            }

            //Make variable of type NpgsqlCommand
            NpgsqlCommand command;
            
            //DataAdaper is used to perform insert, delete and update commands
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();

            //Define string variable "sql" to hold command string. 
            String sql = "";

            //Define SQL query
            sql = "INSERT INTO endpoint_connection (bytes_in) VALUES (3)";
           
            //Command statement used to execute SQL statement against the database, passing connection object (con) and string (sql). 
            command = new NpgsqlCommand(sql, con);

            //Associate data adapter to the SQL command.
            adapter.InsertCommand = new NpgsqlCommand(sql, con);
            //Issue ExecuteNonQuery used to issue DML statements (insert, delete, update) against database.
            adapter.InsertCommand.ExecuteNonQuery();
           
            
            //Close all objects - good practice
            command.Dispose();
            con.Close();
            /*
            if (con.State == ConnectionState.Closed)
            {
                Console.WriteLine("Disconnected");
            }
            */
        }
    }
    
    private static void UpdateData()
    {
        using (NpgsqlConnection con = GetConnection())
        {   
            /* ---- INSERT DATA ---- */ 
            
            //Open connection
            con.Open();
            
            // Print success connection message
            if (con.State == ConnectionState.Open)
            {
                Console.WriteLine("Connected");
            }

            //Make variable of type NpgsqlCommand
            NpgsqlCommand command;
            
            //DataAdaper is used to perform insert, delete and update commands
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();

            //Define string variable "sql" to hold command string. 
            String sql = "";

            //Define SQL query
            sql = "UPDATE endpoint_connection set bytes_out=4 where bytes_in=3";

            //Command statement used to execute SQL statement against the database, passing connection object (con) and string (sql). 
            command = new NpgsqlCommand(sql, con);

            //Associate data adapter to the SQL command.
            adapter.UpdateCommand = new NpgsqlCommand(sql, con);
            //Issue ExecuteNonQuery used to issue DML statements (insert, delete, update) against database.
            adapter.UpdateCommand.ExecuteNonQuery();
           
            
            //Close all objects - good practice
            command.Dispose();
            con.Close();
            /*
            if (con.State == ConnectionState.Closed)
            {
                Console.WriteLine("Disconnected");
            }
            */
        }
    }
    
    private static void DeleteData()
    {
        using (NpgsqlConnection con = GetConnection())
        {   
            /* ---- INSERT DATA ---- */ 
            
            //Open connection
            con.Open();
            
            // Print success connection message
            if (con.State == ConnectionState.Open)
            {
                Console.WriteLine("Connected");
            }

            //Make variable of type NpgsqlCommand
            NpgsqlCommand command;
            
            //DataAdaper is used to perform insert, delete and update commands
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();

            //Define string variable "sql" to hold command string. 
            String sql = "";

            //Define SQL query
            sql = "DELETE FROM endpoint_connection WHERE bytes_in=3";


            //Command statement used to execute SQL statement against the database, passing connection object (con) and string (sql). 
            command = new NpgsqlCommand(sql, con);

            //Associate data adapter to the SQL command.
            adapter.DeleteCommand = new NpgsqlCommand(sql, con);
            //Issue ExecuteNonQuery used to issue DML statements (insert, delete, update) against database.
            adapter.DeleteCommand.ExecuteNonQuery();
           
            
            //Close all objects - good practice
            command.Dispose();
            con.Close();
            /*
            if (con.State == ConnectionState.Closed)
            {
                Console.WriteLine("Disconnected");
            }
            */
        }
    }

    private static NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(@"Server=localhost;Port=5432;User Id=postgres;Password=87654321;Database=TimeseriesDB");
    }
}