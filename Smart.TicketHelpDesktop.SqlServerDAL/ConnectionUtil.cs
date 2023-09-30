namespace Smart.TicketkHelpDesktop.SqlServerDAL
{
    public class ConnectionUtil
    {

        /// <summary>
        /// Test DB connection string
        /// </summary>
        internal static string ConnectionStringTest { get; private set; }

        /// <summary>
        /// setup method to set the connections string
        ///
        /// </summary>
        /// <param name="connectionStringTest"></param>
        public static void Setup(string connectionStringTest)
        {
            if (string.IsNullOrEmpty(connectionStringTest)) { throw new Exception("connection string for unspecified test database"); }
            else { ConnectionStringTest = connectionStringTest; }
        }
    }
}
