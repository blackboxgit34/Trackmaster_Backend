using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class Reports
    {
        public class VehiclesReport
        {
            public int PageCount { get; set; }
            public List<VehicleInformation> modelObjList { get; set; }
        }
        public class VehicleInformation 
        {
            public string VehicleName { get; set; }
            public string driverName { get; set; }
            public string ConductorName { get; set; }
            public string VehicleType { get; set; }
            public string VehicleImagePath { get; set; }
            public int TotalRecords { get; set; }
            public string BBID { get; set; }

        }

        public class DropDownItems
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        public class EmployeeInfo
        {
            public List<Employee> EmployeeList { get; set; }
            public int PageCount { get; set; }
        }

        public class Employee
        {
            [DisplayName("Employee Code")]
            public string EmployeeCode { get; set; }//
            [DisplayName("First Name")]

            public string FirstName { get; set; }//
            [DisplayName("EmployeeType")]
            public string EmployeeType { get; set; }
            [DisplayName("Last Name")]
            public string LastName { get; set; }//
            [DisplayName("Permanent Address")]
            public string PermanentAddress { get; set; }//
            [DisplayName("Permanent Address Postal Code")]
            public string PermanentPostalCode { get; set; }//
            [DisplayName("Permanent Address State")]
            public string PermanentState { get; set; }//
            [DisplayName("Permanent Address City")]
            public string PermanentCity { get; set; }//
            [DisplayName("Correspondence Address")]
            public string Address { get; set; }
            [DisplayName("Correspondence Address Postal Code")]
            public string PostalCode { get; set; }//
            [DisplayName("Correspondence Address State")]
            public string State { get; set; }//
            [DisplayName("Correspondence Address City")]
            public string City { get; set; }//
            [DisplayName("Mobile")] //
            public string Mobile { get; set; }
            [DisplayName("Blood Group")]
            public string BloodGroup { get; set; }//
            [DisplayName("Status")]
            public string Status { get; set; }
            [DisplayName("Hire Date")]
            public string HireDate { get; set; }//
            [DisplayName("Employee CTC")]
            public double EmployeeCTC { get; set; }//
            [DisplayName("Qualification")]
            public string Qualification { get; set; }//
            [DisplayName("Experience")]
            public string Experience { get; set; }//
            [DisplayName("Emergency Contact Info")]
            public string EmergencyContactInfo { get; set; }//
            
            [DisplayName("ETMNo")]
            public string ETMNo { get; set; }//
            public int contractDuration { get; set; }//
            public int Custid { get; set; }//
            public string EmployeeID { get; set; } //
            public string OfficePhone { get; set; }//
            public string EmployeeTypeId { get; set; }//
            public string ImagePath { get; set; }//
            public string ImageFileName { get; set; }
            public string DrivingLicenseNo { get; set; }//
            public string LicenseExpiryDate { get; set; }//
            public string DriverCertifications { get; set; }//
            public string Remarks { get; set; }//
            public string TechnicianCertifications { get; set; }//
            public string AttachmentsPath { get; set; }//
            public string AttachmentsFileName { get; set; }
            public string IdProof { get; set; }//
            public string RoleResponisbility { get; set; }//
        }
    }
}
