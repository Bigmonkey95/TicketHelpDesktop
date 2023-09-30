using log4net;
using Smart.SqlHelper;
using Smart.TicketHelpDesktop.Model;
using Smart.TicketHelpDesktop.SqlServerDAL;
using System.Data;
using System.Data.SqlClient;

namespace Smart.TicketkHelpDesktop.SqlServerDAL
{
    public static class DALTicket

    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DALTicket));

        #region SqlParams
        private const string PARM_ticketCreationDatetimeStart = "@TicketCreationDatetimeStart";
        private const string PARM_ticketCreationDatetimeEnd = "@TicketCreationDatetimeEnd";
        private const string PARM_id = "@Id";
        private const string PARM_id_user = "@IdUser";
        private const string PARM_assigned_user = "@AssignedUser";
        private const string PARM_id_detail = "@IdDetail";
        private const string PARM_applicant = "@Applicant";
        private const string PARM_subject = "@Subject";
        private const string PARM_text = "@Text";
        private const string PARM_ticket_creation_date_time = "@TicketCreationDatetime";
        private const string PARM_priority = "@Priority";
        private const string PARM_affected_application = "@AffectedApplication";
        private const string PARM_user_creation = "@UserCreation";
        private const string PARM_status = "@Status";
        private const string PARM_notes = "@Notes";
        private const string PARM_dated_closed = "@DatedClosed";
        private const string PARM_closed_by = "@ClosedBy";
        private const string PARM_modified_by = "@ModifiedBy";
        private const string PARM_modified_datetime = "@ModifiedDateTime";

        #endregion
        #region SqlFilter

        private const string FILTER_LIKE_subject = "subject LIKE '%' + @Subject + '%' ";
        private const string FILTER_LIKE_text = "text LIKE '%' + @Text + '%' ";
        private const string FILTER_ticketCreationDatetimeStart = "ticketCreationDatetimeStart = @TicketCreationDatetimeStart ";
        private const string FILTER_ticketCreationDatetimeEnd = "ticketCreationDatetimeEnd = @TicketCreationDatetimeEnd ";
        private const string FILTER_RANGE_ticketCreationDatetime = "ticket_creation_date_time BETWEEN @TicketCreationDateTimeStart AND @TicketCreationDateTimeEnd ";
        private const string FILTER_id = "id = @Id";
        private const string FILTER_id_user = "id_user = @IdUser";
        private const string FILTER_id_detail = "id_detail = @IdDetail";
        private const string FILTER_assigned_user = "assigned_user = @AssignedUser";
        private const string FILTER_applicant = "applicant = @Applicant";
        private const string FILTER_subject = "subject = @Subject";
        private const string FILTER_text = "text = @Text";
        private const string FILTER_ticket_creation_date_time = "ticket_creation_date_time = @TicketCreationDatetime";
        private const string FILTER_priority = "priority = @Priority";
        private const string FILTER_affected_application = "affected_application = @AffectedApplication";
        private const string FILTER_user_creation = "user_creation = @UserCreation";
        private const string FILTER_status = "status = @Status";
        private const string FILTER_notes = "notes = @Notes";
        private const string FILTER_AND = " AND ";
        #endregion
        #region SqlSelect
        private const string SQL_SELECT = "SELECT id, id_user, applicant, ticket_creation_date_time, assigned_user, subject, text, priority, affected_application, user_creation, status, notes " +
            "FROM ticket "
            + "WHERE 1 = 1 ";
        #endregion
        #region SqlInsert
        private const string SQL_INSERT_Ticket = "INSERT INTO ticket (id_user, applicant, ticket_creation_date_time, subject, text, priority, assigned_user, affected_application, user_creation, status, notes ) VALUES ( @IdUser, @Applicant, @TicketCreationDateTime, @Subject, @Text, @Priority, @AssignedUser, @AffectedApplication, @UserCreation, 'open', @Notes ) ; SELECT SCOPE_IDENTITY()";
        #endregion
        #region SqlUpdate
        private const string SQL_UPDATE_TICKET_NOTES = @"UPDATE ticket SET notes = @Notes WHERE id = @Id ";

        private const string SQL_CLOSE_TICKET = @"UPDATE ticket SET status = 'closed' WHERE id = @Id; ";
        private const string SQL_UPDATE_ASSIGNED = "UPDATE ticket SET assigned_user = @AssignedUser FROM ticket WHERE ticket.status = 'open' AND (assigned_user IS NULL OR assigned_user = '') AND id = @IdTicket";
        #endregion
        #region SqlDelete
        private const string SQL_DELETE_TICKET = "DELETE ticket WHERE id = @Id";

        #endregion

        public static List<Ticket> GetAllTicket()
        {
            log.Debug("Start GetAll ticket");
            var ticket = new List<Ticket>();
            String sqlString = SQL_SELECT;
            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, sqlString))
                {
                    while (rdr.Read())
                    {
                        ticket.Add(fillData(rdr));
                    }
                    log.Debug("End GetAllTicket");
                    return ticket;
                }
            }
            catch (SqlException e)
            {

                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e); ;
            }
        }
        public static void DeleteTicket(int? Id)
        {
            log.Debug("START DeleteTicket");
            try
            {
                deleteTicket(Id, null);
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
            log.Debug("END DeleteTicket");
        }
        internal static void deleteTicket(int? Id, SqlTransaction trans)
        {
            log.Debug("START deleteTicket - internal");
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParam = new SqlParameter(PARM_id, SqlDbType.Int);
            tmpParam.Value = Id;
            parms.Add(tmpParam);

            if (trans == null)
                SqlServerHelper.ExecuteNonQuery(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_DELETE_TICKET, parms.ToArray());
            else
                SqlServerHelper.ExecuteNonQuery(trans, CommandType.Text, SQL_DELETE_TICKET, parms.ToArray());
            log.Debug("END deleteTicket - internal");

        }
        public static Ticket SelectById(int? Id)
        {
            log.Debug("START Select By Id");
            Ticket result = null;
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
        public static void TransactionalOperationSaveTicketDetail(TicketOperations ticketOperations)
        {
            log.Debug("Start TransactionalOperationSaveTicketDetail");

            SqlConnection conn = new SqlConnection(ConnectionUtil.ConnectionStringTest);
            SqlTransaction trans = null;
            conn.Open();
            trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {

                insertTicket(ticketOperations, trans);

                DetailTicket detailTicket = new DetailTicket
                {
                    IdTicket = ticketOperations.Id,
                    Operation = "Create Ticket",
                    CreateBy = ticketOperations.UserCreation,
                    TicketCreationDateTime = ticketOperations.TicketCreationDatetime,

                };

                DALDetailTicket.insertDetailTicket(detailTicket, trans);
                trans.Commit();
            }
            catch (SqlException e)
            {
                try
                {
                    if (trans != null)
                        trans.Rollback();
                }
                catch (Exception ex)
                {
                    log.Error("Error during rollback:" + ex.Message, ex);
                }
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
            log.Debug("End SaveTicketDetail");
        }
        internal static void insertTicket(TicketOperations ticketOperations, SqlTransaction trans)
        {
            log.Debug("INIZIO inserTicket - internal");
            List<SqlParameter> parms = getTicketParms(ticketOperations);
            if (trans == null)
                ticketOperations.Id = Convert.ToInt32(SqlServerHelper.ExecuteScalar(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_INSERT_Ticket, parms.ToArray()));
            else
                ticketOperations.Id = Convert.ToInt32(SqlServerHelper.ExecuteScalar(trans, CommandType.Text, SQL_INSERT_Ticket, parms.ToArray()));
            log.Debug("FINE InsertTicket - internal");


        }
        public static List<Ticket> TransactionalOperationGetTicketDetails(int? Id)
        {
            log.Debug("Start TransactionalOperationGetTicketDetails");

            SqlConnection conn = new SqlConnection(ConnectionUtil.ConnectionStringTest);
            SqlTransaction trans = null;
            List<Ticket> tickets = new List<Ticket>();
            conn.Open();
            trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {

                Ticket ticket = SelectById(Id);
                var IdTicket = ticket.Id;

                if (ticket != null && ticket.Id.HasValue)
                {

                    List<DetailTicket> detailTickets = DALDetailTicket.GetListTicket(IdTicket, trans);


                    ticket.DetailTickets = detailTickets;
                    tickets.Add(ticket);
                }

                trans.Commit();
                return tickets;
            }
            catch (SqlException e)
            {
                try
                {
                    if (trans != null)
                        trans.Rollback();
                }
                catch (Exception ex)
                {
                    log.Error("Error during rollback:" + ex.Message, ex);
                }
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        public static List<Ticket> Filter(string? Subject, string? Text, DateTime? TicketCreationDatetimeStart, DateTime? TicketCreationDatetimeEnd, string? Applicant, string? Priority, string? AffectedApplication, string? UserCreation, string? Status, int? IdUser)
        {

            log.Debug("Start ");

            var data = new List<Ticket>();

            string sqlString = SQL_SELECT;

            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParm = null;

            if (Subject != null)
            {
                tmpParm = new SqlParameter(PARM_subject, SqlDbType.NVarChar);
                tmpParm.Value = Subject;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_LIKE_subject;
            }

            if (Text != null)
            {
                tmpParm = new SqlParameter(PARM_text, SqlDbType.NVarChar);
                tmpParm.Value = Text;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_LIKE_text;
            }
            if ((TicketCreationDatetimeStart != null) && (TicketCreationDatetimeEnd != null))
            {
                tmpParm = new SqlParameter(PARM_ticketCreationDatetimeStart, SqlDbType.DateTime);
                tmpParm.Value = TicketCreationDatetimeStart;
                parms.Add(tmpParm);

                tmpParm = new SqlParameter(PARM_ticketCreationDatetimeEnd, SqlDbType.DateTime);
                tmpParm.Value = TicketCreationDatetimeEnd;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_RANGE_ticketCreationDatetime;
            }
            if (!string.IsNullOrEmpty(Applicant))
            {
                tmpParm = new SqlParameter(PARM_applicant, SqlDbType.NVarChar);
                tmpParm.Value = Applicant;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_applicant;
            }
            if (!string.IsNullOrEmpty(Priority))
            {
                tmpParm = new SqlParameter(PARM_priority, SqlDbType.NVarChar);
                tmpParm.Value = Priority;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_priority;
            }

            if (!string.IsNullOrEmpty(AffectedApplication))
            {
                tmpParm = new SqlParameter(PARM_affected_application, SqlDbType.NVarChar);
                tmpParm.Value = AffectedApplication;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_affected_application;
            }
            if (!string.IsNullOrEmpty(UserCreation))
            {
                tmpParm = new SqlParameter(PARM_user_creation, SqlDbType.NVarChar);
                tmpParm.Value = UserCreation;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_user_creation;
            }
            if (!string.IsNullOrEmpty(Status))
            {
                tmpParm = new SqlParameter(PARM_status, SqlDbType.NVarChar);
                tmpParm.Value = Status;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_status;
            }
            if (IdUser != null)
            {
                tmpParm = new SqlParameter(PARM_id_user, SqlDbType.NVarChar);
                tmpParm.Value = IdUser;
                parms.Add(tmpParm);

                sqlString += FILTER_AND + FILTER_id_user;
            }
            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, sqlString, parms.ToArray()))
                {
                    while (rdr.Read())
                    {
                        data.Add(fillData(rdr));
                    }
                }
                log.Debug("END ");
                return data;
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
        }
        public static void TransactionalOperationUpdateTicket(TicketOperations ticketOperations, string operationType)
        {
            log.Debug("Start TransactionalOperationUpdateTicket");
            SqlConnection conn = new SqlConnection(ConnectionUtil.ConnectionStringTest);
            SqlTransaction trans = null;
            conn.Open();
            trans = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                if (operationType.Equals("updateNotes", StringComparison.OrdinalIgnoreCase))
                {
                    updateTicketNote(ticketOperations, trans);
                }
                else if (operationType.Equals("closeTicket", StringComparison.OrdinalIgnoreCase))
                {
                    updateTicketNote(ticketOperations, trans);
                    closeTicket(ticketOperations, trans);
                }
                else
                {
                    log.Error("Invalid operationType specified");
                    throw new ArgumentException("Invalid operationType specified");
                }

                if (operationType.Equals("updateNotes", StringComparison.OrdinalIgnoreCase))
                {
                    DetailTicket detailTicket = new DetailTicket
                    {
                        IdTicket = ticketOperations.Id,
                        Operation = operationType,
                        ModifiedBy = DALUser.GetUserNameById(ticketOperations.IdUser),
                        ModifiedDateTime = DateTime.Now,
                        NoteUpdated = ticketOperations.Notes,

                    };
                    DALDetailTicket.insertDetailTicket(detailTicket, trans);
                }
                else
                {
                    DetailTicket detailTicket = new DetailTicket
                    {
                        IdTicket = ticketOperations.Id,
                        Operation = operationType,
                        ClosedBy = DALUser.GetUserNameById(ticketOperations.IdUser),
                        DatedClosed = DateTime.Now,
                        NoteUpdated = ticketOperations.Notes,

                    };
                    DALDetailTicket.insertDetailTicket(detailTicket, trans);
                }


                trans.Commit();
            }
            catch (SqlException e)
            {
                try
                {
                    if (trans != null)
                        trans.Rollback();
                }
                catch (Exception ex)
                {
                    log.Error("Error during rollback:" + ex.Message, ex);
                }
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
            log.Debug("END UpdateTicket");
        }
        internal static void updateTicketNote(TicketOperations ticketOperations, SqlTransaction trans)
        {
            log.Debug("START updateTicket - internal");

            List<SqlParameter> parms = getTicketParms(ticketOperations);
            SqlParameter tmpParam = new SqlParameter(PARM_id, SqlDbType.Int);
            tmpParam.Value = ticketOperations.Id;
            parms.Add(tmpParam);

            if (trans == null)
                SqlServerHelper.ExecuteNonQuery(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_UPDATE_TICKET_NOTES, parms.ToArray());
            else
                SqlServerHelper.ExecuteNonQuery(trans, CommandType.Text, SQL_UPDATE_TICKET_NOTES, parms.ToArray());

            log.Debug("End updateTicket - internal");
        }
        internal static void closeTicket(TicketOperations ticketOperations, SqlTransaction trans)
        {
            log.Debug("START closeTicket - internal");
            List<SqlParameter> parms = getTicketParms(ticketOperations);
            SqlParameter tmpParam = new SqlParameter(PARM_id, SqlDbType.Int);
            tmpParam.Value = ticketOperations.Id;
            parms.Add(tmpParam);

            if (trans == null)
                SqlServerHelper.ExecuteNonQuery(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_CLOSE_TICKET, parms.ToArray());
            else
                SqlServerHelper.ExecuteNonQuery(trans, CommandType.Text, SQL_CLOSE_TICKET, parms.ToArray());

            log.Debug("END closeTicket - internal");
        }
        public static List<Ticket> GetListTicket(int? IdUser)
        {
            log.Debug("START GetListTicket");
            var ticket = new List<Ticket>();
            String sqlString = SQL_SELECT;
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParm = null;
            tmpParm = new SqlParameter(PARM_assigned_user, SqlDbType.Int);
            tmpParm.Value = IdUser;
            parms.Add(tmpParm);
            sqlString += FILTER_AND + FILTER_assigned_user;

            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, sqlString, parms.ToArray()))
                {
                    while (rdr.Read())
                    {
                        ticket.Add(fillData(rdr));

                    }

                    log.Debug("END SELECT LIST TIKCKET BY IDUSER");
                    return ticket;
                }
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e); ;
            }
        }
        public static void AssignUser(TicketOperations ticketOperations, int IdUser)
        {
            log.Debug("START AssignTicket");

            using (SqlConnection connection = new SqlConnection(ConnectionUtil.ConnectionStringTest))
            {
                connection.Open();
                SqlTransaction trans = connection.BeginTransaction();

                try
                {
                    assignUser(ticketOperations, IdUser, trans);

                    DetailTicket detailTicket = new DetailTicket
                    {
                        IdTicket = ticketOperations.Id,
                        Operation = "Assigned",
                        InChargeOf = DALUser.GetUserNameById(IdUser),
                        ChargeDateTime = DateTime.Now,

                    };

                    DALDetailTicket.insertDetailTicket(detailTicket, trans);

                    trans.Commit();

                    log.Debug("END AssignTicket");
                }
                catch (Exception e)
                {
                    trans.Rollback();

                    log.Error(e.Message, e);
                    throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
                }
            }
        }
        internal static void assignUser(TicketOperations ticketOperations, int IdUser, SqlTransaction trans)
        {
            log.Debug("Start asignuser - internal");

            try
            {
                string sqlString = SQL_UPDATE_ASSIGNED;

                List<SqlParameter> parameters = new List<SqlParameter>
                {
                     new SqlParameter("@AssignedUser", SqlDbType.Int) { Value = IdUser },
                     new SqlParameter("@IdTicket", SqlDbType.Int) { Value = ticketOperations.Id }
                };

                using (SqlCommand command = new SqlCommand(sqlString, trans.Connection, trans))
                {
                    command.Parameters.AddRange(parameters.ToArray());

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        log.Debug("Ticket assigned to user successfully");
                    }
                    else
                    {
                        log.Warn("No records found to assign the ticket to a user.");
                    }
                }
            }
            catch (SqlException e)
            {
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException("Error updating the user assigned to the ticket.", e);
            }
            log.Debug("END asignuser - internal");
        }
        private static List<SqlParameter> getTicketParms(TicketOperations ticketOperations)
        {
            log.Debug("Start get Ticket Parms");
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParam;


            tmpParam = new SqlParameter(PARM_id_user, SqlDbType.Int);
            tmpParam.Value = ticketOperations.IdUser.HasValue ? (object)ticketOperations.IdUser.Value : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_applicant, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(ticketOperations.Applicant) ? (object)ticketOperations.Applicant : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_subject, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(ticketOperations.Subject) ? (object)ticketOperations.Subject : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_text, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(ticketOperations.Text) ? (object)ticketOperations.Text : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_priority, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(ticketOperations.Priority) ? (object)ticketOperations.Priority : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_affected_application, SqlDbType.VarChar);
            tmpParam.Value = !string.IsNullOrEmpty(ticketOperations.AffectedApplication) ? (object)ticketOperations.AffectedApplication : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_user_creation, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(ticketOperations.UserCreation) ? (object)ticketOperations.UserCreation : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_assigned_user, SqlDbType.Int);
            tmpParam.Value = ticketOperations.AssignedUser.HasValue ? (object)ticketOperations.AssignedUser.Value : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_notes, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(ticketOperations.Notes) ? (object)ticketOperations.Notes : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_status, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(ticketOperations.Status) ? (object)ticketOperations.Status : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_ticket_creation_date_time, SqlDbType.DateTime);
            tmpParam.Value = ticketOperations.TicketCreationDatetime.HasValue ? (object)ticketOperations.TicketCreationDatetime.Value : DBNull.Value;
            parms.Add(tmpParam);

            log.Debug("End get Ticket Parms");
            return parms;
        }
        internal static Ticket fillData(SqlDataReader rdr)
        {
            Ticket ticket = new Ticket();

            if (rdr["id"] != DBNull.Value) ticket.Id = (int)rdr["id"];
            if (rdr["id_user"] != DBNull.Value) ticket.IdUser = (int)rdr["id_user"];
            if (rdr["applicant"] != DBNull.Value) ticket.Applicant = (string)rdr["applicant"];
            if (rdr["ticket_creation_date_time"] != DBNull.Value) ticket.TicketCreationDateTime = (DateTime)rdr["ticket_creation_date_time"];
            if (rdr["subject"] != DBNull.Value) ticket.Subject = (string)rdr["subject"];
            if (rdr["text"] != DBNull.Value) ticket.Text = (string)rdr["text"];
            if (rdr["assigned_user"] != DBNull.Value) ticket.AssignedUser = (int)rdr["assigned_user"];
            if (rdr["priority"] != DBNull.Value) ticket.Priority = (string)rdr["priority"];
            if (rdr["affected_application"] != DBNull.Value) ticket.AffectedApplication = (string)rdr["affected_application"];
            if (rdr["user_creation"] != DBNull.Value) ticket.UserCreation = (string)rdr["user_creation"];
            if (rdr["status"] != DBNull.Value) ticket.Status = (string)rdr["status"];
            if (rdr["notes"] != DBNull.Value) ticket.Notes = (string)rdr["notes"];
            return ticket;
        }

    }
}
