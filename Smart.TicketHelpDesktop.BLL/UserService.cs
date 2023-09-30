using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Smart.TicketHelpDesktop.Model;
using Smart.TicketHelpDesktop.Model.Exceptions;
using Smart.TicketHelpDesktop.SqlServerDAL;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Smart.TicketHelpDesktop.BLL
{
    public class UserService
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(UserService));
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecretKey;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _jwtSecretKey = _configuration.GetSection("JwtSettings")["SecretKey"];


        }
        public List<User> GetAllUsers()
        {
            List<User> data = new List<User>();
            log.Debug("START GetAllUsers");
            try
            {
                data = DALUser.GetAllUser();
                return data;
            }
            catch (DALException ex)
            {
                log.Error("Cannot read users: " + ex.Message, ex);
                throw new ApplicationException("Cannot read users: " + ex.Message, ex);
            }
        }
        public User GetById(int? Id)
        {
            log.Debug("START GetById");
            User result = null;
            if (!Id.HasValue)
            {
                log.Error("Input parameter not set");
                throw new ArgumentNullException("Input parameter not set");
            }
            try
            {
                result = DALUser.SelectById(Id.Value);
            }
            catch (DALException ex)
            {
                log.Error("Cannot read object: " + ex.Message, ex);
                throw new ApplicationException("Cannot read object: " + ex.Message, ex);
            }
            log.Debug("END GetById");
            return result;
        }
        public void UpdateUser(User user)
        {
            log.Debug("START UpdateUser");
            if (user == null || !user.Id.HasValue)
            {
                log.Error("Input parameter not set or incompatible");
                throw new ArgumentNullException("Input parameter not set or incompatible");
            }
            try
            {
                DALUser.UpdateUser(user);
                log.Debug("END UpdateUser");
            }
            catch (DALException ex)
            {
                log.Error("Cannot update user: " + ex.Message, ex);
                throw new ApplicationException("Cannot update user: " + ex.Message, ex);
            }
        }
        private string GenerateToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(GetSecretKey());
            var signingKey = new SymmetricSecurityKey(key);

            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("LastName", user.LastName),
                    new Claim(ClaimTypes.Role, user.Permission),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            };



            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        private string GetSecretKey()
        {

            return _configuration["JwtSettings:SecretKey"];
        }
        public RegisterRequest RegisterUser(RegisterRequest registerRequest)
        {
            log.Debug("START RegisterUser");


            if (!string.Equals(registerRequest.Password, registerRequest.ConfirmPassword))
            {
                log.Error("Password and confirmPassword do not match.");
                throw new ArgumentException("Password and confirmPassword do not match.");
            }

            User user = new User
            {
                Name = registerRequest.Name,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                Password = DALUser.HashPassword(registerRequest.Password),
                Permission = "user"
            };
            string token = GenerateToken(user);
            user.Token = token;
            var existingUser = DALUser.GetUserByEmail(user.Email);
            if (existingUser != null)
            {
                throw new ApplicationException("Email is already in use.");
            }
            try
            {
                DALUser.RegisterUser(user);
                log.Info($"Successfully registered user: {registerRequest.Email}");
                log.Debug("END RegisterUser");
                return registerRequest;
            }
            catch (DALException ex)
            {
                log.Error("Unable to register user: " + ex.Message, ex);
                throw new ApplicationException("Unable to register user: " + ex.Message, ex);
            }
        }
        public string Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email and password must not be empty or whitespace.");
            }

            var user = DALUser.Login(email, password);

            if (user == null)
            {

                throw new ApplicationException("Authentication failed. Invalid email or password.");
            }

            var token = GenerateToken(user);

            if (string.IsNullOrWhiteSpace(token))
            {

                throw new ApplicationException("Authentication failed. Token generation error.");
            }

            DALUser.UpdateUserToken(user.Id, token);

            return token;
        }
        public string GetUserIdFromToken(HttpContext httpContext)
        {

            var user = httpContext.User;

            if (user.Identity.IsAuthenticated)
            {

                var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    return userIdClaim.Value;
                }
            }
            return null;
        }
        public List<User> GetListUserByRol(string? Rol)
        {
            List<User> user = new List<User>();
            log.Debug("START GetListUserByRol");
            if (Rol == null)
            {
                log.Error("Input parameter not set or incompatible");
                throw new ArgumentNullException("Input parameter not set or incompatible");
            }
            try
            {
                user = DALUser.GetListUserByRol(Rol);
                return user;
            }
            catch (DALException ex)
            {
                log.Error("Cannot get user list: " + ex.Message, ex);
                throw new ApplicationException("Cannot get user list: " + ex.Message, ex);
            }
        }
    }

}


