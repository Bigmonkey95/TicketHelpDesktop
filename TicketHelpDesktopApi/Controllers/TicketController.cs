using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart.TicketHelpDesktop.BLL;
using Smart.TicketHelpDesktop.Model;
using System.Threading.Channels;

namespace TicketHelpDesktopApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketController : ControllerBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TicketController));





        /// <summary>
        /// Retrieve a List Ticket with the specified id param from the database
        /// </summary>
        /// <param name="id">Tickets identifier</param>
        /// <response code="200">Tickets retrieved</response>
        /// <response code="204">there is no Ticket in the database with the specified id param</response>
        /// <response code="500">in case of an error with the request</response>
        /// <response code="401">if not authenticated on identity</response>
        [Route("Filter")]
        [ProducesResponseType(typeof(Ticket), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet]
        [Authorize]
        public IActionResult Filter(string? Subject, string? Text, DateTime? TicketCreationDatetimeStart, DateTime? TicketCreationDatetimeEnd, string? Applicant, string? Priority, string? AffectedApplication, string? UserCreation, string? Status, int? IdUser)
        {
            List<Ticket> result = new List<Ticket>();

            log.Info("START GetListData");
            try
            {
                result = Factory.TicketService.Filter(Subject, Text, TicketCreationDatetimeStart, TicketCreationDatetimeEnd, Applicant, Priority, AffectedApplication, UserCreation, Status, IdUser);
                if (result == null)
                {
                    log.Warn("No Content");
                    return NoContent();
                }
                log.Info("END GetListData");
                return Ok(result);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        /// <summary>
        /// Retrieve a List Ticket with the specified filtered param from the database and export to Csv
        /// </summary>
        /// <param name="FilteredTicket">Tickets identifier to export</param>
        /// <response code="200">Tickets Exported</response>
        /// <response code="204">there is no Ticket in the database with the specified id param</response>
        /// <response code="500">in case of an error with the request</response>
        /// <response code="401">if not authenticated on identity</response>
        [Route("ExportToCSV")]
        [HttpPost]
        [Authorize]
        public IActionResult ExportToCSV(List<Ticket> FilteredTicket)
        {
            var saveLocation = "C:/Exports/SaveTickets.csv";

            var success = TicketService.ExportTicketsToCSV(FilteredTicket, saveLocation);

            if (success)
            {

                byte[] fileBytes = System.IO.File.ReadAllBytes(saveLocation);
                return File(fileBytes, "text/csv", "SaveTickets.csv");
            }
            else
            {
                return StatusCode(500, "Internal Server Error Cannot Export A Ticket to CSV.");
            }
        }




        /// <summary>
        /// Get All Ticket in the database
        /// </summary>
        /// <param name="GetAllTicket">the Ticket to Get</param>
        /// <response code="200">Ticket List o Get</response>
        /// <response code="204">Ticket List to Get not found</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">if not authenticated on identity</response>
        /// <response code="500">in case of an error with the request</response>
        [Route("GetAllTicket")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult GetAllTicket()
        {

            List<Ticket> result = new List<Ticket>();
            log.Info("START GET All Tickets");
            try
            {
                result = Factory.TicketService.GetAllTicket();
                if (result == null)
                {
                    log.Warn("No Content");
                    return NoContent();
                }
                log.Info("En Get All Tickets");
                return Ok(result);
            }
            catch (Exception ex)
            {

                log.Error(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex}");

            }
        }

        /// <summary>
        /// Get List a Ticket  in the database by IdUser
        /// </summary>
        /// <param name="IdUser">the List Ticket to Get</param>
        /// <response code="200">Ticket List o Get</response>
        /// <response code="204">Ticket List to Get not found</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">if not authenticated on identity</response>
        /// <response code="500">in case of an error with the request</response>
        [Route("GetLisTickets")]
        [ProducesResponseType(typeof(Ticket), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet]
        [Authorize]
        public IActionResult GetListTickets(int? IdUser)
        {

            List<Ticket> result = new List<Ticket>();

            log.Info("START GET Tickets");
            try
            {
                result = Factory.TicketService.GetListTicket(IdUser);
                if (result == null)
                {
                    log.Warn("No Content");
                    return NoContent();
                }
                log.Info("En Get Tickets");
                return Ok(result);
            }
            catch (Exception ex)
            {

                log.Error(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex}");

            }


        }


        /// <summary>
        /// Update a Ticket in the database by Id with the specified operation type (updateNotes or closeTicket).
        /// </summary>
        /// <param name="ticketOperations">The Ticket to update.</param>
        /// <param name="operationType">The type of operation to perform ("updateNotes" or "closeTicket").</param>
        /// <response code="200">Ticket successfully updated.</response>
        /// <response code="204">Ticket to update not found.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized: User does not have permission to update this ticket.</response>
        /// <response code="500">Internal server error.</response>
        [Route("UpdateTicket")]
        [ProducesResponseType(typeof(Ticket), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPut]
        [Authorize]
        public IActionResult UpdateTicket(TicketOperations ticketOperations, string operationType)
        {
            log.Info("START UpdateTicket");

            if (string.IsNullOrWhiteSpace(operationType))
            {
                log.Error("Not all parameters specified");
                return BadRequest("Not all parameters specified");
            }

            try
            {
                string userIdFromToken = Factory.UserService.GetUserIdFromToken(HttpContext);
                int.TryParse(userIdFromToken, out int assignedUser);
                ticketOperations.IdUser = assignedUser;

                Ticket existingTicket = Factory.TicketService.GetById(ticketOperations.Id);


                if (existingTicket == null)
                {
                    log.Error("Ticket not found.");
                    return NotFound("Ticket not found.");
                }

                if (existingTicket.AssignedUser != assignedUser)
                {
                    log.Error("Unauthorized: User does not have permission to update or close this ticket.");
                    return Unauthorized("User does not have permission to update or close this ticket.");
                }

                Factory.TicketService.UpdateTicket(ticketOperations, operationType);

                log.Info("END UpdateTicket");
                return Ok(ticketOperations);
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
        /// Save an Ticket in the database
        /// </summary>
        /// <param name="ticket">the Ticket to save</param>
        /// <response code="200">the Ticket saved</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">if not authenticated on identity</response>
        /// <response code="500">in case of an error with the request</response>
        [Route("SaveTicket")]
        [ProducesResponseType(typeof(Channel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPost]
        [Authorize]
        public IActionResult SaveTicket(TicketOperations ticketOperations)
        {
            log.Info("START SaveClientOrder");

            if (ticketOperations == null)
            {
                log.Error("Not all parameter specified");
                return BadRequest("Not all parameter specified");
            }
            try
            {
                string userIdFromToken = Factory.UserService.GetUserIdFromToken(HttpContext);
                int.TryParse(userIdFromToken, out int userId);


                ticketOperations.IdUser = userId;


                Factory.TicketService.SaveTicket(ticketOperations);

                log.Info("END SaveTicket");
                return Ok(ticketOperations);
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
        /// Get Ticket And Details from the database
        /// </summary>
        /// <param name="Id">the Ticket and Details to Get</param>
        /// <response code="200">the Ticket and Details Get</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">if not authenticated on identity</response>
        /// <response code="500">in case of an error with the request</response>
        [Route("GetTicketDetails")]
        [ProducesResponseType(typeof(Channel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpGet]
        [Authorize]
        public IActionResult GetTicketDetails(int? Id)
        {
            log.Info("START GetTicketDetails");

            List<Ticket> result = new List<Ticket>();

            log.Info("START GET GetTicketDetails");
            try
            {
                result = Factory.TicketService.GetListTicketDetails(Id);
                if (result == null)
                {
                    log.Warn("No Content");
                    return NoContent();
                }
                log.Info("En Get GetTicketDetails");
                return Ok(result);
            }
            catch (Exception ex)
            {

                log.Error(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex}");

            }


        }


        /// <summary>
        /// Assign a Ticket to a User.
        /// </summary>
        /// <param name="ticketOperations">The ID of the Ticket to be assigned.</param>
        /// <param name="IdUser">The ID of the User to whom the Ticket will be assigned.</param>
        /// <response code="200">If the Ticket was successfully assigned to the User.</response>
        /// <response code="204">If the Ticket or User was not found.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="500">If there is an error with the request.</response>
        [Route("AssignTicket")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [HttpPut]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult AssignTicket(TicketOperations ticketOperations, int IdUser)
        {
            log.Info("START AssignTicket");

            try
            {
                Ticket ticket = Factory.TicketService.GetById(ticketOperations.Id);
                User user = Factory.UserService.GetById(IdUser);

                if (ticket == null || user == null)
                {
                    log.Warn("Ticket or User not found");
                    return NoContent();
                }

                if (ticket.Status != "open" || ticket.AssignedUser != null)
                {
                    log.Warn("Ticket cannot be assigned due to status or existing assignment.");
                    return BadRequest("Ticket cannot be assigned due to status or existing assignment.");
                }

             
                Factory.TicketService.AssignUser(ticketOperations, IdUser);

                log.Info($"Ticket {ticketOperations.Id} assigned to User {ticketOperations.IdUser}");
                log.Info("END AssignTicket");
                return Ok(new { message = "User Assigned" });
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }


    }


}
