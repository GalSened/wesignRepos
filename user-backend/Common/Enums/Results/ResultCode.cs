namespace Common.Enums.Results
{
    using System.ComponentModel;

    public enum ResultCode
    {

        [Description("Something went wrong. Please try again later")]
        GeneralErrorMessage = 0,
        [Description("Invalid document id")]
        InvalidDocumentId = 1,
        [Description("Invalid credential")]
        InvalidCredential = 2,
        [Description("Invalid token")]
        InvalidToken = 3,
        [Description("Invalid email address")]
        InvalidEmail = 4,
        [Description("Email already exists")]
        EmailAlreadyExist = 5,
        [Description("User does not exist, please register")]
        UserNotExist = 6,
        [Description("Activation is required")]
        ActivationRequired = 7,
        [Description("User is not group creator")]
        UserIsNotGroupCreator = 8,
        [Description("User belongs to another group")]
        UserBelongToAnotherCompany = 9,
        [Description("Program utilization is at maximum limit")]
        ProgramUtilizationGetToMax = 10,
        [Description("Cannot delete group creator")]
        CannotDeleteGroupCreator = 11,
        [Description("Invalid template id")]
        InvalidTemplateId = 12,
        [Description("Invalid page number")]
        InvalidPageNumber = 13,
        [Description("Invalid positive offset")]
        InvalidPositiveOffsetNumber = 14,
        [Description("Invalid limit")]
        InvalidLimitNumber = 15,
        [Description("Invalid DateTime format")]
        InvalidDateTimeFormat = 16,
        [Description("User is not editor/companyAdmin")]
        UserIsNotEditorOrCompanyAdmin = 17,
        [Description("Template is in use, there are documents related to it")]
        TemplateInUse = 18,
        [Description("Template does not belong to user group")]
        TemplateNotBelongToUserGroup = 19,
        [Description("User program expired")]
        UserProgramExpired = 20,
        [Description("Contact not created by user")]
        ContactNotCreatedByUser = 21,
        [Description("Field name does not exist")]
        FieldNameNotExist = 22,
        [Description("Invalid contact id")]
        InvalidContactId = 23,
        [Description("Contact with same means already exists")]
        ContactAlreadyExists = 24,
        [Description("Duplicate seal ids")]
        DuplicateSealsId = 25,
        [Description("Template with the same name already exist")]
        TemplateWithSameNameAlreadyExist = 26,
        [Description("Invalid DocumentCollection id")]
        InvalidDocumentCollectionId = 27,
        [Description("Contact does not belong to user group")]
        ContactNotBelongToUserGroup = 28,
        [Description("Document does not belong to document collection")]
        DocumentNotBelongToDocumentCollection = 29,
        [Description("Signer method does not fit contact means")]
        SignerMethodNotFeetToContactMeans = 30,
        [Description("Group with the same name already exists in the company")]
        GroupAlreadyExistInCompany = 31,
        [Description("Invalid GroupId")]
        InvalidGroupId = 32,
        [Description("Cannot delete your own user")]
        CannotDeleteYourOwnUser = 33,
        [Description("Operation is not allowed to a free trial user")]
        OperationNotAllowByFreeTrialUser = 34,
        [Description("Invalid refresh token")]
        InvalidRefreshToken = 35,
        [Description("Cannot download unsigned document")]
        CannotDownloadUnsignedDocument = 36,
        [Description("Cannot resend signed document")]
        CannotResendSignedDocument = 37,
        [Description("Invalid signer id ")]
        InvalidSignerId = 38,
        [Description("Cannot share unsigned document")]
        CannotShareUnsignedDocument = 39,
        [Description("Invalid file type")]
        InvalidFileType = 40,
        [Description("Not all fields exist in documents")]
        NotAllFieldsExistsInDocuments = 41,
        [Description("Not all fields belong to signer")]
        NotAllFieldsBelongToSigner = 42,
        [Description("Not all mandatory fields are filled in")]
        NotAllMandatoryFieldsFilledIn = 43,
        [Description("Program already exists")]
        ProgramAlreadyExist = 44,
        [Description("Program does not exist")]
        ProgramNotExist = 45,
        [Description("Company already exists")]
        CompanyAlreadyExists = 46,
        [Description("Company does not exist")]
        CompanyNotExist = 47,
        [Description("There are related companies to this program")]
        ThereAreRelatedCompaniesToThisProgram = 48,
        [Description("Invalid user type")]
        InvalidUserType = 49,
        [Description("Invalid expiry time")]
        InvalidExpiredTime = 50,
        [Description("Email or Username belongs to other company")]
        EmailBelongToOtherCompany = 51,
        [Description("There are users in the group")]
        ThereAreUsersInGroup = 52,
        [Description("Failed to initialize license request")]
        FailedInitLicenseRequest = 53,
        //[Description("Null input")] // TODO - change to different 
        //NullInput = 54,
        [Description("Invalid license")]
        InvalidLicense = 55,
        [Description("Documents exceed license limit")]
        DocumentsExceedLicenseLimit = 56,
        [Description("Sms exceed license limit")]
        SmsExceedLicenseLimit = 57,
        [Description("Users exceed license limit")]
        UsersExceedLicenseLimit = 58,
        [Description("Templates exceed license limit")]
        TemplatesExceedLicenseLimit = 59,
        [Description("Invalid base64string format")]
        InvalidBase64StringFormat = 60,
        [Description("Forbidden to create free trail user")]
        ForbiddenToCreateFreeTrailUser = 61,
        [Description("Cannot parse *.xlsx file to contacts")]
        InvalidBulkOfContacts = 62,
        [Description("Sign operation failed")]
        SignOperationFailed = 63,
        [Description("ActiveDirectory Config Not Exist")]
        ActiveDirectoryConfigNotExist = 64,
        [Description("Cant parse xml file to pdf meta data")]
        InvalidXML = 65,
        [Description("Amount of fields in a pdf is not the same as the amount of fields in a meta data ")]
        InvalidXMLMisMatchPlaceolders = 66,
        [Description("Document not belong to the user group")]
        DocumentNotBelongToUserGroup = 67,
        [Description("Cant parse interface to xml")]
        InvalidModelToParseToXml = 68,
        [Description("Cannot create template from another user")]
        CannotCreateTemplateFromAnotherUser = 69,
        [Description("Cannot download attachments form unsigned document")]
        CannotDownloadAttchmentsFromUnsignedDocument = 70,
        [Description("Signer not uploaded attachments or not required to")]
        SignerNotUpladedAttchmentsOrNotRequoredTo = 71,
        [Description("Can't resend in a wrong order")]
        TryToSendToSendDocNotInTheRightOrder = 72,
        [Description("Invalid phone")]
        InvalidPhone = 73,
        [Description("Invalid ReCaptcha token")]
        InvalidCaptchaToken = 74,
        [Description("Cannot create signing link for signer that already signed or declined")]
        CannotCreateSigningLinkToSignerThatSignedOrDecline = 75,
        [Description("Signature field is not assigned to signer")]
        SignatureFieldNotAssignToSigner = 76,
        [Description("Unsupported file type for signing")]
        UnsupportedFileTypeForSigning = 77,
        [Description("Invalid File Content")]
        InvalidFileContent = 78,
        [Description("Name is missing")]
        NameIsMissing = 79,
        [Description("Document already signed by the signer")]
        DocumentAlreadySignedBySigner = 80,
        [Description("Cannot cancel signed document")]
        CannotCancelSignedDocument = 81,
        [Description("Document already canceled")]
        DocumentAlreadyCanceled = 82,
        [Description("Document is not signed")]
        DocumentNotSigned = 83,
        [Description("Document is not configured to be signed using Signer1 after document signing flow")]
        DocumentNotConfigureToBeSignUsingSigner1AfterDocumentSigningFlow = 84,
        [Description("Your SMS provider not support sending SMS globally")]
        SmsProviderNotSupportSendingSmsGlobally = 85,
        [Description("Invalid field value according to field type")]
        InvalidFieldValueAccordingToFieldType = 86,
        [Description("Invalid field name not Exist in the selected template")]
        InvalidFieldNameNotExistInTemplate = 87,
        [Description("Unsupported image format")]
        NotSupportedImageFormat = 88,
        [Description("Export documents for this type not supported")]
        ExportDocumentsForThisTypeNotSupported = 89,
        [Description("Visual identity not required")]
        VisualIdentityNotRequired = 90,
        [Description("Visual identity missing service settings ")]
        VisualIdentityMissingSettings = 91,
        [Description("Visual identity can't get token from identity service")]
        VisualIdentityCantReadTokenFromService = 92,
        [Description("Visual identity operation failed")]
        VisualIdentityOperationFailed = 93,
        [Description("Visual identity operation failed, wrong user identify")]
        VisualIdentityOperationFailedWrongUser = 94,
        [Description("Invalid Document Sending Method")]
        InvalidSendingMethod = 95,
        [Description("Contact data is insufficient")]
        InsufficientContactData = 96,
        [Description("Duplicate contact data")]
        DuplicateContactData = 97,
        [Description("User authentication mode is none")]
        InvalidUserAuthenticationMode = 98,
        [Description("Payment service is inactive")]
        PaymentServiceIsNotActive = 99,
        [Description("Invalid trasaction id")]
        InvalidTransactionId = 100,
        [Description("Collection sent is not a valid report collection")]
        InvalidObjectType = 101,
        [Description("Invalid date time")]
        InvalidDateTime = 102,
        [Description("Cannot delete the last user from company")]
        CantDeleteTheLastUserFromCompany = 103,
        [Description("Username already exists")]
        UsernameAlreadyExist = 104,
        [Description("Username or Email already exists")]
        UsernameOrEmailAlreadyExist = 105,
        [Description("Invalid SMS username or password Settings")]
        InvalidSMSUserNameOrPassword = 106,
        [Description("Missing setting for external PDF service.")]
        MissingSettingsForPDFExternalSettings = 107,
        [Description("Faild to merge file error from external service.")]
        FaildToMergeFileErrorFromExternalService = 108,
        [Description("Faild to merge file error from external service, Invalid credential")]
        FaildToMergeFileErrorFromExternalServiceInvalidCredential = 109,
        [Description("Failed to merge file error from external service, File Size exceeding allowed limit")]
        FileToMergeFileErrorFromExternalServiceFileSizeExceedingLimit = 110,
        [Description("Faild to post notification to CallBackUrl")]
        FailedToPostNotificationToCallbackUrl = 111,
        [Description("Visual identity operation failed, maximum amount of attempts reached")]
        VisualIdentityMaximumAttemptsReached = 112,
        [Description("Visual identifications exceed license limit")]
        VisualIdentificationsExceedLicenseLimit = 113,
        [Description("Invalid contacts sending group id")]
        InvalidContactsSendingGroupId = 114,
        [Description("Invalid contacts sending group member order")]
        InvalidContactsSendingGroupOrderOfTheMembers = 115,
        [Description("Contacts sending group max limit")]
        InvalidContactsSendingGroupMaxLimit = 116,
        [Description("Contacts sending group max limit member in group")]
        InvalidContactsSendingGroupMembersMaxLimit = 117,
        [Description("Contact in status deleted")]
        InvalidContactInStatusDeleted = 118,
        [Description("Invalid input")]
        InvalidInput = 119,
        [Description("Invalid contacts group name")]
        InvalidContactsGroupName = 120,
        [Description("Document not declined")]
        DocumentNotDeclined = 121,
        [Description("OTP Token is wrong or expired")]
        OTPTokenWrongOrExpired = 122,
        [Description("Wrong OTP code")]
        WrongOTPCode = 123,
        [Description("User is already connected to the group")]
        UserAlreadyConnectedToTheSelectedGroup = 124,
        [Description("User is not in token group")]
        UserNotInTokenGroup = 125,
        [Description("Password was already used before")]
        PasswordAlreadyUsed = 126,
        [Description("Password is expired")]
        PasswordExpired = 127,
        [Description("Password Is Too Short to company policy")]
        PasswordIsTooShortToCompanyPolicy = 128,
        [Description("Password session expired")]
        PasswordSessionExpired = 129,
        [Description("Invalid user id")]
        InvalidUserId = 130,
        [Description("Group does not belong to user")]
        GroupNotBelongToUser = 131,
        [Description("Old reports fetching failed")]
        OldReportsFetchFailed = 132,
        [Description("Failed to write deleted document collection in mongo db")]
        FailedWriteDeletedDocInMongo = 133,
        [Description("Missing setting for history integrator service")]
        MissingSettingsForHistoryIntegratorService = 134,
        [Description("External flow information does not exist")]
        ExternalFlowInfoNotExist = 135,
        [Description("Emails required to management periodic report")]
        EmailsRequiredToManagementPeriodicReports = 136,
        [Description("Periodic report file is expired")]
        PeriodicReportFileIsExpired = 137,
        [Description("Periodic report file does not exist")]
        PeriodicReportFileIsNotExists= 138,
        [Description("Failed to deserialize report parameters")]
        FailedToDeserializeReportParameters = 139,
        [Description("Failed to manage periodic report mail")]
        FailedCreateManagementPeriodicReportMail = 139,
        [Description("Failed to create a video conference. external service issue")]
        FailedToCreateVideoConfrence = 140,
        [Description("Free account cannot create a video conference")]
        FreeAccountsCannotCreateVideoConference = 141,
        [Description("User company not activate the video conference feature")]
        VideoConfrenceIsNotEnabled = 142,
        [Description("Video confrence number exceeds license limit")]
        VideoConfrenceExceedLicenseLimit = 143,
        [Description("Report frequency is required")]
        ReportFrequencyRequired = 144,
        [Description("Duplicated periodic report emails")]
        DuplicatedPeriodicReportEmails = 145,
        [Description("Signer should not contain both otp code and face recognition")]
        OtpAndFaceRecognitionSetup = 146,
        [Description("User cannot remove the group he connected to")]
        UserCannotRemoveConnectedGroup = 147,
        [Description("The selected phone already exists for this user")]
        SamePhoneExistAtTheSystem = 148,
        [Description("OTP submission limit exceeded")]
        OTPSubmissionLimitExceeded = 149,
        [Description("Password submission limit exceeded")]
        PasswordSubmissionLimitExceeded = 150,
        [Description("Signer authentication does not exist")]
        SignerAuthDoesntExist = 151,
        [Description("Success")]
        Success = 1000,
    }
}