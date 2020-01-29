using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace CMI.Importer.DAL
{
    public class ImporterProvider : IImporterProvider
    {
        #region Private Member Variables
        private readonly ImporterConfig importerConfig;
        #endregion

        #region Constructor
        public ImporterProvider(
            IOptions<ImporterConfig> importerConfig
        )
        {
            this.importerConfig = importerConfig.Value;
        }
        #endregion

        #region Public Methods
        public IEnumerable<ClientProfileDetails> RetrieveClientProfileDataFromExcel()
        {
            List<ClientProfileDetails> clientProfiles = new List<ClientProfileDetails>();

            ISheet dataSheet;

            string dataFileExt = Path.GetExtension(importerConfig.InboundImporterConfig.SourceDataFileFullPath);

            using (var stream = new FileStream(importerConfig.InboundImporterConfig.SourceDataFileFullPath, FileMode.Open))
            {
                //set start position
                stream.Position = 0;
                if (dataFileExt.Equals(ExcelSheetExtensionType.Xls, StringComparison.InvariantCultureIgnoreCase))
                {
                    HSSFWorkbook workbook = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                    dataSheet = workbook.GetSheet(InboundImporterStage.ClientProfiles); //get data sheet from workbook  
                }
                else
                {
                    XSSFWorkbook workbook = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                    dataSheet = workbook.GetSheet(InboundImporterStage.ClientProfiles); //get data sheet from workbook   
                }
                //throw exception when datasheet is null
                if (dataSheet == null)
                {
                    throw new Exception(string.Format("Sheet {0} not found.", InboundImporterStage.ClientProfiles));
                }

                //iterate through data and populate in object
                for (int dataRowCounter = (dataSheet.FirstRowNum + 1); dataRowCounter <= dataSheet.LastRowNum; dataRowCounter++) //Read actual data
                {
                    IRow currentDataRow = dataSheet.GetRow(dataRowCounter);

                    //check if row is null or all cell values in row are blank, skip row processing in such case
                    if (currentDataRow == null)
                    {
                        continue;
                    }
                    //if row is blank skip row processing
                    if (currentDataRow.Cells.All(d => d.CellType == CellType.Blank))
                    {
                        continue;
                    }

                    ICell dataCellIntegrationId = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.IntegrationId);
                    ICell dataCellFirstName = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.FirstName);
                    ICell dataCellMiddleName = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.MiddleName);
                    ICell dataCellLastName = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.LastName);
                    ICell dataCellClientType = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.ClientType);
                    ICell dataCellTimeZone = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.TimeZone);
                    ICell dataCellGender = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.Gender);
                    ICell dataCellEthnicity = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.Ethnicity);
                    ICell dataCellDateOfBirth = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.DateOfBirth);
                    ICell dataCellSupervisingOfficer = currentDataRow.GetCell((int)ClientProfileDataTemplateColumnName.SupervisingOfficer);

                    clientProfiles.Add(new ClientProfileDetails
                    {
                        IntegrationId = (
                            dataCellIntegrationId == null || dataCellIntegrationId.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellIntegrationId.ToString())
                            ? null
                            : dataCellIntegrationId.ToString()
                        ),
                        FirstName = (
                            dataCellFirstName == null || dataCellFirstName.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellFirstName.ToString())
                            ? null
                            : dataCellFirstName.ToString()
                        ),
                        MiddleName = (
                            dataCellMiddleName == null || dataCellMiddleName.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellMiddleName.ToString())
                            ? null
                            : dataCellMiddleName.ToString()
                        ),
                        LastName = (
                            dataCellLastName == null || dataCellLastName.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellLastName.ToString())
                            ? null
                            : dataCellLastName.ToString()
                        ),
                        ClientType = (
                            dataCellClientType == null || dataCellClientType.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellClientType.ToString())
                            ? null
                            : dataCellClientType.ToString()
                        ),
                        TimeZone = (
                            dataCellTimeZone == null || dataCellTimeZone.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellTimeZone.ToString())
                            ? null
                            : dataCellTimeZone.ToString()
                        ),
                        Gender = (
                            dataCellGender == null || dataCellGender.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellGender.ToString())
                            ? null
                            : dataCellGender.ToString()
                        ),
                        Ethnicity = (
                            dataCellEthnicity == null || dataCellEthnicity.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellEthnicity.ToString())
                            ? null
                            : dataCellEthnicity.ToString()
                        ),
                        DateOfBirth = (
                            dataCellDateOfBirth == null || dataCellDateOfBirth.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellDateOfBirth.ToString())
                            ? null
                            : dataCellDateOfBirth.ToString()
                        ),
                        SupervisingOfficerEmailId = (
                            dataCellSupervisingOfficer == null || dataCellSupervisingOfficer.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellSupervisingOfficer.ToString())
                            ? null
                            : dataCellSupervisingOfficer.ToString()
                        )
                    });
                }
            }

            return clientProfiles;
        }

        public IEnumerable<ContactDetails> RetrieveContactDataFromExcel()
        {
            List<ContactDetails> contacts = new List<ContactDetails>();

            ISheet dataSheet;

            string dataFileExt = Path.GetExtension(importerConfig.InboundImporterConfig.SourceDataFileFullPath);

            using (var stream = new FileStream(importerConfig.InboundImporterConfig.SourceDataFileFullPath, FileMode.Open))
            {
                //set start position
                stream.Position = 0;
                if (dataFileExt.Equals(ExcelSheetExtensionType.Xls, StringComparison.InvariantCultureIgnoreCase))
                {
                    HSSFWorkbook workbook = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                    dataSheet = workbook.GetSheet(InboundImporterStage.Contacts); //get data sheet from workbook  
                }
                else
                {
                    XSSFWorkbook workbook = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                    dataSheet = workbook.GetSheet(InboundImporterStage.Contacts); //get data sheet from workbook   
                }
                //throw exception when datasheet is null
                if (dataSheet == null)
                {
                    throw new Exception(string.Format("Sheet {0} not found.", InboundImporterStage.Contacts));
                }

                //iterate through data and populate in object
                for (int dataRowCounter = (dataSheet.FirstRowNum + 1); dataRowCounter <= dataSheet.LastRowNum; dataRowCounter++) //Read actual data
                {
                    IRow currentDataRow = dataSheet.GetRow(dataRowCounter);

                    //check if row is null or all cell values in row are blank, skip row processing in such case
                    if (currentDataRow == null)
                    {
                        continue;
                    }
                    //if row is blank skip row processing
                    if (currentDataRow.Cells.All(d => d.CellType == CellType.Blank))
                    {
                        continue;
                    }

                    ICell dataCellIntegrationId = currentDataRow.GetCell((int)ContactDataTemplateColumnName.IntegrationId);
                    ICell dataCellContactId = currentDataRow.GetCell((int)ContactDataTemplateColumnName.ContactId);
                    ICell dataCellContactValue = currentDataRow.GetCell((int)ContactDataTemplateColumnName.ContactValue);
                    ICell dataCellContactType = currentDataRow.GetCell((int)ContactDataTemplateColumnName.ContactType);
                    ICell dataCellStateCode = currentDataRow.GetCell((int)ContactDataTemplateColumnName.StateCode);


                    contacts.Add(new ContactDetails
                    {
                        IntegrationId = (
                            dataCellIntegrationId == null || dataCellIntegrationId.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellIntegrationId.ToString())
                            ? null
                            : dataCellIntegrationId.ToString()
                        ),
                        ContactId = (
                            dataCellContactId == null || dataCellContactId.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellContactId.ToString())
                            ? null
                            : dataCellContactId.ToString()
                        ),
                        ContactValue = (
                            dataCellContactValue == null || dataCellContactValue.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellContactValue.ToString())
                            ? null
                            : dataCellContactValue.ToString()
                        ),
                        ContactType = (
                            dataCellContactType == null || dataCellContactType.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellContactType.ToString())
                            ? null
                            : dataCellContactType.ToString()
                        )
                    });
                }
            }

            return contacts;
        }

        public IEnumerable<AddressDetails> RetrieveAddressDataFromExcel()
        {
            List<AddressDetails> addresses = new List<AddressDetails>();

                ISheet dataSheet;

                string dataFileExt = Path.GetExtension(importerConfig.InboundImporterConfig.SourceDataFileFullPath);

                using (var stream = new FileStream(importerConfig.InboundImporterConfig.SourceDataFileFullPath, FileMode.Open))
                {
                    //set start position
                    stream.Position = 0;
                    if (dataFileExt.Equals(ExcelSheetExtensionType.Xls, StringComparison.InvariantCultureIgnoreCase))
                    {
                        HSSFWorkbook workbook = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                        dataSheet = workbook.GetSheet(InboundImporterStage.Addresses); //get data sheet from workbook  
                    }
                    else
                    {
                        XSSFWorkbook workbook = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                        dataSheet = workbook.GetSheet(InboundImporterStage.Addresses); //get data sheet from workbook   
                    }
                    //throw exception when datasheet is null
                    if (dataSheet == null)
                    {
                        throw new Exception(string.Format("Sheet {0} not found.", InboundImporterStage.Addresses));
                    }

                    //iterate through data and populate in object
                    for (int dataRowCounter = (dataSheet.FirstRowNum + 1); dataRowCounter <= dataSheet.LastRowNum; dataRowCounter++) //Read actual data
                    {
                        IRow currentDataRow = dataSheet.GetRow(dataRowCounter);

                        //check if row is null or all cell values in row are blank, skip row processing in such case
                        if (currentDataRow == null)
                        {
                            continue;
                        }
                        //if row is blank skip row processing
                        if (currentDataRow.Cells.All(d => d.CellType == CellType.Blank))
                        {
                            continue;
                        }

                        ICell dataCellIntegrationId = currentDataRow.GetCell((int)AddressDataTemplateColumnName.IntegrationId);
                        ICell dataCellAddressId = currentDataRow.GetCell((int)AddressDataTemplateColumnName.AddressId);
                        ICell dataCellFullAddress = currentDataRow.GetCell((int)AddressDataTemplateColumnName.FullAddress);
                        ICell dataCellAddressType = currentDataRow.GetCell((int)AddressDataTemplateColumnName.AddressType);
                        ICell dataCellIsPrimary = currentDataRow.GetCell((int)AddressDataTemplateColumnName.IsPrimary);


                        addresses.Add(new AddressDetails
                        {
                            IntegrationId = (
                                dataCellIntegrationId == null || dataCellIntegrationId.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellIntegrationId.ToString())
                                ? null
                                : dataCellIntegrationId.ToString()
                            ),
                            AddressId = (
                                dataCellAddressId == null || dataCellAddressId.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellAddressId.ToString())
                                ? null
                                : dataCellAddressId.ToString()
                            ),
                            FullAddress = (
                                dataCellFullAddress == null || dataCellFullAddress.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellFullAddress.ToString())
                                ? null
                                : dataCellFullAddress.ToString()
                            ),
                            AddressType = (
                                dataCellAddressType == null || dataCellAddressType.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellAddressType.ToString())
                                ? null
                                : dataCellAddressType.ToString()
                            ),
                            IsPrimary = (
                                dataCellIsPrimary == null || dataCellIsPrimary.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellIsPrimary.ToString())
                                ? null
                                : dataCellIsPrimary.ToString()
                            )
                        });
                    }
                }
            
            return addresses;
        }

        public IEnumerable<CourtCaseDetails> RetrieveCourtCaseDataFromExcel()
        {
            List<CourtCaseDetails> cases = new List<CourtCaseDetails>();

            ISheet dataSheet;

            string dataFileExt = Path.GetExtension(importerConfig.InboundImporterConfig.SourceDataFileFullPath);

            using (var stream = new FileStream(importerConfig.InboundImporterConfig.SourceDataFileFullPath, FileMode.Open))
            {
                //set start position
                stream.Position = 0;
                if (dataFileExt.Equals(ExcelSheetExtensionType.Xls, StringComparison.InvariantCultureIgnoreCase))
                {
                    HSSFWorkbook workbook = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                    dataSheet = workbook.GetSheet(InboundImporterStage.CourtCases); //get data sheet from workbook  
                }
                else
                {
                    XSSFWorkbook workbook = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                    dataSheet = workbook.GetSheet(InboundImporterStage.CourtCases); //get data sheet from workbook   
                }
                //throw exception when datasheet is null
                if (dataSheet == null)
                {
                    throw new Exception(string.Format("Sheet {0} not found.", InboundImporterStage.CourtCases));
                }

                //iterate through data and populate in object
                for (int dataRowCounter = (dataSheet.FirstRowNum + 1); dataRowCounter <= dataSheet.LastRowNum; dataRowCounter++) //Read actual data
                {
                    IRow currentDataRow = dataSheet.GetRow(dataRowCounter);

                    //check if row is null or all cell values in row are blank, skip row processing in such case
                    if (currentDataRow == null)
                    {
                        continue;
                    }
                    //if row is blank skip row processing
                    if (currentDataRow.Cells.All(d => d.CellType == CellType.Blank))
                    {
                        continue;
                    }

                    ICell dataCellIntegrationId = currentDataRow.GetCell((int)CourtCaseDataTemplateColumnName.IntegrationId);
                    ICell dataCellCaseNumber = currentDataRow.GetCell((int)CourtCaseDataTemplateColumnName.CaseNumber);
                    ICell dataCellCaseDate = currentDataRow.GetCell((int)CourtCaseDataTemplateColumnName.CaseDate);
                    ICell dataCellStatus = currentDataRow.GetCell((int)CourtCaseDataTemplateColumnName.Status);
                    ICell dataCellEndDate = currentDataRow.GetCell((int)CourtCaseDataTemplateColumnName.EndDate);
                    ICell dataCellEarlyReleaseDate = currentDataRow.GetCell((int)CourtCaseDataTemplateColumnName.EarlyReleaseDate);
                    ICell dataCellEndReason = currentDataRow.GetCell((int)CourtCaseDataTemplateColumnName.EndReason);


                    cases.Add(new CourtCaseDetails
                    {
                        IntegrationId = (
                            dataCellIntegrationId == null || dataCellIntegrationId.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellIntegrationId.ToString())
                            ? null
                            : dataCellIntegrationId.ToString()
                        ),
                        CaseNumber = (
                            dataCellCaseNumber == null || dataCellCaseNumber.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellCaseNumber.ToString())
                            ? null
                            : dataCellCaseNumber.ToString()
                        ),
                        CaseDate = (
                            dataCellCaseDate == null || dataCellCaseDate.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellCaseDate.ToString())
                            ? null
                            : dataCellCaseDate.ToString()
                        ),
                        Status = (
                            dataCellStatus == null || dataCellStatus.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellStatus.ToString())
                            ? null
                            : dataCellStatus.ToString()
                        ),
                        EndDate = (
                            dataCellEndDate == null || dataCellEndDate.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellEndDate.ToString())
                            ? null
                            : dataCellEndDate.ToString()
                        ),
                        EarlyReleaseDate = (
                            dataCellEarlyReleaseDate == null || dataCellEarlyReleaseDate.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellEarlyReleaseDate.ToString())
                            ? null
                            : dataCellEarlyReleaseDate.ToString()
                        ),
                        EndReason = (
                            dataCellEndReason == null || dataCellEndReason.CellType == CellType.Blank || string.IsNullOrEmpty(dataCellEndReason.ToString())
                            ? null
                            : dataCellEndReason.ToString()
                        )
                    });
                }
            }

            return cases;
        }

        public IEnumerable<ClientProfileDetails> SaveClientProfilesToDatabase(IEnumerable<ClientProfileDetails> receivedClientProfiles)
        {
            List<ClientProfileDetails> savedClientProfiles = new List<ClientProfileDetails>();

            using (SqlConnection conn = new SqlConnection(importerConfig.CmiDbConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = StoredProc.SaveClientProfiles;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    var dataTable = new DataTable(UserDefinedTableType.ClientProfileTbl)
                    {
                        Locale = CultureInfo.InvariantCulture
                    };

                    dataTable.Columns.Add(TableColumnName.Id, typeof(int));
                    dataTable.Columns.Add(TableColumnName.IsImportSuccessful, typeof(bool));
                    dataTable.Columns.Add(TableColumnName.IntegrationId, typeof(string));
                    dataTable.Columns.Add(TableColumnName.FirstName, typeof(string));
                    dataTable.Columns.Add(TableColumnName.MiddleName, typeof(string));
                    dataTable.Columns.Add(TableColumnName.LastName, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ClientType, typeof(string));
                    dataTable.Columns.Add(TableColumnName.TimeZone, typeof(string));
                    dataTable.Columns.Add(TableColumnName.Gender, typeof(string));
                    dataTable.Columns.Add(TableColumnName.Ethnicity, typeof(string));
                    dataTable.Columns.Add(TableColumnName.DateOfBirth, typeof(string));
                    dataTable.Columns.Add(TableColumnName.SupervisingOfficerEmailId, typeof(string));

                    //check for null & check if any record to process
                    if (receivedClientProfiles != null && receivedClientProfiles.Any())
                    {
                        foreach (var clientProfileDetails in receivedClientProfiles)
                        {
                            dataTable.Rows.Add(
                                clientProfileDetails.Id,
                                clientProfileDetails.IsImportSuccessful,
                                clientProfileDetails.IntegrationId,
                                string.IsNullOrEmpty(clientProfileDetails.FirstName) ? null : clientProfileDetails.FirstName,
                                string.IsNullOrEmpty(clientProfileDetails.MiddleName) ? null : clientProfileDetails.MiddleName,
                                string.IsNullOrEmpty(clientProfileDetails.LastName) ? null : clientProfileDetails.LastName,
                                string.IsNullOrEmpty(clientProfileDetails.ClientType) ? null : clientProfileDetails.ClientType,
                                string.IsNullOrEmpty(clientProfileDetails.TimeZone) ? null : clientProfileDetails.TimeZone,
                                string.IsNullOrEmpty(clientProfileDetails.Gender) ? null : clientProfileDetails.Gender,
                                string.IsNullOrEmpty(clientProfileDetails.Ethnicity) ? null : clientProfileDetails.Ethnicity,
                                string.IsNullOrEmpty(clientProfileDetails.DateOfBirth) ? null : clientProfileDetails.DateOfBirth,
                                string.IsNullOrEmpty(clientProfileDetails.SupervisingOfficerEmailId) ? null : clientProfileDetails.SupervisingOfficerEmailId
                            );
                        }
                    }
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = SqlParamName.ClientProfileTbl,
                        Value = dataTable,
                        SqlDbType = SqlDbType.Structured,
                        Direction = ParameterDirection.Input
                    });

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            savedClientProfiles.Add(new ClientProfileDetails
                            {
                                Id = Convert.ToInt32(reader[TableColumnName.Id]),
                                IsImportSuccessful = Convert.ToBoolean(reader[TableColumnName.IsImportSuccessful]),
                                IntegrationId = Convert.ToString(reader[TableColumnName.IntegrationId]),

                                FirstName = Convert.ToString(reader[TableColumnName.FirstName]),
                                MiddleName= Convert.ToString(reader[TableColumnName.MiddleName]),
                                LastName = Convert.ToString(reader[TableColumnName.LastName]),
                                ClientType = Convert.ToString(reader[TableColumnName.ClientType]),
                                TimeZone = Convert.ToString(reader[TableColumnName.TimeZone]),
                                Gender = Convert.ToString(reader[TableColumnName.Gender]),
                                Ethnicity = Convert.ToString(reader[TableColumnName.Ethnicity]),
                                DateOfBirth = Convert.ToString(reader[TableColumnName.DateOfBirth]),
                                SupervisingOfficerEmailId = Convert.ToString(reader[TableColumnName.SupervisingOfficerEmailId])
                            });
                        }
                    }
                }
            }

            return savedClientProfiles;
        }

        public IEnumerable<ContactDetails> SaveContactsToDatabase(IEnumerable<ContactDetails> receivedContacts)
        {
            List<ContactDetails> savedContacts = new List<ContactDetails>();

            using (SqlConnection conn = new SqlConnection(importerConfig.CmiDbConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = StoredProc.SaveContacts;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    var dataTable = new DataTable(UserDefinedTableType.ContactTbl)
                    {
                        Locale = CultureInfo.InvariantCulture
                    };

                    dataTable.Columns.Add(TableColumnName.Id, typeof(int));
                    dataTable.Columns.Add(TableColumnName.IsImportSuccessful, typeof(bool));
                    dataTable.Columns.Add(TableColumnName.IntegrationId, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ContactId, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ContactValue, typeof(string));
                    dataTable.Columns.Add(TableColumnName.ContactType, typeof(string));

                    //check for null & check if any record to process
                    if (receivedContacts != null && receivedContacts.Any())
                    {
                        foreach (var contactDetails in receivedContacts)
                        {
                            dataTable.Rows.Add(
                                contactDetails.Id,
                                contactDetails.IsImportSuccessful,
                                contactDetails.IntegrationId,
                                contactDetails.ContactId,
                                string.IsNullOrEmpty(contactDetails.ContactValue) ? null : contactDetails.ContactValue,
                                string.IsNullOrEmpty(contactDetails.ContactType) ? null : contactDetails.ContactType
                            );
                        }
                    }
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = SqlParamName.ContactTbl,
                        Value = dataTable,
                        SqlDbType = SqlDbType.Structured,
                        Direction = ParameterDirection.Input
                    });

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            savedContacts.Add(new ContactDetails
                            {
                                Id = Convert.ToInt32(reader[TableColumnName.Id]),
                                IsImportSuccessful = Convert.ToBoolean(reader[TableColumnName.IsImportSuccessful]),
                                IntegrationId = Convert.ToString(reader[TableColumnName.IntegrationId]),
                                ContactId = Convert.ToString(reader[TableColumnName.ContactId]),
                                ContactValue = Convert.ToString(reader[TableColumnName.ContactValue]),
                                ContactType = Convert.ToString(reader[TableColumnName.ContactType])
                            });
                        }
                    }
                }
            }

            return savedContacts;
        }

        public IEnumerable<AddressDetails> SaveAddressesToDatabase(IEnumerable<AddressDetails> receivedAddresses)
        {
            List<AddressDetails> savedAddresses = new List<AddressDetails>();

            using (SqlConnection conn = new SqlConnection(importerConfig.CmiDbConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = StoredProc.SaveAddresses;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    var dataTable = new DataTable(UserDefinedTableType.AddressTbl)
                    {
                        Locale = CultureInfo.InvariantCulture
                    };

                    dataTable.Columns.Add(TableColumnName.Id, typeof(int));
                    dataTable.Columns.Add(TableColumnName.IsImportSuccessful, typeof(bool));
                    dataTable.Columns.Add(TableColumnName.IntegrationId, typeof(string));
                    dataTable.Columns.Add(TableColumnName.AddressId, typeof(string));
                    dataTable.Columns.Add(TableColumnName.FullAddress, typeof(string));
                    dataTable.Columns.Add(TableColumnName.AddressType, typeof(string));
                    dataTable.Columns.Add(TableColumnName.IsPrimary, typeof(string));

                    //check for null & check if any record to process
                    if (receivedAddresses != null && receivedAddresses.Any())
                    {
                        foreach (var addressDetails in receivedAddresses)
                        {
                            dataTable.Rows.Add(
                                addressDetails.Id,
                                addressDetails.IsImportSuccessful,
                                addressDetails.IntegrationId,
                                addressDetails.AddressId,
                                string.IsNullOrEmpty(addressDetails.FullAddress) ? null : addressDetails.FullAddress,
                                string.IsNullOrEmpty(addressDetails.AddressType) ? null : addressDetails.AddressType,
                                string.IsNullOrEmpty(addressDetails.IsPrimary) ? null : addressDetails.IsPrimary
                            );
                        }
                    }
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = SqlParamName.AddressTbl,
                        Value = dataTable,
                        SqlDbType = SqlDbType.Structured,
                        Direction = ParameterDirection.Input
                    });

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            savedAddresses.Add(new AddressDetails
                            {
                                Id = Convert.ToInt32(reader[TableColumnName.Id]),
                                IsImportSuccessful = Convert.ToBoolean(reader[TableColumnName.IsImportSuccessful]),
                                IntegrationId = Convert.ToString(reader[TableColumnName.IntegrationId]),
                                AddressId = Convert.ToString(reader[TableColumnName.AddressId]),
                                FullAddress = Convert.ToString(reader[TableColumnName.FullAddress]),
                                AddressType = Convert.ToString(reader[TableColumnName.AddressType]),
                                IsPrimary = Convert.ToString(reader[TableColumnName.IsPrimary])
                            });
                        }
                    }
                }
            }

            return savedAddresses;
        }

        public IEnumerable<CourtCaseDetails> SaveCourtCasesToDatabase(IEnumerable<CourtCaseDetails> receivedCourtCases)
        {
            List<CourtCaseDetails> savedCourtCases = new List<CourtCaseDetails>();

            using (SqlConnection conn = new SqlConnection(importerConfig.CmiDbConnString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = StoredProc.SaveCourtCases;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    var dataTable = new DataTable(UserDefinedTableType.CourtCaseTbl)
                    {
                        Locale = CultureInfo.InvariantCulture
                    };

                    dataTable.Columns.Add(TableColumnName.Id, typeof(int));
                    dataTable.Columns.Add(TableColumnName.IsImportSuccessful, typeof(bool));
                    dataTable.Columns.Add(TableColumnName.IntegrationId, typeof(string));
                    dataTable.Columns.Add(TableColumnName.CaseNumber, typeof(string));
                    dataTable.Columns.Add(TableColumnName.CaseDate, typeof(string));
                    dataTable.Columns.Add(TableColumnName.Status, typeof(string));
                    dataTable.Columns.Add(TableColumnName.EndDate, typeof(string));
                    dataTable.Columns.Add(TableColumnName.EarlyReleaseDate, typeof(string));
                    dataTable.Columns.Add(TableColumnName.EndReason, typeof(string));

                    //check for null & check if any record to process
                    if (receivedCourtCases != null && receivedCourtCases.Any())
                    {
                        foreach (var courtCaseDetails in receivedCourtCases)
                        {
                            dataTable.Rows.Add(
                                courtCaseDetails.Id,
                                courtCaseDetails.IsImportSuccessful,
                                courtCaseDetails.IntegrationId,
                                courtCaseDetails.CaseNumber,
                                string.IsNullOrEmpty(courtCaseDetails.CaseDate) ? null : courtCaseDetails.CaseDate,
                                string.IsNullOrEmpty(courtCaseDetails.Status) ? null : courtCaseDetails.Status,
                                string.IsNullOrEmpty(courtCaseDetails.EndDate) ? null : courtCaseDetails.EndDate,
                                string.IsNullOrEmpty(courtCaseDetails.EarlyReleaseDate) ? null : courtCaseDetails.EarlyReleaseDate,
                                string.IsNullOrEmpty(courtCaseDetails.EndDate) ? null : courtCaseDetails.EndDate
                            );
                        }
                    }
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = SqlParamName.CourtCaseTbl,
                        Value = dataTable,
                        SqlDbType = SqlDbType.Structured,
                        Direction = ParameterDirection.Input
                    });

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            savedCourtCases.Add(new CourtCaseDetails
                            {
                                Id = Convert.ToInt32(reader[TableColumnName.Id]),
                                IsImportSuccessful = Convert.ToBoolean(reader[TableColumnName.IsImportSuccessful]),
                                IntegrationId = Convert.ToString(reader[TableColumnName.IntegrationId]),
                                CaseNumber = Convert.ToString(reader[TableColumnName.CaseNumber]),
                                CaseDate = Convert.ToString(reader[TableColumnName.CaseDate]),
                                Status = Convert.ToString(reader[TableColumnName.Status]),
                                EndDate = Convert.ToString(reader[TableColumnName.EndDate]),
                                EarlyReleaseDate = Convert.ToString(reader[TableColumnName.EarlyReleaseDate]),
                                EndReason = Convert.ToString(reader[TableColumnName.EndReason])
                            });
                        }
                    }
                }
            }

            return savedCourtCases;
        }
        #endregion
    }
}
