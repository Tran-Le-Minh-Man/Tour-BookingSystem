using System.Data.OleDb;
using System.Diagnostics;
using TourBookingSystem.Models;
using TourBookingSystem.Utils;

namespace TourBookingSystem.DAOs
{
    public class OrderDAO
    {
        private static readonly string TABLE_NAME = "[Orders]";

        public class OderDAOException : Exception
        {
            private readonly string operation;

            public OderDAOException(string operation, string message, Exception cause) : base(message, cause)
            {
                this.operation = operation;
            }

            public string getOperation() { return operation; }
        }
        public bool Insert(Order order)
        {
            string sql =
                "INSERT INTO [Orders] " +
                "([user_id],[booking_id],[tour_id],[quantity],[total_price],[note]," +
                "[payment_method],[payment_provider],[status],[created_at]) " +
                "VALUES (?,?,?,?,?,?,?,?,?,?)";

            using (var conn = DBConnection.getConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.Add("?", OleDbType.Integer).Value = order.user_id;

                cmd.Parameters.Add("?", OleDbType.Integer).Value = order.booking_id;

                cmd.Parameters.Add("?", OleDbType.Integer).Value = order.tour_id;

                cmd.Parameters.Add("?", OleDbType.Integer).Value = order.quantity;

                cmd.Parameters.Add("?", OleDbType.Double).Value = Convert.ToDouble(order.total_price);

                cmd.Parameters.Add("?", OleDbType.VarChar).Value = order.note ?? "";

                cmd.Parameters.Add("?", OleDbType.VarChar).Value = order.payment_method ?? "";

                cmd.Parameters.Add("?", OleDbType.VarChar).Value = order.payment_provider ?? "";

                cmd.Parameters.Add("?", OleDbType.VarChar).Value = order.status;

                cmd.Parameters.Add("?", OleDbType.Date).Value = DateTime.Now;

                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public Order getById(int id)
        {
            string sql = $"SELECT * FROM {TABLE_NAME} WHERE id = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                    using (OleDbDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            return mapOrder(rs);
                        }
                    }
                }
            }

            return null;
        }
        public bool updateStatus(int id, string status)
        {
            string sql = "UPDATE " + TABLE_NAME + " SET [status] = ? WHERE [id] = ?";

            using (OleDbConnection conn = DBConnection.getConnection())
            {
                try
                {
                    using (OleDbCommand stmt = new OleDbCommand(sql, conn))
                    {
                        Debug.WriteLine("[updateStatus] Running SQL: " + sql);

                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = status.ToUpper() });
                        stmt.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                        int rows = stmt.ExecuteNonQuery();
                        Debug.WriteLine("[updateStatus] Rows affected: " + rows);

                        return rows > 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[updateStatus ERROR] " + ex.Message);
                    return false;
                }
            }
        }

        public bool hasPaidOrder(int bookingId)
        {
            string sql = "SELECT COUNT(*) FROM Orders WHERE booking_id=? AND status='PAID'";

            using (var conn = DBConnection.getConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("?", bookingId);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                return count > 0;
            }
        }
        private Order mapOrder(OleDbDataReader rs)
        {
            return new Order
            {
                id = Convert.ToInt32(rs["id"]),
                user_id = Convert.ToInt32(rs["user_id"]),
                booking_id = Convert.ToInt32(rs["booking_id"]),
                tour_id = Convert.ToInt32(rs["tour_id"]),

                quantity = Convert.ToInt32(rs["quantity"]),
                total_price = Convert.ToDecimal(rs["price_total"]),

                note = rs["note"].ToString(),

                payment_method = rs["payment_method"].ToString(),
                payment_provider = rs["payment_provider"].ToString(),

                status = rs["status"].ToString(),

                created_at = Convert.ToDateTime(rs["created_at"])
            };
        }
    }
}
