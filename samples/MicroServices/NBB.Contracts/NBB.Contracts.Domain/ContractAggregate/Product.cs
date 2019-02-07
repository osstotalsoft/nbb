using System.Collections.Generic;
using NBB.Domain;

namespace NBB.Contracts.Domain.ContractAggregate
{
    public class Product : ValueObject
    {
        public string Name { get; }

        public decimal Price { get; }

        //Used by ef
        private Product()
        {
        }

        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Name;
            yield return Price;
        }
    }
}
