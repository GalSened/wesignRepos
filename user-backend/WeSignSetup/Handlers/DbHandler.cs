using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using WeSignSetup.Models;

namespace WeSignSetup.Handlers
{
    public class DbHandler
    {
        private readonly LogHandler _logHandler;
        private readonly ConnectionStringData _connectionStringData;

        public DbHandler(LogHandler logHandler, ConnectionStringData connectionStringData)
        {
            _logHandler = logHandler;
            _connectionStringData = connectionStringData;
        }

        public void CreateWeSignDB(string dbName)
        {
            string conn = _connectionStringData.ConnectionStringWithDbUser.Replace($"Initial Catalog={dbName}", "Database=master");
            if (IsDatabaseExists(conn, dbName))
            {
                _logHandler.Debug($"CreateWeSignDB | [{dbName}] DB already exists");
            }

            _logHandler.Debug($"CreateWeSignDB | connectionString = {conn}");
            SqlConnection sqlConnection = new SqlConnection(conn);
            SqlCommand sqlCommand = new SqlCommand($"CREATE DATABASE {dbName}", sqlConnection);
            try
            {
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                _logHandler.Debug($"CreateWeSignDB | Successfully create [{dbName}] DB ");
            }
            catch (Exception ex)
            {
                _logHandler.Error("CreateWeSignDB | There is problem while create Db", ex);
                throw ex;
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
            }
        }

        public void CreateTables(string sqlFilePath)
        {
            try
            {
                _logHandler.Debug($"CreateTables | connectionString = {_connectionStringData.ConnectionStringWithDbUser}, from script file [{sqlFilePath}]");
                using (SqlConnection conn = new SqlConnection(_connectionStringData.ConnectionStringWithDbUser))
                {
                    if (!File.Exists(sqlFilePath))
                    {
                        _logHandler.Warn($"CreateTables | SQL File script [{sqlFilePath}] not exist");
                        throw new InvalidOperationException($"Script file [{sqlFilePath}] not exist");
                    }
                    string scriptText = File.ReadAllText(sqlFilePath);

                    ServerConnection svrConnection = new ServerConnection(conn);
                    Server server = new Server(svrConnection);
                    server.ConnectionContext.ExecuteNonQuery(scriptText);
                    _logHandler.Debug("CreateTables | Successfully create DB tables");
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error("CreateTables | There is problem while create tables", ex);
                throw ex;
            }
        }

        public void DeleteAllTables(string sqlFilePath)
        {
            try
            {
                _logHandler.Debug($"CreateTables | connectionString = {_connectionStringData.ConnectionStringWithDbUser}, from script file [{sqlFilePath}]");
                using (SqlConnection conn = new SqlConnection(_connectionStringData.ConnectionStringWithDbUser))
                {
                    if (!File.Exists(sqlFilePath))
                    {
                        _logHandler.Warn($"CreateTables | SQL File script [{sqlFilePath}] not exist");
                        throw new InvalidOperationException($"Script file [{sqlFilePath}] not exist");
                    }
                    string scriptText = File.ReadAllText(sqlFilePath);

                    ServerConnection svrConnection = new ServerConnection(conn);
                    Server server = new Server(svrConnection);
                    server.ConnectionContext.ExecuteNonQuery(scriptText);
                    _logHandler.Debug("CreateTables | Successfully create DB tables");
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error("CreateTables | There is problem while create tables", ex);
                throw ex;
            }
        }


        public void UpdateOtpConfiguration(bool? isChecked)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionStringData.ConnectionStringWithDbUser))
                {
                    string scriptText = $"UPDATE Configuration SET Value = '{isChecked}' WHERE [Key] = 'UseManagementOtpAuth'";
                    _logHandler.Debug(scriptText);
                    ServerConnection svrConnection = new ServerConnection(conn);
                    Server server = new Server(svrConnection);
                    server.ConnectionContext.ExecuteNonQuery(scriptText);
                    _logHandler.Debug("UpdateOtpConfiguration| Successfully update use OTP for management site configuration");
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error("UpdateOtpConfiguration | There is problem while update DB", ex);
                throw ex;
            }

        }
        
        private bool IsDatabaseExists(string connectionString, string databaseName)
        {
            bool result = false;
            try
            {                
                using (var tmpConn = new SqlConnection(connectionString))
                {
                    string sqlCreateDBQuery = $"SELECT database_id FROM sys.databases WHERE Name = '{databaseName}'";
                    using (SqlCommand sqlCmd = new SqlCommand(sqlCreateDBQuery, tmpConn))
                    {
                        tmpConn.Open();
                        object resultObj = sqlCmd.ExecuteScalar();
                        int databaseID = 0;
                        if (resultObj != null)
                        {
                            int.TryParse(resultObj.ToString(), out databaseID);
                        }
                        tmpConn.Close();
                        result = (databaseID > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error($"Error - IsDatabaseExists - database name [{databaseName}]", ex);                
            }

            return result;
        }

    }
}
