namespace EventR.Spec.Domain.Events
{
    public interface IArchiveCustomerAccount
    {
         ArchiveReason Reason { get; set; }
    }
}
