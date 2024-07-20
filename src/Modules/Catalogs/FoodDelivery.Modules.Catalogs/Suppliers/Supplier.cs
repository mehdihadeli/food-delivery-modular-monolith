using BuildingBlocks.Core.Domain;

namespace FoodDelivery.Modules.Catalogs.Suppliers;

public class Supplier : Entity<SupplierId>
{
    public string Name { get; private set; }

    public Supplier(SupplierId id, string name) : base(id)
    {
        Name = name;
    }
}
