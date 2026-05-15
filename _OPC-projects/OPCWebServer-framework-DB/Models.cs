using System;
using System.Collections.Generic;

namespace OPCWebServer
{
    public class AppConfig
    {
        public OpcSettings OpcSettings { get; set; } = new OpcSettings();
        public WebSettings WebSettings { get; set; } = new WebSettings();
        public UdpSettings UdpSettings { get; set; } = new UdpSettings();
        public List<TagConfig> Tags { get; set; } = new List<TagConfig>();
        public DatabaseSettings DatabaseSettings { get; set; } = new DatabaseSettings();
    }

    public class OpcSettings
    {
        public string AppName { get; set; } = "Graybox";
        public string ServerId { get; set; } = "Graybox.Simulator";
        public int RefreshRateMs { get; set; } = 2000;
    }

    public class WebSettings
    {
        public bool Enabled { get; set; } = true;
        public int Port { get; set; } = 8085;
        public string StaticFolder { get; set; } = "wwwroot";
    }

    public class UdpSettings
    {
        public bool Enabled { get; set; } = true;
        public string RemoteIp { get; set; } = "127.0.0.1";
        public int RemotePort { get; set; } = 3310;
    }
    public class DatabaseSettings
    {
        public bool Enabled { get; set; } = false;
    }
    public class TagConfig
    {
        public int Id { get; set; }
        public string Address { get; set; } = "";
        public string DataType { get; set; } = "float";
        public double Multiplier { get; set; }
        public double Offset { get; set; }
        public bool Invert { get; set; }
        public bool UdpSend { get; set; }
        public bool SaveToDb { get; set; }
    }

    public class TagViewItem
    {
        public int Id { get; set; }
        public string Address { get; set; } = "";
        public string DataType { get; set; } = "float";
        public bool Invert { get; set; } = false;
        public bool UdpSend { get; set; } = false;
        public bool SaveToDb { get; set; } = false;
    }
    public class RawData
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int TagId { get; set; }
        public double Value { get; set; } // double универсальнее для float/bool
    }
    public class AveragedData
    {
        public int Id { get; set; }
        public DateTime CalculationTime { get; set; } // Когда считали (19:00)
        public DateTime PeriodStart { get; set; }    // Начало периода (07:00)
        public int TagId { get; set; }
        public double AverageValue { get; set; }
        public double RuntimeMinutes { get; set; }
        public double TotalPeriodMinutes { get; set; }
    }
    public class DowntimeEvent
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationMinutes { get; set; }
    }
}
