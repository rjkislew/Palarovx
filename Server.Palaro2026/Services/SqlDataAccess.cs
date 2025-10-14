using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Server.Palaro2026.Services
{
    public class SqlDataAccess(IConfiguration config) : ISqlDataAccess
    {
        private readonly IConfiguration _config = config;

        public string ConnectionStringName { get; set; } = "Palaro2026DB";

        #region Executions
        public async Task<List<T>> QueryAsync<T, P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60)
        {
            string constring = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection con = new SqlConnection(constring))
            {
                var data = await con.QueryAsync<T>(sql, param: param, commandType: commandType, commandTimeout: commandTimeout);
                return data.ToList();
            }
        }

        public async Task<List<T>> QueryAsync<T>(string sql, CommandType commandType = CommandType.Text, int commandTimeout = 60)
        {
            string constring = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection con = new SqlConnection(constring))
            {
                var data = await con.QueryAsync<T>(sql, commandType: commandType, commandTimeout: commandTimeout);
                return data.ToList();
            }
        }

        public async Task<T> QueryFirstAsync<T, P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60)
        {
            string constring = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection con = new SqlConnection(constring))
            {
                var data = await con.QueryFirstOrDefaultAsync<T>(sql, param: param, commandType: commandType, commandTimeout: commandTimeout);
                return data;
            }
        }

        public async Task<T> QuerySingleAsync<T, P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60)
        {
            string constring = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection con = new SqlConnection(constring))
            {
                var data = await con.QuerySingleOrDefaultAsync<T>(sql, param: param, commandType: commandType, commandTimeout: commandTimeout);
                return data;
            }
        }

        public async Task ExecuteAsync<P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60)
        {
            string constring = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection con = new SqlConnection(constring))
            {
                await con.ExecuteAsync(sql: sql, param: param, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        public async Task<T> ExecuteScalarAsync<T, P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60)
        {
            string constring = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection con = new SqlConnection(constring))
            {
                return await con.ExecuteScalarAsync<T>(sql: sql, param: param, commandType: commandType, commandTimeout: commandTimeout);
            }
        }
        #endregion
    }

    public interface ISqlDataAccess
    {
        string ConnectionStringName { get; set; }

        Task ExecuteAsync<P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60);
        Task<T> ExecuteScalarAsync<T, P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60);
        Task<List<T>> QueryAsync<T, P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60);
        Task<List<T>> QueryAsync<T>(string sql, CommandType commandType = CommandType.Text, int commandTimeout = 60);
        Task<T> QueryFirstAsync<T, P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60);
        Task<T> QuerySingleAsync<T, P>(string sql, P param, CommandType commandType = CommandType.Text, int commandTimeout = 60);
    }
}
