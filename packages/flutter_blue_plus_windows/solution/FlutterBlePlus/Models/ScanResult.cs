using System.Runtime.InteropServices;

namespace FlutterBlePlus.Models;

[StructLayout(LayoutKind.Sequential)]
public struct ScanResult
{
    public IntPtr Name;  // Utf8 string pointer
    public IntPtr MacAddress;  // Utf8 string pointer
    public int Rssi;
    public int ManufacturerId;
    public float Latitude;
    public float Longitude;
}
