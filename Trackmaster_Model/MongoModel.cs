using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class MongoModel
    {
        public class VehicleMaster
        {
            public string BBID { get; set; }
            public string VehName { get; set; }
            public string DriverName { get; set; }
            public string MobNo { get; set; }
            public int TotalRecords { get; set; }
            public double lat { get; set; }
            public double lng { get; set; }
            public int speed { get; set; }
            public DateTime lastUpdated { get; set; }
            public int gsmSignal { get; set; }
            public double vehicleBatteryVoltage { get; set; }
            public int vtsBatteryLevel { get; set; }
            public string location { get; set; }

            public decimal CurrentFuelLevel { get; set; }

            public double RemainingFuelLevel { get; set; }

            public decimal TotalFuel { get; set; }

            public string LastDateTime { get; set; }

            public string DisconnectedData { get; set; }


            public int gpsAntConStatus { get; set; }
            public bool GPSFix { get; set; }
            public bool IgnitionStatus { get; set; }
            public double vehBattery { get; set; }
            public int deviceBattery { get; set; }
            public string Type { get; set; }
            public string model { get; set; }
            public int overSpeedLimit { get; set; }
            public bool acSignal { get; set; }
            public int immobilizer { get; set; }
            public string Status { get; set; }
            public int alertsCount { get; set; }
            public Double distance0h { get; set; }
            public Double distance { get; set; }
            public double todayDistance { get; set; }
            public double todayWHour { get; set; }
            public double wHour { get; set; }
            public double wHour0h { get; set; }

        }

        [BsonIgnoreExtraElements]
        public class DeviceLiveData
        {
            [BsonId]
            public ObjectId Id { get; set; }

            [JsonPropertyName("mongoBbid")]
            public string bbid { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public int speed { get; set; }
            public DateTime dataDate { get; set; }
            public int gsmsignal { get; set; }
            public bool hasfix { get; set; }
            public bool ignitionStatus { get; set; }
            public double vehicleBatteryVoltage { get; set; }
            public int vtsBatteryLevel { get; set; }
            public string location { get; set; }
            public double fuelLevel { get; set; }
            public bool acSignal { get; set; }
            public int immobilizer { get; set; }
            public Double distance0h { get; set; }
            public Double distance { get; set; }
            public string vehName { get; set; }
            public int overSpeedLimit { get; set; }

            [BsonElement("ioJson")]
            public IoJsonData ioJson { get; set; }

            [BsonElement("ioJson0h")]
            public IoJsonData ioJson0h { get; set; }
        }


        [BsonIgnoreExtraElements]
        public class IoJsonData
        {
            [BsonElement("wHour")]
            public double wHour { get; set; }

        }
    }
}
