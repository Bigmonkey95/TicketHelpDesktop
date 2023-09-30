using log4net;
using Smart.SqlHelper;
using Smart.TicketHelpDesktop.Model;
using System.Data;
using System.Data.SqlClient;

namespace Smart.TicketkHelpDesktop.SqlServerDAL
{
    public static class DALDetailTicket

    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DALDetailTicket));

        #region SqlParams
        private const string PARM_id = "@Id";
        private const string PARM_operation = "@Operation";
        private const string PARM_id_ticket = "@IdTicket";
        private const string PARM_create_by = "@CreateBy";
        private const string PARM_in_charge_of = "@InChargeOf";
        private const string PARM_charge_datetime = "@ChargeDateTime";
        private const string PARM_modified_by = "@ModifiedBy";
        private const string PARM_modified_datetime = "@ModifiedDateTime";
        private const string PARM_ticket_creation_date_time = "@TicketCreationDateTime";
        private const string PARM_closed_by = "@ClosedBy";
        private const string PARM_note_updated = "@NoteUpdated";
        private const string PARM_dated_closed = "@DatedClosed";
        #endregion
        #region SqlFilter
        private const string FILTER_id = "id = @Id";
        private const string FILTER_id_ticket = "id_ticket = @IdTicket";
        private const string FILTER_create_by = "create_by = @CreateBy";
        private const string FILTER_in_charge_of = "in_charge_of = @InChargeOf";
        private const string FILTER_charge_datetime = "charge_datetime = @ChargeDateTime";
        private const string FILTER_modified_by = "modified_by = @ModifiedBy";
        private const string FILTER_modified_datetime = "modified_datetime = @ModifiedDateTime";
        private const string FILTER_ticket_creation_date_time = "ticket_creation_date_time = @TicketCreationDateTime";
        private const string FILTER_closed_by = "closed_by = @ClosedBy";
        private const string FILTER_AND = " AND ";
        #endregion
        #region SqlSelect
        private const string SQL_SELECT = "SELECT id, id_ticket, operation, create_by, in_charge_of, charge_datetime, modified_by, ticket_creation_date_time, modified_datetime, closed_by, note_updated, dated_closed " +
            "FROM details_ticket "
            + "WHERE 1 = 1 ";
        #endregion
        #region SqlInsert

        private const string SQL_INSERT_DETAIL_TICKET = "INSERT INTO details_ticket ( id_ticket, operation, create_by, in_charge_of, charge_datetime, modified_by, modified_datetime, ticket_creation_date_time, closed_by, dated_closed, note_updated) VALUES ( @IdTicket, @Operation, @CreateBy, @InChargeOf, @ChargeDateTime, @ModifiedBy, @ModifiedDateTime,@TicketCreationDateTime, @ClosedBy, @DatedClosed, @NoteUpdated ) ; SELECT SCOPE_IDENTITY()";
        #endregion

        public static List<DetailTicket> GetListTicket(int? IdTicket, SqlTransaction trans)
        {
            log.Debug("START GetListTicket");
            var detailTicket = new List<DetailTicket>();
            String sqlString = SQL_SELECT;
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParm = null;
            tmpParm = new SqlParameter(PARM_id_ticket, SqlDbType.Int);
            tmpParm.Value = IdTicket;
            parms.Add(tmpParm);
            sqlString += FILTER_AND + FILTER_id_ticket;

            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(trans, CommandType.Text, sqlString, parms.ToArray()))
                {
                    while (rdr.Read())
                    {
                        detailTicket.Add(fillData(rdr));
                    }

                    log.Debug("END SELECT LIST TICKET BY IDTICKET");
                    return detailTicket;
                }
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
        }
        public static DetailTicket SelectDetailById(int? IdTicket)
        {
            log.Debug("START Select By Id");
            DetailTicket result = null;
            String sqlString = SQL_SELECT;
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParm = null;
            tmpParm = new SqlParameter(PARM_id_ticket, SqlDbType.Int);
            tmpParm.Value = IdTicket;
            parms.Add(tmpParm);
            sqlString += FILTER_AND + FILTER_id_ticket;

            try
            {
                using (SqlDataReader rdr = SqlServerHelper.ExecuteReader(ConnectionUtil.ConnectionStringTest, CommandType.Text, sqlString, parms.ToArray()))
                {
                    if (rdr.Read())
                    {
                        result = fillData(rdr);
                    }
                }
                log.Debug("END Select By Id");
                return result;
            }
            catch (SqlException e)
            {
                log.Error(e.Message, e);
                throw new Smart.TicketHelpDesktop.Model.Exceptions.DALException(e.Message, e);
            }
        }
        internal static void insertDetailTicket(DetailTicket detailTicket, SqlTransaction trans)
        {
            log.Debug("START inserDetailTicket - internal");
            List<SqlParameter> parms = getDetailTicketParms(detailTicket);
            if (trans == null)
                detailTicket.Id = Convert.ToInt32(SqlServerHelper.ExecuteScalar(ConnectionUtil.ConnectionStringTest, CommandType.Text, SQL_INSERT_DETAIL_TICKET, parms.ToArray()));
            else
                detailTicket.Id = Convert.ToInt32(SqlServerHelper.ExecuteScalar(trans, CommandType.Text, SQL_INSERT_DETAIL_TICKET, parms.ToArray()));
            log.Debug("End InsertDetailTicket - internal");

        }
        private static List<SqlParameter> getDetailTicketParms(DetailTicket detailticket)
        {
            log.Debug("Start getDetailTicketParms");
            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter tmpParam;
            tmpParam = new SqlParameter(PARM_id_ticket, SqlDbType.Int);
            tmpParam.Value = detailticket.IdTicket.HasValue ? (object)detailticket.IdTicket.Value : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_operation, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(detailticket.Operation) ? (object)detailticket.Operation : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_create_by, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(detailticket.CreateBy) ? (object)detailticket.CreateBy : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_ticket_creation_date_time, SqlDbType.DateTime);
            tmpParam.Value = detailticket.TicketCreationDateTime.HasValue ? (object)detailticket.TicketCreationDateTime.Value : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_in_charge_of, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(detailticket.InChargeOf) ? (object)detailticket.InChargeOf : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_charge_datetime, SqlDbType.DateTime);
            tmpParam.Value = detailticket.ChargeDateTime.HasValue ? (object)detailticket.ChargeDateTime.Value : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_modified_by, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(detailticket.ModifiedBy) ? (object)detailticket.ModifiedBy : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_note_updated, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(detailticket.NoteUpdated) ? (object)detailticket.NoteUpdated : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_modified_datetime, SqlDbType.DateTime);
            tmpParam.Value = detailticket.ModifiedDateTime.HasValue ? (object)detailticket.ModifiedDateTime.Value : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_closed_by, SqlDbType.NVarChar);
            tmpParam.Value = !string.IsNullOrEmpty(detailticket.ClosedBy) ? (object)detailticket.ClosedBy : DBNull.Value;
            parms.Add(tmpParam);

            tmpParam = new SqlParameter(PARM_dated_closed, SqlDbType.DateTime);
            tmpParam.Value = detailticket.DatedClosed.HasValue ? (object)detailticket.DatedClosed.Value : DBNull.Value;
            parms.Add(tmpParam);


            log.Debug("Fine get Parms");
            return parms;

        }
        internal static DetailTicket fillData(SqlDataReader rdr)
        {
            DetailTicket detailticket = new DetailTicket();

            if (rdr["id"] != DBNull.Value) detailticket.Id = (int)rdr["id"];
            if (rdr["id_ticket"] != DBNull.Value) detailticket.IdTicket = (int)rdr["id_ticket"];
            if (rdr["operation"] != DBNull.Value) detailticket.Operation = (string)rdr["operation"];
            if (rdr["create_by"] != DBNull.Value) detailticket.CreateBy = (string)rdr["create_by"];
            if (rdr["in_charge_of"] != DBNull.Value) detailticket.InChargeOf = (string)rdr["in_charge_of"];
            if (rdr["charge_datetime"] != DBNull.Value) detailticket.ChargeDateTime = (DateTime)rdr["charge_datetime"];
            if (rdr["ticket_creation_date_time"] != DBNull.Value) detailticket.TicketCreationDateTime = (DateTime)rdr["ticket_creation_date_time"];
            if (rdr["modified_by"] != DBNull.Value) detailticket.ModifiedBy = (string)rdr["modified_by"];
            if (rdr["modified_datetime"] != DBNull.Value) detailticket.ModifiedDateTime = (DateTime)rdr["modified_datetime"];
            if (rdr["closed_by"] != DBNull.Value) detailticket.ClosedBy = (string)rdr["closed_by"];
            if (rdr["note_updated"] != DBNull.Value) detailticket.NoteUpdated = (string)rdr["note_updated"];
            if (rdr["dated_closed"] != DBNull.Value) detailticket.DatedClosed = (DateTime)rdr["dated_closed"];
            return detailticket;
        }


    }

}