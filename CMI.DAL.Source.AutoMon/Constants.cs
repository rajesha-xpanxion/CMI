using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source.AutoMon
{
    public class Constants
    {
    }

    public class SQLParamName
    {
        public const string SOURCE_DATABASE_NAME = "@SourceDatabaseName";
        public const string LAST_EXECUTION_DATE_TIME = "@LastExecutionDateTime";
    }

    public class DBColumnName
    {
        public const string PIN = "Pin";
        public const string ID = "Id";
        public const string ADDRESS_TYPE = "AddressType";
        public const string LINE1 = "Line1";
        public const string LINE2 = "Line2";
        public const string CITY = "City";
        public const string STATE = "State";
        public const string ZIP = "Zip";
        public const string FIRST_NAME = "FirstName";
        public const string MIDDLE_NAME = "MiddleName";
        public const string LAST_NAME = "LastName";
        public const string DATE_OF_BIRTH = "DateOfBirth";
        public const string CLIENT_TYPE = "ClientType";
        public const string GENDER = "Gender";
        public const string RACE = "Race";
        public const string CASELOAD_NAME = "CaseloadName";
        public const string CASELOAD_TYPE = "CaseloadType";
        public const string OFFICER_LOGON = "OfficerLogon";
        public const string OFFICER_EMAIL = "OfficerEmail";
        public const string OFFICER_FIRST_NAME = "OfficerFirstName";
        public const string OFFICER_LAST_NAME = "OfficerLastName";
        public const string PHONE_NUMBER_TYPE = "PhoneNumberType";
        public const string PHONE = "Phone";
        public const string IS_PRIMARY = "IsPrimary";
        public const string COMMENT = "Comment";
        public const string CASE_NUMBER = "CaseNumber";
        public const string CASE_STATUS = "CaseStatus";
        public const string CLOSURE_REASON = "ClosureReason";
        public const string OFFENSE_LABEL = "OffenseLabel";
        public const string OFFENSE_STATUTE = "OffenseStatute";
        public const string OFFENSE_CATEGORY = "OffenseCategory";
        public const string OFFENSE_DATE = "OffenseDate";
        public const string CASE_DATE = "CaseDate";
        public const string SUPERVISION_START_DATE = "SupervisionStartDate";
        public const string SUPERVISION_END_DATE = "SupervisionEndDate";
        public const string EMAIL_ADDRESS = "EmailAddress";
        public const string DATE = "Date";
        public const string TEXT = "Text";
        public const string AUTHOR_EMAIL = "AuthorEmail";
        public const string IS_ACTIVE = "IsActive";
        public const string NOTE_TYPE = "NoteType";
    }
}
