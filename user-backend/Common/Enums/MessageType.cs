namespace Common.Enums
{
    public enum MessageType
    {
        SignReminder = 0,
        BeforeSigning = 1,
        AfterSigning = 2,
        SingleSignerSignedNotification = 3,
        AllSignersSignedNotification = 4,
        ProgramIsAboutToExipred = 5, 
        OtpCode = 6 ,
        Decline = 7,
        SignerViewDocumentNotification = 8,
        ProgramCapacityIsAboutToExpired = 9,
        UnsignedDocumentIsAboutToBeDeleted = 10,
        UserPeriodicReport = 11,   
        SharedDocumentNotification = 12,
        ManagementPeriodicReport = 13,
        VideoConfrenceNotification = 14,
        SignerNoteNotification = 15
    }

    public enum ProgramCapacityType
    {
        SMS = 1,
        Documents = 2,
        VisualIdentification = 3,
    }
}
