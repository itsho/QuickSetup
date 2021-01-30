using System;
using System.Linq;
using System.Management;
using System.Text;
using Simplified.IO;

namespace QuickSetup.UI.Infra
{
    public static class HardwareLogic
    {
        public static string GetComputerDetails()
        {
            try
            {
                var result = new StringBuilder();

                {
                    var myProcessorObject = new ManagementObjectSearcher("select Name from Win32_Processor");
                    foreach (var o in myProcessorObject.Get())
                    {
                        var obj = (ManagementObject)o;
                        result.Append("CPU: " + obj["Name"] + Environment.NewLine);
                        break;
                    }
                }
                {
                    var myVideoObject = new ManagementObjectSearcher("select Name from Win32_VideoController");
                    foreach (var o in myVideoObject.Get())
                    {
                        var obj = (ManagementObject)o;
                        result.Append("GPU: " + obj["Name"] + Environment.NewLine);
                        break;
                    }
                }
                {
                    var myOperativeSystemObject = new ManagementObjectSearcher("select Caption from Win32_OperatingSystem");
                    foreach (var o in myOperativeSystemObject.Get())
                    {
                        var obj = (ManagementObject)o;
                        result.Append("OS: " + obj["Caption"] + Environment.NewLine);
                        break;
                    }
                }

                return result.ToString();
            }
            catch (Exception e)
            {
                Logger.Log.Error(e);
                return "Unknown";
            }
        }

        public static string GetFixedDrivesStatus()
        {
            try
            {
                var driveInfo = WmiController.GetSmartInformation();
                var result = new StringBuilder();

                foreach (var drive in driveInfo)
                {
                    result.AppendLine("-----------------------------------------------------");
                    result.AppendLine($" DRIVE ({((drive.IsOK) ? "OK" : "BAD")}): {drive.Serial} - {drive.Model} - {drive.Type}");
                    result.AppendLine("-----------------------------------------------------");
                    result.AppendLine("");

                    result.AppendLine("Attribute\t\t\tCurrent  Worst  Threshold  Data  Status");
                    var maxNameLen = drive.SmartAttributes.Max(s => s.Name.Length);
                    foreach (var attr in drive.SmartAttributes)
                    {
                        if (attr.HasData)
                            result.AppendLine($"{attr.Name.PadRight(maxNameLen, ' ')} {attr.Current}\t {attr.Worst}\t {attr.Threshold}\t {attr.Data.ToString().PadRight(9, ' ')} {((attr.IsOK) ? "OK" : "BAD")}");
                    }
                    result.AppendLine();
                }
              

                return result.ToString();
            }
            catch (Exception e)
            {
                Logger.Log.Error(e);
                return "Unknown";
            }
        }
    }
}