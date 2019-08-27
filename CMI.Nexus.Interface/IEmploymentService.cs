using CMI.Nexus.Model;
using System.Collections.Generic;

namespace CMI.Nexus.Interface
{
    public interface IEmploymentService
    {
        bool AddNewEmploymentDetails(Employment vehicle);

        Employment GetEmploymentDetails(string clientId, string employmentId);

        List<Employment> GetAllEmploymentDetails(string clientId);

        bool UpdateEmploymentDetails(Employment employment);

        bool DeleteEmploymentDetails(string clientId, string employmentId);
    }
}
