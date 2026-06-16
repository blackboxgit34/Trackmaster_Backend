using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Trackmaster_Model.Reports;


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

        // neha k 
        public class SMSReport 
        {
            [DisplayName("Vehicle Name")]
            public string VehicleName { get; set; }//           
            public string MessageDate { get; set; }//           
            public string MessageType { get; set; }//          
            public string Mobile { get; set; }//         
            public string MessageText { get; set; }//           
            public string androidstatus { get; set; }//           
            public string iosstatus { get; set; }//          
            public string BBID { get; set; } //      
            public int fmsVehicleId { get; set; } //           
        }
        public class SMSReportEx
        {
            public List<SMSReport> objSMSReport { get; set; }
            public int pagecount { get; set; }
        }
       
        //neha  k 
        public class IgnitionStatus
        {
            public string IgnitionOnTime { get; set; }
            public string IgnitionOffTime { get; set; }
            public string SLocation { get; set; }
            public string ELocation { get; set; }
            public string Duration { get; set; }
        }
        public class IgnitionStatusEx
        {
            public string VehicleName { get; set; }
            public string DriverName { get; set; }
            public string IgnitionOnOffCounter { get; set; }
            public string TotalIgnitionTime { get; set; }
            public int custid { get; set; }
            public string bbid { get; set; }
            public List<IgnitionStatus> objIgnitionStatusReport { get; set; }
        }

        public class ConsolidatedIgnitionModel
        {
            public int PageCount { get; set; }
            public List<IgnitionStatusEx> ConsolidatedIgnitionList { get; set; }
        }



        public class VehicleStatusResponse
        {
            public int ItemCount { get; set; }
            public List<VehicleStatusDto> VehicleData { get; set; } = new();
        }

        public class VehicleStatusDto
        {
            public int RowNo { get; set; }
            public string BBID { get; set; }
            public string VehName { get; set; }
            public string DriverName { get; set; }
            public int Overspeed { get; set; }

            public List<SpeedLogDto> Logs { get; set; } = new List<SpeedLogDto>(); // ✅ SAFE INIT
        }

        public class SpeedLogDto
        {
            public DateTime? Time { get; set; }
            public string Speed { get; set; }
            public string Location { get; set; }
            public string Status { get; set; }           
        }
        public class BatteryDisconnectionResponse
        {
            public int ItemCount { get; set; }

            public List<BatteryDisconnectionDto> VehicleData { get; set; } = new();
        }

        public class BatteryDisconnectionDto
        {
            public int RowNo { get; set; }

            public string BBID { get; set; }

            public string VehName { get; set; }

            public List<BatteryDisconnectionLogDto> Logs { get; set; } = new();
        }

        public class BatteryDisconnectionLogDto
        {
            public DateTime? Batterydisc { get; set; }

            public DateTime? Batterycon { get; set; }

            public string Startloc { get; set; }

            public string Endloc { get; set; }

            public string Duration { get; set; }

            public string Status { get; set; }
        }
        public class StoppageMainModel
        {
            public List<StoppageSubModel> StoppageSubModel { get; set; }
            public int PageCount { get; set; }
        }


        public class StoppageSubModel
        {
            public string VehicleName { get; set; }
            public string DriverName { get; set; }

            public int StoppageCount { get; set; }
            public string TotalStoppageTime { get; set; }
            public string BBID { get; set; }
            public List<StoppageAnalysis> objStoppageReport { get; set; }
        }
        public class StoppageAnalysis
        {
            public string StopDateAndTime { get; set; }
            public string Location { get; set; }
            public string Duration { get; set; }
            public Boolean IgnitionStatus { get; set; }
            //public string poi { get; set; }
            //public string AddPoi { get; set; }
            //public string StopLatitude { get; set; }
            //public string StopLongitude { get; set; }
            //public string VehicleName { get; set; }
           }


      
        public class IdlingMainModel
        {
            public string BBID { get; set; }
            public int idlingCount { get; set; }          
            public string TotalIdlingTime { get; set; }          
            public string VehicleName { get; set; }           
            public string DriverName { get; set; }           
            public List<IdlingSubStatus> IdlingSubStatuslist { get; set; }
        }
        public class IdlingSubStatus
        {
            public Boolean IgnitionStatus { get; set; }
            public string startDate { get; set; }           
            public string stopDate { get; set; }                      
            public string location { get; set; }
            public string duration { get; set; }
            [DisplayName("POI Location")]
            public string poi { get; set; }

           
            public string AddPoi { get; set; }
            public decimal latitude { get; set; }
            public decimal longitude { get; set; }

            public string Vstatus { get; set; }


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
        #region Neha Vaid
        public class OverSpeedModel
        {
            public int PageCount { get; set; }
            public List<overSpeedMain> OSmainLst { get; set; }
        }
        public class overSpeedMain
        {
            public string bbid { get; set; }
            public string vehName { get; set; }
            public string driverName { get; set; }
            public int overspeedCount { get; set; }
            public int maxSpeed { get; set; }
            public int avgSpeed { get; set; }
            public int overSpeedVal { get; set; }
            public int totalSpeed { get; set; }
            public string overSpeedDuration { get; set; }
            public List<OverSpeedAnalysis> OSsublst { get; set; }
        }
        public class OverSpeedAnalysis
        {
            public DateTime dateTime { get; set; }
            public string location { get; set; }
            public int speed { get; set; }
            public float latitude { get; set; }
            public float longitude { get; set; }
            
        }
        #endregion
        public class EntryExitReport
        {
            public List<POIEntryExitModelExt> vehicleList { get; set; }
            public int PageCount { get; set; }
        }

        public class POIEntryExitModelExt
        {
            public String VehName { get; set; }
            public String driverName { get; set; }
            public int poisCovered { get; set; }
            public String Bbid { get; set; }

            //public int Totalpois { get; set; }

            public List<POIEntryExitModel> poisCoveredList { get; set; }
        }

        public class POIEntryExitModel
        {
            public String POIName { get; set; }
            public string Intime { get; set; }
            public string OutTime { get; set; }
            public string duration { get; set; }
            public int POIID { get; set; }
            [Browsable(false)]
            public string Bbid { get; set; }
            [Browsable(false)]
            public double POILat { get; set; }
            [Browsable(false)]
            public double POILong { get; set; }
            [Browsable(false)]
            public string IsActive { get; set; }
            [Browsable(false)]
            public string StartTempTime { get; set; }
            [Browsable(false)]
            public string Vehname { get; set; }
            [Browsable(false)]
            public string EndtTempTime { get; set; }
        }

    }
}
