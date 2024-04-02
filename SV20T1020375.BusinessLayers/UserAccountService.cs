using SV20T1020375.DataLayers;
using SV20T1020375.DataLayers.SQLServer;
using SV20T1020375.DomainModels;
namespace SV20T1020375.BusinessLayers
{
    public static class UserAccountService
    {
        private static readonly IUserAccountDAL employeeAccountDB;
        static UserAccountService()
        {
            string connectionString = Configuration.ConnectionString;
            employeeAccountDB = new EmployeeAccountDAL(connectionString);
        }
        public static UserAccount? Authorize(string userName, string password)
        {
            return employeeAccountDB.Authorize(userName,password);
        }
        public static bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return employeeAccountDB.ChangePassword(userName,oldPassword,newPassword);
        }
        public static string getPasswordByUserName(string username)
        {
            return employeeAccountDB.getPasswordByUserName(username);
        }
    }
}
