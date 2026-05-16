using Microsoft.AspNetCore.Http;
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
            public int Custid { get; set; }
            public int? EmployeeId { get; set; }

            public short Designation { get; set; }
            public string EmployeeCode { get; set; }
            public string EmployeeType { get; set; }
            public int? contractDuration { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ?Qualification { get; set; }
            public string ?Experience { get; set; }
            public string ?PermanentAddress { get; set; }
            public string ?PermanentPostalCode { get; set; }
            public string? PermanentState { get; set; }
            public string ?PermanentCity { get; set; }
            public string ?correspondenceAddress { get; set; }
            public string ?correspondencePostalCode { get; set; }
            public string ?correspondenceState { get; set; }
            public string ?correspondenceCity { get; set; }
            public string ?HireDate { get; set; }
            public double ?EmployeeCTC { get; set; }
            public string ?Role { get; set; }
            public string ?OfficePhone { get; set; }
            public string ?EmergencyContactInfo { get; set; }
            public string ?Mobile { get; set; }
            public string ?IdProofNo { get; set; }
            public string ?IdProofType { get; set; }
            public string ?Remarks { get; set; }
            public string ?BloodGroup { get; set; }
            public List<IFormFile>? ImageFiles { get; set; }
        }

        public class DocInfo
        {
            public string Name { get; set; }
            public string fullPath { get; set; }
        }
    }
}
