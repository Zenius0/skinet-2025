using System;
using Core.Entities.OrderAggregate;

namespace Core.Entities;

public class OrderItem : BaseEntity
{
    public ProductItemOrdered ItemOrdered { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
