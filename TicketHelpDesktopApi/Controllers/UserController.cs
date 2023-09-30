using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart.TicketHelpDesktop.BLL;
using Smart.TicketHelpDesktop.Model;
using System.Runtime.InteropServices;

namespace TicketHelpDesktopApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UserController));
        private readonly IConfiguration _configuration;



        /// <summary>
        /// Get All  User in the database
        /// </summary>
        /// <param name="id">the User to Get</param>
        /// <response code="200">User List o Get</response>
        /// <response code="204">User List to Get not found</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">if not authenticated on identity</response>
        /// <response code="500">in case of an error with the request</response>
        [Route("GetAllUsers")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult GetAllUsers()
        {

            List<User> result = new List<User>();
            log.Info("START GET Users");
            try
            {
                result = Factory.UserService.GetAllUsers();
                if (result == null)
                {
                    log.Warn("No Content");
                    return NoContent();
                }
                log.Info("En Get All Users");
                return Ok(result);
            }
            catch (Exception ex)
            {

                log.Error(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex}");

            }
        }

        /// <summary>
        /// Retrieve a User with the specified id param from the database
        /// </summary>
        /// <param name="id">User's identifier</param>
        /// <response code="200">User retrieved</response>
        /// <response code="204">there is no User in the database with the specified id param</response>
        /// <response code="500">in case of an error with the request</response>
        /// <response code="401">if not authenticated on identity</response>
        [Route("ById")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet]
        [Authorize]
        public IActionResult ById(int Id)
        {
            log.Info("START ById");
            try
            {
                User result = Factory.UserService.GetById(Id);
                if (result == null)
                {
                    log.Warn("No Content");
                    return NoContent();
                }
                log.Info("END ById");
                return Ok(result);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }



        /// <summary>
        /// Update an User in the database
        /// </summary>
        /// <param name="user">the User to update</param>
        /// <response code="200">User Updated</response>
        /// <response code="204">User to update not found</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">if not authenticated on identity</response>
        /// <response code="500">in case of an error with the request</response>
        [Route("UpdateUser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPut]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult UpdateUser(User user)
        {
            log.Info("START UpdateUser");
            if (user == null || !user.Id.HasValue)
            {
                log.Error("Not all parameter specified");
                return BadRequest("Not all parameter specified");
            }
            try
            {
                Factory.UserService.UpdateUser(user);
                log.Info("END UpdateUser");
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                log.Error(ex.Message, ex);
                return BadRequest();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }


        /// <summary>
        /// Login With Email and Password in the database
        /// </summary>
        /// <param name="request">The Login Request </param>
        /// <response code="200">Login Correct</response>
        /// <response code="204">The User not found</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">if not authenticated on identity</response>
        /// <response code="500">in case of an error with the request</response>
        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                log.Error("Not all parameters specified.");
                return BadRequest(new { error = "Not all parameters specified." });
            }

            try
            {

                var authToken = Factory.UserService.Login(request.Email, request.Password);

                if (authToken == null)
                {
                    log.Error($"Invalid credentials for user {request.Email}");
                    return Unauthorized(new { error = "Invalid credentials" });
                }

                return Ok(new LoginResponse { Token = authToken, Status = "ok" });
            }
            catch (ArgumentException ex)
            {
                log.Error(ex.Message, ex);
                return BadRequest(new { error = "Wrong email or password" });
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return StatusCode(500, new { error = "Wrong email or password" });
            }
        }



        /// <summary>
        /// Register a user in the database
        /// </summary>
        /// <param name="registerRequest">The User Request </param>
        /// <response code="200">Register Correct</response>
        /// <response code="204">The Register Error not Found</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">if not authenticated on identity</response>
        /// <response code="500">in case of an error with the request</response>
        [HttpPost("Register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]

        public IActionResult Register([FromBody] RegisterRequest registerRequest)
        {


            if (registerRequest == null || string.IsNullOrEmpty(registerRequest.Email) || string.IsNullOrEmpty(registerRequest.Password))
            {
                log.Error("Not all registration parameters specified.");
                return BadRequest(new { error = "Not all registration parameters specified." });
            }
            try
            {

                var result = Factory.UserService.RegisterUser(registerRequest);

                if (result == null)
                {
                    log.Error($"Registration failed for user {registerRequest.Email}");
                    return StatusCode(500, new { error = "Registration failed" });
                }

                return Ok(new { message = "User registered successfully" });
            } 
            catch (ArgumentException ex)
            {
                log.Error(ex.Message, ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                log.Error(ex.Message, ex);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return StatusCode(500, new { error = $"Internal server error: {ex}" });
            }
        }
    }
}








