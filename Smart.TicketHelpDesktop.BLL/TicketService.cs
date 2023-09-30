using log4net;
using Smart.TicketHelpDesktop.Model;
using Smart.TicketHelpDesktop.Model.Exceptions;
using Smart.TicketHelpDesktop.SqlServerDAL;
using Smart.TicketkHelpDesktop.SqlServerDAL;

namespace Smart.TicketHelpDesktop.BLL
{
    public class TicketService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TicketService));
        public Ticket GetById(int? Id)
        {
            log.Debug("START Get ById");
            Ticket result = null;
            if (!Id.HasValue)
            {
                log.Error("Input Parameter not set");
                throw new ArgumentNullException("Input Parameter not set");
            }
            try
            {
                result = DALTicket.SelectById(Id.Value);
            }
            catch (DALException ex)
            {
                log.Error("Cannot read example object: " + ex.Message, ex);
                throw new ApplicationException("Cannot read example object: " + ex.Message, ex);
            }
            log.Debug("END Get ById");
            return result;
        }
        public void SaveTicket(TicketOperations ticketOperations)
        {
            log.Debug("START SaveClient");
            if (ticketOperations == null || !ticketOperations.Id.HasValue)
            {
                log.Error("Input parameter not set or mismatch");
                throw new ArgumentNullException("Input parameter not set or mismatch");
            }
            try
            {

                DALTicket.TransactionalOperationSaveTicketDetail(ticketOperations);
                log.Debug("END SaveClient");
            }
            catch (DALException ex)
            {
                log.Error("Unable to save Client: " + ex.Message, ex);
                throw new ApplicationException("Unable to save Client: " + ex.Message, ex);
            }
        }
        public List<Ticket> Filter(string? Subject, string? Text, DateTime? TicketCreationDatetimeStart, DateTime? TicketCreationDatetimeEnd, string? Applicant, string? Priority, string? AffectedApplication, string? UserCreation, string? Status, int? IdUser)
        {
            List<Ticket> data = new List<Ticket>();

            log.Debug("START GetListData");
            if ((Subject == null) && (Text == null) && (TicketCreationDatetimeStart == null) && (TicketCreationDatetimeEnd == null) && (Applicant == null) && (Priority == null) && (AffectedApplication == null) && (UserCreation == null) && (Status == null) && (IdUser == null))
            {
                log.Error("Input parameter not set or mismatch");
                throw new ArgumentNullException("Input parameter not set or mismatch");
            }
            try
            {
                data = DALTicket.Filter(Subject, Text, TicketCreationDatetimeStart, TicketCreationDatetimeEnd, Applicant, Priority, AffectedApplication, UserCreation, Status, IdUser);
                return data;
            }
            catch (DALException ex)
            {
                log.Error("Unable to get listData: " + ex.Message, ex);
                throw new ApplicationException("Unable to get listData: " + ex.Message, ex);
            }
        }


        public static bool ExportTicketsToCSV(List<Ticket> filteredTickets, string saveLocation)
        {
            if (filteredTickets == null || filteredTickets.Count == 0)
            {
                return false;
            }

            using (var writer = new StreamWriter(saveLocation))
            {
                var headerLine = string.Join(",", "id", "id_user", "applicant", "ticket_creation_date_time", "subject", "assigned_user", "text", "priority", "affected_application", "user_creation", "status", "notes");
                writer.WriteLine(headerLine);

                foreach (var ticket in filteredTickets)
                {
                    var dataLine = string.Join(",",
                        $"\"{ticket.Id}\"",
                        $"\"{ticket.IdUser}\"",
                        $"\"{ticket.Applicant}\"",
                        $"\"{ticket.TicketCreationDateTime}\"",
                        $"\"{ticket.Subject}\"",
                        $"\"{ticket.AssignedUser}\"",
                        $"\"{ticket.Text}\"",
                        $"\"{ticket.Priority}\"",
                        $"\"{ticket.AffectedApplication}\"",
                        $"\"{ticket.UserCreation}\"",
                        $"\"{ticket.Status}\"",
                        $"\"{ticket.Notes}\"");

                    writer.WriteLine(dataLine);
                }

                return true;
            }
        }

        public List<Ticket> GetAllTicket()
        {
            List<Ticket> ticket = new List<Ticket>();
            log.Error("START GetAllTicket");

            try
            {
                ticket = DALTicket.GetAllTicket();
                return ticket;


            }
            catch (DALException ex)
            {
                log.Error("Unable to GetAllTicket: " + ex.Message, ex);
                throw new ApplicationException("Unable to GetAllTicket: " + ex.Message, ex);

            }
        }
        public List<Ticket> GetListTicket(int? IdUser)
        {
            List<Ticket> ticket = new List<Ticket>();
            log.Debug("START GetListTicket");
            if (!IdUser.HasValue)
            {
                log.Error("Input parameter not set or mismatch");
                throw new ArgumentNullException("Input parameter not set or mismatch");
            }
            try
            {
                ticket = DALTicket.GetListTicket(IdUser);
                return ticket;

            }
            catch (DALException ex)
            {
                log.Error("Unable to GET TICKETS LIST: " + ex.Message, ex);
                throw new ApplicationException("Unable To get tickets list: " + ex.Message, ex);
            }
        }
        public void UpdateTicket(TicketOperations ticketOperations, string operationType)
        {
            log.Debug("START SaveClient");
            if (ticketOperations == null)
            {
                log.Error("Input parameter not set or mismatch");
                throw new ArgumentNullException("Input parameter not set or mismatch");
            }
            try
            {
                DALTicket.TransactionalOperationUpdateTicket(ticketOperations, operationType);

                log.Debug("END UpdateTicket");
            }
            catch (DALException ex)
            {
                log.Error("Unable to update Ticket: " + ex.Message, ex);
                throw new ApplicationException("Unable to update Ticket: " + ex.Message, ex);
            }
        }
        public List<Ticket> GetListTicketDetails(int? Id)
        {
            List<Ticket> ticket = new List<Ticket>();
            log.Debug("START GetListTicketDetails");
            if (Id == null)
            {
                log.Error("Input parameter not set or mismatch");
                throw new ArgumentNullException("Input parameter not set or mismatch");
            }
            try
            {
                ticket = DALTicket.TransactionalOperationGetTicketDetails(Id);
                log.Debug("END GetListTicketDetails");
                return ticket;
            }
            catch (DALException ex)
            {
                log.Error("Unable to GetListTicketDetails: " + ex.Message, ex);
                throw new ApplicationException("Unable to GetListTicketDetails: " + ex.Message, ex);
            }
        }
        public void AssignUser(TicketOperations ticketOperations, int IdUser)
        {
            log.Debug("START AssignUser");
            try
            {
                Ticket ticket = DALTicket.SelectById(ticketOperations.Id);
                if (ticket == null)
                {
                    log.Error("Ticket cannot be assigned.");
                    throw new InvalidOperationException("Ticket cannot be assigned.");
                }

                User user = DALUser.SelectById(IdUser);
                if (user == null || user.Permission != "user")
                {
                    log.Error("User not found or not a simple user.");
                    throw new InvalidOperationException("User not found or not a simple user.");
                }

                DALTicket.AssignUser(ticketOperations, IdUser);

                log.Debug("END AssignUser");
            }
            catch (Exception ex)
            {
                log.Error("Unable to assign ticket to user: " + ex.Message, ex);
                throw new ApplicationException("Unable to assign ticket to user: " + ex.Message, ex);
            }
        }
    }
}
