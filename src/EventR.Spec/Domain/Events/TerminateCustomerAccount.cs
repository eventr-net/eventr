using System.Runtime.Serialization;

namespace EventR.Spec.Domain.Events
{
    [DataContract]
    public class TerminateCustomerAccount
    {
        [DataMember(Order = 1)]
        public TerminateReason Reason { get; set; }
    }
}
