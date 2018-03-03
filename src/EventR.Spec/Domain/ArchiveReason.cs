namespace EventR.Spec.Domain
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum ArchiveReason
    {
        None = 0,

        [EnumMember]
        RequestedByCustomer = 1,

        [EnumMember]
        AutomaticDueInactivity = 2,
    }
}
