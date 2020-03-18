namespace EventR.Spec.Domain.Events
{
    using System.Runtime.Serialization;

    public interface IInitializeNewCustomer
    {
        [DataMember(Order = 1)]
        string Email { get; set; }

        [DataMember(Order = 2)]
        string PreferredLanguage { get; set; }

        [DataMember(Order = 3)]
        int? Age { get; set; }
    }
}
