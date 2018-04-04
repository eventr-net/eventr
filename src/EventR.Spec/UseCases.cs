namespace EventR.Spec
{
    using System;
    using System.Collections.Generic;
    using EventR.Abstractions;
    using EventR.Spec.Domain;

    public static class UseCases
    {
        public static ICollection<Action<CustomerAggregate>> Simple()
        {
            return new Action<CustomerAggregate>[]
            {
                x => x.SetUpNewCustomer("john.simple@example.com", "en", 32).PlaceOrder(Cart1, Address1),
            };
        }

        public static ICollection<Action<CustomerAggregate>> Full()
        {
            var orderDate = DateTimeOffset.Parse("2018-02-17 09:22:16");
            var deliveryDate = orderDate.AddHours(32);
            var orderId = Guid.NewGuid();
            return new Action<CustomerAggregate>[]
            {
                x => x.SetUpNewCustomer("mary.advanced@example.com", "en", 41).PlaceOrder(Cart2, Address2, orderDate, orderId),
                x => x.ChangeShippingAddress(orderId, Address3),
                x => x.ConfirmDelivery(orderId, "John Doe", deliveryDate),
            };
        }

        public static ICollection<Action<CustomerAggregate>> ManyEvents(int n = 300)
        {
            Expect.Range(n, 1, 100000, nameof(n));

            var actions = new List<Action<CustomerAggregate>>(n + 2);
            var startDate = DateTimeOffset.Now.AddDays(-n);
            actions.Add(x => x.SetUpNewCustomer("lucas.many.events@example.com", "en"));
            for (int i = 0; i < n; i++)
            {
                var orderId = Guid.NewGuid();
                var orderDate = startDate.AddDays(n);
                actions.Add(x => x
                    .PlaceOrder(Cart3, Address4, orderDate)
                    .ConfirmDelivery(orderId, "John Doe"));
            }

            actions.Add(x => x.TerminateAccount(TerminateReason.RequestedByCustomer));

            return actions;
        }

        public static readonly ShoppingCart Cart1 = new ShoppingCart
        {
            Items = new ShoppingCartItem[]
            {
                new ShoppingCartItem("ITM-1234", 1),
            },
            Currency = "USD",
        };

        public static readonly ShoppingCart Cart2 = new ShoppingCart
        {
            Items = new ShoppingCartItem[]
            {
                new ShoppingCartItem("ITM-9999", 2),
                new ShoppingCartItem("ITM-9911", 1),
            },
            Currency = "CAD",
        };

        public static readonly ShoppingCart Cart3 = new ShoppingCart
        {
            Items = new ShoppingCartItem[]
            {
                new ShoppingCartItem("ITM-0493", 2),
                new ShoppingCartItem("ITM-1010", 1),
            },
            Currency = "GBP",
        };

        public static readonly Address Address1 = new Address
        {
            Name = "John Simple",
            ZipCode = "D34 67",
            Street = "Halsey Av. 164b",
            City = "Dallas",
            State = "TX",
            Country = "USA",
        };

        public static readonly Address Address2 = new Address
        {
            Name = "Mary Advanced",
            ZipCode = "TT23A81",
            Street = "La Revolucion D21",
            City = "Toronto",
            Country = "Canada",
        };

        public static readonly Address Address3 = new Address
        {
            Name = "Mary Advanced",
            ZipCode = "TT09329",
            Street = "Velvet Road 987",
            City = "Toronto",
            Country = "Canada",
        };

        public static readonly Address Address4 = new Address
        {
            Name = "Lucas Many Events",
            ZipCode = "WA4 N7",
            Street = "Kensington Road 12",
            City = "London",
            Country = "UK",
        };
    }
}
