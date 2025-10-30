namespace HistoryIntegratorService.Common.Enums
{
    public enum DocumentStatus
    {
        Draft = 0,
        Created = 1,
        Sent = 2,
        Viewed = 3,
        Signed = 4,
        Declined = 5,
        SendingFailed = 6,
        Deleted = 7,
        Canceled = 8,
        //Signing document after signing flow using signer1
        ExtraServerSigned = 9
    }
}
