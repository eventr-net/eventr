namespace EventR.Spec.Domain
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum TerminateReason
    {
        None = 0,

        [EnumMember]
        RequestedByCustomer = 1,

        [EnumMember]
        AutomaticDueInactivity = 2,
    }
}
