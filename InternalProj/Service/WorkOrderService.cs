using InternalProj.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalProj.Service
{
    public class WorkOrderService
    {
        private readonly ApplicationDbContext _context;

        public WorkOrderService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task UpdateWorkOrderBalance(int workOrderId)
        {
            var workOrder = await _context.WorkOrders.FindAsync(workOrderId);
            if (workOrder == null) return;

            var totalReceipts = await _context.Receipts
                .Where(r => r.WorkOrderId == workOrderId)
                .SumAsync(r => (double?)r.CurrentAmount) ?? 0;

            var totalInvoices = await _context.Invoices
                .Where(i => i.WorkOrderId == workOrderId)
                .SumAsync(i => (decimal?)i.NetAmount) ?? 0;

            var advance = workOrder.Advance ?? 0;
            var subTotal = workOrder.SubTotal;

            var newBalance = (decimal)subTotal - (decimal)advance - (decimal)totalReceipts - totalInvoices;

            if (Math.Abs(newBalance) < 0.00001m)
            {
                newBalance = 0;
            }

            workOrder.Balance = (double?)newBalance;


            _context.WorkOrders.Update(workOrder);
            await _context.SaveChangesAsync();
        }

    }
}