using System.ComponentModel.DataAnnotations.Schema;
using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.TicketTypes;

namespace EventFlow.Domain.Orders.OrderItems;

public class OrderItem : AuditableEntity
{
    public Guid OrderId { get; private set; }
    public Guid TicketTypeId { get; private set; }
    public Guid TenantId {get; private set;}
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => Quantity * UnitPrice;

    public Order Order { get; private set; } = null!;
    public TicketType TicketType { get; private set; } = null!;

    private OrderItem() { } // EF Core

    private OrderItem(Guid id,Guid tenantId,Guid ticketTypeId, int quantity, decimal unitPrice):base(id)
    {
        TicketTypeId = ticketTypeId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TenantId=tenantId;
    }

    internal static OrderItem Create(Guid id,Guid tenantId,Guid ticketTypeId, int quantity, decimal unitPrice) =>
        new(id,tenantId,ticketTypeId, quantity, unitPrice);
}