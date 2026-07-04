using Dapper;
using ShopEase.WebApp.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ShopEase.WebApp.Repositories
{
    #region Interface
    public interface IOtpRepository
    {
        Task<bool> SaveOtpAsync(int userId, string otp, int expiryMinutes = 10);
        Task<bool> VerifyOtpAsync(int userId, string otp);
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
        public async Task<bool> SaveOtpAsync(int userId, string otp, int expiryMinutes = 10)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@Otp", otp);
                    parameters.Add("@ExpiryTime", DateTime.UtcNow.AddMinutes(expiryMinutes));
                    parameters.Add("@IsVerified", false);
                    parameters.Add("@Attempts", 0);

                    connection.Open();
                    var result = await connection.ExecuteAsync(
                        "[dbo].[sp_Otp_Insert]",
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

        #region VerifyOtpAsync
        public async Task<bool> VerifyOtpAsync(int userId, string otp)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@Otp", otp);
                    parameters.Add("@IsVerified", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    connection.Open();
                    await connection.ExecuteAsync(
                        "[dbo].[sp_Otp_Verify]",
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
                        "[dbo].[sp_Otp_Resend]",
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
                        "[dbo].[sp_Otp_Invalidate]",
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