
using Microsoft.Extensions.Configuration;

namespace Smart.TicketHelpDesktop.BLL
{


    public static class Factory
    {
        private static IConfiguration _configuration;
        public static UserService UserService { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
            UserService = new UserService(_configuration);
        }
        public static TicketService TicketService { get; } = new TicketService();



        /// <param name="ConnectionStringTestDb">connection string for test db</param>

        public static void Setup(string? ConnectionStringTestDb)

        {
            if (ConnectionStringTestDb == null)
            {
                return;
            }
            Smart.TicketkHelpDesktop.SqlServerDAL.ConnectionUtil.Setup(ConnectionStringTestDb);
        }


    }
}
