namespace EventR.Spec.Domain.Events
{
    using System;

    public interface IInitializeNewCustomer
    {
        string Email { get; set; }

        string PreferredLanguage { get; set; }

        int? Age { get; set; }
    }
}
