using log4net;
using Smart.SqlHelper;
using Smart.TicketHelpDesktop.Model;
using Smart.TicketkHelpDesktop.SqlServerDAL;
using System.Data;
using System.Data.SqlClient;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace Smart.TicketHelpDesktop.SqlServerDAL
{
    public static class DALUser

    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DALUser));

        #region SqlParams
        private const string PARM_id = "@Id";
        private const string PARM_name = "@Name";
        private const string PARM_last_name = "@LastName";
        private const string PARM_email = "@Email";
        private const string PARM_password = "@Password";
        private const string PARM_permission = "@Permission";
        private const string PARM_token = "@Token";

        #endregion

        #region SqlFilter

        private const string FILTER_id = "id = @Id";
        private const string FILTER_name = "name = @Name";
        private const string FILTER_last_name = "last_name = @LastName";
        private const string FILTER_email = "email = @Email";
        private const string FILTER_password = "password = @Password";
        private const string FILTER_permission = "permission = @Permission";
        private const string FILTER_token = "token = @Token";
        private const string FILTER_AND = " AND ";
        #endregion

        #region SqlSelect
        private const string SQL_SELECT = "SELECT id, name, last_name, email, password, token, permission " +
            "FROM users "
            + "WHERE 1 = 1 ";

        private const string SQL_SELECT_BY_NAME = "SELECT name FROM users WHERE id = @Id";
        private const string SQL_SELECT_USER_BY_EMAIL = "SELECT id, name, last_name, email, password, permission, token FROM users WHERE email = @Email";
        private const string SQL_UPDATE_USER_TOKEN = "UPDATE users SET token = @Token WHERE id = @Id";
        #endregion

        #region SqlInsert
        private const string SQL_INSERT_USER = "INSERT INTO users (name, last_name, email, password, permission, token) VALUES (@Name, @LastName, @Email, @Password, @Permission, @Token); SELECT SCOPE_IDENTITY()";
        #endregion

        #region SqlDelete
        private const string SQL_DELETE_USER = "DELETE users WHERE id = @Id";
        #endregion

        #region SqlUpdate
        private const string SQL_UPDATE_USER = "UPDATE users SET name = @Name, last_name = @LastName, email = @Email, password = @Password, permission = @Permission " +
          "WHERE id = @id ";
        #endregion

        public static List<User> GetAllUser()
        {
            log.Debug("START GetAll user");
            var user = new List<User>();
            String sqlString = SQL_SELECT;
            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, sqlString))
                {
                    while (rdr.Read())
                    {
                        user.Add(filRol(rdr));
                    }
                    log.Debug("End getallUser");
                    return user;
                }
            }
            catch (SqlException e)
            {

                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e); ;
            }
        }
        public static void UpdateUser(User user)
        {
            log.Debug("Start UpdateUser");
            try
            {
                updateUser(user, null);
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
            log.Debug("End UpdateUser");
        }
        internal static void updateUser(User user, SqlTransaction trans)
        {
            log.Debug("Start updateUser - internal");
            List<SqlParameter> parms = getUserParms(user);
            SqlParameter tmpParam = new SqlParameter(PARM_id, SqlDbType.Int);
            tmpParam.Value = user.Id;
            parms.Add(tmpParam);

            if (trans == null)
                SqlServerHelper.ExecuteNonQuery(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_UPDATE_USER, parms.ToArray());
            else
                SqlServerHelper.ExecuteNonQuery(trans, CommandType.Text, SQL_UPDATE_USER, parms.ToArray());
            log.Debug("End updateUser - internal");
        }
        public static string GetUserNameById(int? idUser)
        {
            log.Debug("INIZIO GetUserNameById");
            string userName = null;
            string sqlString = SQL_SELECT_BY_NAME;
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParm = new SqlParameter("@Id", SqlDbType.Int);
            tmpParm.Value = idUser;
            parms.Add(tmpParm);

            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, sqlString, parms.ToArray()))
                {
                    if (rdr.Read())
                    {
                        userName = rdr["Name"].ToString();
                    }
                }
                log.Debug("FINE GetUserNameById");
                return userName;
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
        }
        public static User SelectById(int? Id)
        {
            log.Debug("Start Select By Id");
            User result = null;
            String sqlString = SQL_SELECT;
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParm = null;
            tmpParm = new SqlParameter(PARM_id, SqlDbType.Int);
            tmpParm.Value = Id;
            parms.Add(tmpParm);
            sqlString += FILTER_AND + FILTER_id;

            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, sqlString, parms.ToArray()))
                {
                    if (rdr.Read())
                    {
                        result = fillData(rdr);
                    }
                }
                log.Debug("End Select By Id");
                return result;
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e); ;
            }
        }
        public static User Login(string email, string password)
        {

            var user = GetUserByEmail(email);
            if (user == null)
            {
                throw new InvalidCredentialException("Wrong email or password");
            }


            var passwordHash = HashPassword(password);
            if (user.Password != passwordHash)
            {
                throw new InvalidCredentialException("Wrong email or password");
            }

            return user;
        }
        public static User GetUserByEmail(string? Email)
        {
            User user = null;
            var parms = new List<SqlParameter>()
            {
                  new SqlParameter("@Email", SqlDbType.NVarChar) { Value = Email }
            };

            try
            {
                var reader = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_SELECT_USER_BY_EMAIL, parms.ToArray());
                if (reader.Read())
                {
                    user = fillData(reader);
                }
                reader.Close();
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }



            return user;
        }
        public static void UpdateUserToken(int? Id, string? token)
        {
            var parms = new List<SqlParameter>()
            {
                  new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                  new SqlParameter("@Token", SqlDbType.NVarChar, -1) { Value = token }
            };

            try
            {
                SqlServerHelper.ExecuteNonQuery(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_UPDATE_USER_TOKEN, parms.ToArray());
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
        }
        public static void RegisterUser(User user)
        {
            var parms = getUserParms(user);

            try
            {
                SqlServerHelper.ExecuteNonQuery(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_INSERT_USER, parms.ToArray());
                log.Debug("End RegisterUser");
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
        }
        public static string HashPassword(string password)
        {

            var hasher = new SHA256Managed();
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            var hashBytes = hasher.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hashBytes);
        }
        public static List<User> GetListUserByRol(string? Permission)
        {
            log.Debug("INIZIO GetListUserByRol");
            var user = new List<User>();
            String sqlString = SQL_SELECT;
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParm = null;
            tmpParm = new SqlParameter(PARM_permission, SqlDbType.NVarChar);
            tmpParm.Value = Permission;
            parms.Add(tmpParm);
            sqlString += FILTER_AND + FILTER_permission;

            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, sqlString, parms.ToArray()))
                {
                    while (rdr.Read())
                    {
                        user.Add(filRol(rdr));

                    }

                    log.Debug("END GetListUserByRol");
                    return user;
                }
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e); ;
            }
        }
        private static List<SqlParameter> getUserParms(User user)
        {
            log.Debug("Inizio get Parms");
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParam;

            tmpParam = new SqlParameter(PARM_name, SqlDbType.NVarChar);
            tmpParam.Value = user.Name;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_last_name, SqlDbType.NVarChar);
            tmpParam.Value = user.LastName;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_email, SqlDbType.NVarChar);
            tmpParam.Value = user.Email;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_password, SqlDbType.NVarChar, 50);
            tmpParam.Value = user.Password;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_permission, SqlDbType.NVarChar);
            tmpParam.Value = user.Permission;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_token, SqlDbType.NVarChar);
            tmpParam.Value = user.Token;
            parms.Add(tmpParam);

            log.Debug("Fine get Parms");
            return parms;
        }
        internal static User fillData(SqlDataReader rdr)
        {
            User user = new User();
            if (rdr["id"] != DBNull.Value) user.Id = (int)rdr["id"];
            if (rdr["name"] != DBNull.Value) user.Name = (string)rdr["name"];
            if (rdr["last_name"] != DBNull.Value) user.LastName = (string)rdr["last_name"];
            if (rdr["email"] != DBNull.Value) user.Email = (string)rdr["email"];
            if (rdr["password"] != DBNull.Value) user.Password = (string)rdr["password"];
            if (rdr["permission"] != DBNull.Value) user.Permission = (string)rdr["permission"];
            if (rdr["token"] != DBNull.Value) user.Token = (string)rdr["token"];
            return user;
        }
        internal static User filRol(SqlDataReader rdr)
        {
            User user = new User();
            if (rdr["id"] != DBNull.Value) user.Id = (int)rdr["id"];
            if (rdr["name"] != DBNull.Value) user.Name = (string)rdr["name"];
            if (rdr["last_name"] != DBNull.Value) user.LastName = (string)rdr["last_name"];
            if (rdr["email"] != DBNull.Value) user.Email = (string)rdr["email"];
            if (rdr["password"] != DBNull.Value) user.Password = (string)rdr["password"];
            if (rdr["permission"] != DBNull.Value) user.Permission = (string)rdr["permission"];
            return user;
        }
    }
}