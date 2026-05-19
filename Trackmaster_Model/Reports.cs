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


        public class StoppageMainModel
        {
            public List<StoppageSubModel> StoppageSubModel { get; set; }
            public int PageCount { get; set; }
        }


        public class StoppageSubModel
        {
            [DisplayName("Vehicle Name")]
            public string VehicleName { get; set; }
            [DisplayName("Driver Name")]
            public string DriverName { get; set; }

            [DisplayName("Stoppage Count")]
            public int StoppageCount { get; set; }
            [DisplayName("Total Stoppage Time (Days-hh-mm-ss)")]
            public string TotalStoppageTime { get; set; }
           
            public string BBID { get; set; }
           
            public string Type { get; set; }
            public List<StoppageAnalysis> objStoppageReport { get; set; }


        }
        public class StoppageAnalysis
        {
            [DisplayName("Start Date")]
            public string StartDate { get; set; }
            [DisplayName("Stop Date")]
            public string StopDate { get; set; }
            [DisplayName("Stop Location")]
            public string StopLocation { get; set; }
            [DisplayName("Duration(Days-hh-mm-ss)")]
            public string Duration { get; set; }
            [DisplayName("Ignition")]
            public Boolean IgnitionStatus { get; set; }

            [DisplayName("POI Location")]
            public string poi { get; set; }
            public string AddPoi { get; set; }
            
            public string StopLatitude { get; set; }
            
            public string StopLongitude { get; set; }
           
            public string VehicleName { get; set; }
           }

        public class ReportBase 
        {
            public string DriverName { get; set; }
            public string TotalDistance { get; set; }
            public string VehicleName { get; set; }
            public string BBID { get; set; }
            public string VehicleRegNo { get; set; }
            public string TotalIgnitionOn { get; set; }
            public string TotalMachWorking { get; set; }
            public double dbl_totalWorkinghrs { get; set; }
            public double dbl_idelingWorkinghrs { get; set; }
            public double dbl_StopWorkinghrs { get; set; }
            //this attribute is used for storing report date(in pdf)
            public string ReportDate { get; set; }
            public string TotalStoppageTime { get; set; }
        }
    }
}
