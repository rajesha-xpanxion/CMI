using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source.AutoMon
{
    public class SQLQuery
    {
        public const string GET_TIME_ZONE = @"SELECT [ItemValue] FROM [dbo].[InstallationParam] WHERE [Name] = 'Time Zone'";
    }

    public class StoredProc
    {
        public const string GET_ALL_OFFENDER_DETAILS = "[dbo].[GetAllOffenderDetails]";
        public const string GET_ALL_OFFENDER_ADDRESS_DETAILS = "[dbo].[GetAllOffenderAddressDetails]";
        public const string GET_ALL_OFFENDER_PHONE_DETAILS = "[dbo].[GetAllOffenderPhoneDetails]";
        public const string GET_ALL_OFFENDER_EMAIL_DETAILS = "[dbo].[GetAllOffenderEmailDetails]";
        public const string GET_ALL_OFFENDER_CASE_DETAILS = "[dbo].[GetAllOffenderCaseDetails]";
        public const string GET_ALL_OFFENDER_NOTE_DETAILS = "[dbo].[GetAllOffenderNoteDetails]";
    }
}
