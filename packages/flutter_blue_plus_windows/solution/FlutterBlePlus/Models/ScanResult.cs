using System.Runtime.InteropServices;

namespace FlutterBlePlus.Models;

[StructLayout(LayoutKind.Sequential)]
public struct ScanResult
{
    public IntPtr RemoteId;
    public IntPtr Name;
    public IntPtr AdvName;
    public int Rssi;
    public IntPtr ServiceUuidsPtr; // Pointer to array of strings
    public int ServiceUuidsCount;  // Count of UUIDs in the array
}
