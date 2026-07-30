using Dapper;
using ShopEase.WebApp.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ShopEase.WebApp.Repositories
{
    #region Interface
    public interface IOtpRepository
    {
        Task<bool> SaveOtpAsync(string email,int userId, string otp, int expiryMinutes = 10);
        Task<bool> VerifyOtpAsync(int userId,string email, string otp);
        Task<bool> ResendOtpAsync(int userId, string newOtp, int expiryMinutes = 10);
        Task<bool> InvalidateOtpAsync(int userId);
    }
    #endregion

    public class OtpRepository : IOtpRepository
    {
        #region Properties
        private readonly AppSettings _appSettings;
        #endregion

        #region Constructor
        public OtpRepository(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        #endregion

        #region CreateConnection
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_appSettings.ConnectionStrings.ShopEaseConnection);
        }
        #endregion

        #region SaveOtpAsync
        public async Task<bool> SaveOtpAsync(string email, int userId, string otp, int expiryMinutes = 10)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@Email", email);
                    parameters.Add("@Otp", otp);
                    parameters.Add("@RowsInserted", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    connection.Open();
                    await connection.ExecuteAsync(
                        "[dbo].[Otp_Insert]",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    var rowsInserted = parameters.Get<int>("@RowsInserted");
                    return rowsInserted > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region VerifyOtpAsync
        public async Task<bool> VerifyOtpAsync(int userId, string email, string otp)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@Otp", otp);
                    parameters.Add("@Email", email);
                    parameters.Add("@IsVerified", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    connection.Open();
                    await connection.ExecuteAsync(
                        "[dbo].[Otp_Verify]",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    var isVerified = parameters.Get<bool>("@IsVerified");
                    return isVerified;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region ResendOtpAsync
        public async Task<bool> ResendOtpAsync(int userId, string newOtp, int expiryMinutes = 10)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@Otp", newOtp);
                    parameters.Add("@ExpiryTime", DateTime.UtcNow.AddMinutes(expiryMinutes));

                    connection.Open();
                    var result = await connection.ExecuteAsync(
                        "[dbo].[Otp_Resend]",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    return result > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region InvalidateOtpAsync
        public async Task<bool> InvalidateOtpAsync(int userId)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);

                    connection.Open();
                    var result = await connection.ExecuteAsync(
                        "[dbo].[Otp_Invalidate]",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    return result > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}