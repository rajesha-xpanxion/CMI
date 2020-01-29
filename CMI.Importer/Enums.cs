
namespace CMI.Importer
{
    public enum ClientProfileDataTemplateColumnName
    {
        [System.ComponentModel.Description("Integration Id")]
        IntegrationId = 0,

        [System.ComponentModel.Description("First Name")]
        FirstName,

        [System.ComponentModel.Description("Middle Name")]
        MiddleName,

        [System.ComponentModel.Description("Last Name")]
        LastName,

        [System.ComponentModel.Description("Client Type")]
        ClientType,

        [System.ComponentModel.Description("Time Zone")]
        TimeZone,

        [System.ComponentModel.Description("Gender")]
        Gender,

        [System.ComponentModel.Description("Ethnicity")]
        Ethnicity,

        [System.ComponentModel.Description("Date Of Birth")]
        DateOfBirth,

        [System.ComponentModel.Description("Supervising Officer")]
        SupervisingOfficer
    }

    public enum CaseDataTemplateColumnName
    {
        [System.ComponentModel.Description("Integration Id")]
        IntegrationId = 0,

        [System.ComponentModel.Description("Case Number")]
        CaseNumber,

        [System.ComponentModel.Description("Case Date")]
        CaseDate,

        [System.ComponentModel.Description("Status")]
        Status,

        [System.ComponentModel.Description("End Date")]
        EndDate,

        [System.ComponentModel.Description("Early Release Date")]
        EarlyReleaseDate,

        [System.ComponentModel.Description("End Reason")]
        EndReason
    }

    public enum AddressDataTemplateColumnName
    {
        [System.ComponentModel.Description("Integration Id")]
        IntegrationId = 0,

        [System.ComponentModel.Description("Address Id")]
        AddressId,

        [System.ComponentModel.Description("Full Address")]
        FullAddress,

        [System.ComponentModel.Description("Address Type")]
        AddressType,

        [System.ComponentModel.Description("Is Primary")]
        IsPrimary
    }

    public enum ContactDataTemplateColumnName
    {
        [System.ComponentModel.Description("Integration Id")]
        IntegrationId = 0,

        [System.ComponentModel.Description("Contact Id")]
        ContactId,

        [System.ComponentModel.Description("Contact Value")]
        ContactValue,

        [System.ComponentModel.Description("Contact Type")]
        ContactType,

        //[System.ComponentModel.Description("Is Primary")]
        //IsPrimary,

        [System.ComponentModel.Description("State Code")]
        StateCode
    }
}
