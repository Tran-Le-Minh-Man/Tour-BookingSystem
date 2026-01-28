using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TourBookingSystem.Models;
using TourBookingSystem.Database;

namespace TourBookingSystem.DAOs
{
    public class OrderDAO
    {
        private readonly ApplicationDbContext _context;

        public OrderDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INSERT =================
        public bool Insert(Order order)
        {
            try
            {
                order.created_at = DateTime.Now;

                _context.Orders.Add(order);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Insert ERROR] " + ex.Message);
                return false;
            }
        }

        // ================= GET BY ID =================
        public Order? GetById(int id)
        {
            return _context.Orders
                           .FirstOrDefault(o => o.id == id);
        }

        // ================= UPDATE STATUS =================
        public bool UpdateStatus(int id, string status)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.id == id);
                if (order == null) return false;

                order.status = status.ToUpper();
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[UpdateStatus ERROR] " + ex.Message);
                return false;
            }
        }

        // ================= CHECK PAID ORDER =================
        public bool HasPaidOrder(int bookingId)
        {
            return _context.Orders
                           .Any(o => o.booking_id == bookingId
                                  && o.status == "PAID");
        }
    }
}
