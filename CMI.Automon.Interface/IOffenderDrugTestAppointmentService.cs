using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Interface
{
    public interface IOffenderDrugTestAppointmentService
    {
        void SaveOffenderDrugTestAppointmentDetails(string CmiDbConnString, OffenderDrugTestAppointment offenderDrugTestAppointmentDetails);
    }
}
