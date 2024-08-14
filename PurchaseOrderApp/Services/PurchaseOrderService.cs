// Services/PurchaseOrderService.cs
using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Data;
using PurchaseOrderApp.Models;
using PurchaseOrderApp.Exceptions;

namespace PurchaseOrderApp.Services
{
    public class PurchaseOrderService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseOrder>> GetAllPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders.Include(po => po.Lines).ToListAsync();
        }

        public async Task<PurchaseOrder> GetPurchaseOrderAsync(int id)
        {
            return await _context.PurchaseOrders.Include(po => po.Lines).FirstOrDefaultAsync(po => po.ID == id);
        }

        public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();
            return purchaseOrder;
        }

        public async Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            _context.Entry(purchaseOrder).State = EntityState.Modified;
            foreach (var line in purchaseOrder.Lines)
            {
                _context.Entry(line).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return purchaseOrder;
        }

        public async Task DeletePurchaseOrderAsync(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder != null)
            {
                _context.PurchaseOrders.Remove(purchaseOrder);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<PurchaseOrder> UpsertPurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            ValidatePurchaseOrder(purchaseOrder);

            var existingOrder = await _context.PurchaseOrders
                .Include(po => po.Lines)
                .FirstOrDefaultAsync(po => po.PurchId == purchaseOrder.PurchId);

            if (existingOrder == null)
            {
                purchaseOrder.UpdatedAt = DateTime.UtcNow;
                _context.PurchaseOrders.Add(purchaseOrder);
            }
            else
            {
                existingOrder.OrderAccount = purchaseOrder.OrderAccount;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                _context.PurchaseOrderLines.RemoveRange(existingOrder.Lines);
                existingOrder.Lines = purchaseOrder.Lines;

                _context.Entry(existingOrder).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return existingOrder ?? purchaseOrder;
        }

        private void ValidatePurchaseOrder(PurchaseOrder purchaseOrder)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(purchaseOrder.PurchId))
            {
                errors.Add("PurchId is required.");
            }

            if (string.IsNullOrWhiteSpace(purchaseOrder.OrderAccount))
            {
                errors.Add("OrderAccount is required.");
            }

            if (purchaseOrder.Lines == null || !purchaseOrder.Lines.Any())
            {
                errors.Add("At least one line item is required.");
            }
            else
            {
                foreach (var line in purchaseOrder.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line.ItemId))
                    {
                        errors.Add($"ItemId is required for line item.");
                    }                 
                    else if (line.ItemId.Equals("BadItem", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"ItemId 'BadItem' is not allowed.");
                    }

                    if (line.Quantity <= 0)
                    {
                        errors.Add($"Quantity must be greater than 0 for item {line.ItemId}.");
                    }

                    if (line.LineAmount <= 0)
                    {
                        errors.Add($"LineAmount must be greater than 0 for item {line.ItemId}.");
                    }
                }
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }
        public async Task<List<PurchaseOrder>> GetPurchaseOrdersAsync(DateTime? updatedAt = null)
        {
            var query = _context.PurchaseOrders.Include(po => po.Lines).AsQueryable();

            if (updatedAt.HasValue)
            {
                query = query.Where(po => po.UpdatedAt >= updatedAt.Value);
            }

            return await query.OrderByDescending(po => po.UpdatedAt).ToListAsync();
        }

        public void SeedTestData()
        {
            if (!_context.PurchaseOrders.Any())
            {
                var testOrders = new List<PurchaseOrder>
        {
            new PurchaseOrder
            {
                PurchId = "PO-001",
                OrderAccount = "ACCT-001",
                UpdatedAt = DateTime.UtcNow,
                Lines = new List<PurchaseOrderLine>
                {
                    new PurchaseOrderLine { PurchId = "PO-001", ItemId = "ITEM-001", Quantity = 10, LineAmount = 100 },
                    new PurchaseOrderLine { PurchId = "PO-001", ItemId = "ITEM-002", Quantity = 5, LineAmount = 75 }
                }
            },
            new PurchaseOrder
            {
                PurchId = "PO-002",
                OrderAccount = "ACCT-002",
                UpdatedAt = DateTime.UtcNow.AddHours(-4), // One day ago
                Lines = new List<PurchaseOrderLine>
                {
                    new PurchaseOrderLine { PurchId = "PO-002", ItemId = "ITEM-003", Quantity = 8, LineAmount = 120 }
                }
            },
            new PurchaseOrder
            {
                PurchId = "PO-003",
                OrderAccount = "ACCT-003",
                UpdatedAt = DateTime.UtcNow.AddHours(-2), // 12 hours ago
                Lines = new List<PurchaseOrderLine>
                {
                    new PurchaseOrderLine { PurchId = "PO-003", ItemId = "ITEM-001", Quantity = 15, LineAmount = 150 },
                    new PurchaseOrderLine { PurchId = "PO-003", ItemId = "ITEM-004", Quantity = 3, LineAmount = 90 }
                }
            }
        };

                _context.PurchaseOrders.AddRange(testOrders);
                _context.SaveChanges();
            }
        }
    }
}