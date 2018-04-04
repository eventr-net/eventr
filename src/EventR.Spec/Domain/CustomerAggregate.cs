namespace EventR.Spec.Domain
{
    using System;
    using System.Linq;
    using EventR.Abstractions;
    using EventR.Spec.Domain.Events;

    public sealed class CustomerAggregate : AggregateRoot<Customer>
    {
        public CustomerAggregate(string streamId, IAggregateRootServices services)
            : base(streamId, services)
        { }

        public CustomerAggregate(IAggregateRootServices services)
            : this(Guid.NewGuid().ToString("N"), services)
        { }

        public CustomerAggregate SetUpNewCustomer(string email, string preferredLanguage, int? age = null)
        {
            Apply<IInitializeNewCustomer>(e =>
            {
                e.Email = email;
                e.PreferredLanguage = preferredLanguage;
                e.Age = age;
            });
            return this;
        }

        public CustomerAggregate PlaceOrder(ShoppingCart cart, Address shippingAddress, DateTimeOffset? date = null, Guid? orderId = null)
        {
            Apply<IPlaceOrder>(e =>
            {
                e.Id = orderId ?? Guid.NewGuid();
                e.Date = date ?? DateTimeOffset.Now;
                e.ShoppingCart = cart;
                e.ShippingAddress = shippingAddress;
            });
            return this;
        }

        public CustomerAggregate ConfirmDelivery(Guid orderId, string byAgent, DateTimeOffset? date = null)
        {
            Apply<IConfirmDelivery>(e =>
            {
                e.Date = date ?? DateTimeOffset.Now;
                e.OrderId = orderId;
                e.Agent = byAgent;
            });
            return this;
        }

        public CustomerAggregate ChangeShippingAddress(Guid orderId, Address newAddress)
        {
            Apply<IChangeShippingAddress>(e =>
            {
                e.OrderId = orderId;
                e.NewShippingAddress = newAddress;
            });
            return this;
        }

        public void TerminateAccount(TerminateReason reason)
        {
            Apply<ITerminateCustomerAccount>(e =>
            {
                e.Reason = reason;
            });
        }

        #region Data handlers

        private void Handle(IInitializeNewCustomer e)
        {
            Data.Email = e.Email;
            Data.PreferredLanguage = e.PreferredLanguage;
            Data.Age = e.Age;
        }

        private void Handle(IPlaceOrder e)
        {
            var order = new Order
            {
                Id = e.Id,
                Created = e.Date,
                ShippingAddress = e.ShippingAddress,
                ShippingFee = 3.5m,
            };
            var lines = e.ShoppingCart.Items.Select(
                x => new OrderLine
                {
                    Sku = x.Sku,
                    Quantity = x.Quantity,
                    Label = $"human redable label for {x.Sku}",
                    Price = x.Quantity * 5.99m,
                    Tax = x.Quantity * 0.9m,
                }).ToList();
            order.Lines = lines;
            Data.Orders.Add(order);
        }

        private void Handle(ITerminateCustomerAccount e)
        {
            Data.IsTerminated = true;
            Data.TerminateReason = e.Reason;
        }

        private void Handle(IChangeShippingAddress e)
        {
            var order = Data.Orders?.FirstOrDefault(x => x.Id == e.OrderId);
            if (order == null)
            {
                throw new BusinessLogicException($"Order {e.OrderId} not found..");
            }

            if (order.IsDelivered)
            {
                throw new BusinessLogicException($"Can't change shipping address for order {e.OrderId}; it's already delivered.");
            }

            order.ShippingAddress = e.NewShippingAddress;
        }

        private void Handle(IConfirmDelivery e)
        {
            var order = Data.Orders?.FirstOrDefault(x => x.Id == e.OrderId);
            if (order == null)
            {
                throw new BusinessLogicException($"Order {e.OrderId} not found; customer {Data.Email}.");
            }

            order.DeliveryReceipt = new DeliveryReceipt
            {
                OrderId = e.OrderId,
                Delivered = e.Date,
                Agent = e.Agent,
            };
        }

        #endregion
    }
}
