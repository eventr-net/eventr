namespace EventR.Spec.Domain.Events
{
    public interface ITerminateCustomerAccount
    {
         TerminateReason Reason { get; set; }
    }
}
