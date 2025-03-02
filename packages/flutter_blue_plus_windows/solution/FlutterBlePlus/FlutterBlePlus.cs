using System.Runtime.InteropServices;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using FlutterBlePlus.Models;

namespace FlutterBlePlus;

public static class FlutterBlePlus
{
    private static readonly BluetoothLEAdvertisementWatcher Watcher = new() 
    {
        ScanningMode = BluetoothLEScanningMode.Active,
    };
    
    static FlutterBlePlus()
    {
        Watcher.Received += WatcherOnReceived;   
    }

    private static async void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        try
        {
            // Call the callback if it's registered
            if (_scanResultCallback == null) return;
            
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
            if (device == null) return;
            
            var serviceUuids = args.Advertisement.ServiceUuids;
            var name = device.Name;
            if (name.Equals($"Bluetooth {Utils.UlongToMacAddress(device.BluetoothAddress)}", StringComparison.InvariantCultureIgnoreCase))
            {
                name = null;
            }
            
            var scanResult = new ScanResult
            {
                RemoteId = Marshal.StringToHGlobalAnsi(device.DeviceId),
                Name = Marshal.StringToHGlobalAnsi(name),
                Rssi = args.RawSignalStrengthInDBm,
                AdvName = Marshal.StringToHGlobalAnsi(args.Advertisement.LocalName),
                ServiceUuidsCount = serviceUuids.Count,
                ServiceUuidsPtr = IntPtr.Zero
            };
            
            Console.WriteLine($"{device.DeviceId} {name} {device.Name} {scanResult.Rssi}");
            
            // Handle service UUIDs if any
            if (serviceUuids.Count > 0)
            {
                // Allocate memory for an array of IntPtr (pointers to strings)
                var serviceUuidsArrayPtr = Marshal.AllocHGlobal(serviceUuids.Count * IntPtr.Size);
            
                // For each UUID, allocate memory for the string and store pointer in the array
                for (var i = 0; i < serviceUuids.Count; i++)
                {
                    var uuidStringPtr = Marshal.StringToHGlobalAnsi(serviceUuids[i].ToString());
                    Marshal.WriteIntPtr(serviceUuidsArrayPtr, i * IntPtr.Size, uuidStringPtr);
                }
            
                scanResult.ServiceUuidsPtr = serviceUuidsArrayPtr;
            }
            
            // Allocate memory for the struct and copy the struct to it
            var deviceInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ScanResult>());
            Marshal.StructureToPtr(scanResult, deviceInfoPtr, false);
            
            _scanResultCallback(deviceInfoPtr);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "StartScan")]
    public static void StartScan()
    {
        Watcher.Start();
    }

    [UnmanagedCallersOnly(EntryPoint = "StopScan")]
    public static void StopScan()
    {
        Watcher.Stop();
    }
    
    // Define the delegate type that matches the callback signature
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ScanResultCallback(IntPtr deviceInfoPtr);
    private static ScanResultCallback? _scanResultCallback;
    
    [UnmanagedCallersOnly(EntryPoint = "RegisterScanResultCallback")]
    public static void RegisterScanResultCallback(IntPtr callbackPtr)
    {
        _scanResultCallback = callbackPtr == IntPtr.Zero 
            ? null 
            : Marshal.GetDelegateForFunctionPointer<ScanResultCallback>(callbackPtr);
    }

    [UnmanagedCallersOnly(EntryPoint = "FreeScanResultMemory")]
    public static void FreeScanResultMemory(IntPtr scanResultPtr)
    {
        if (scanResultPtr == IntPtr.Zero) return;
        // Get the struct to free the string pointers
        var deviceInfo = Marshal.PtrToStructure<ScanResult>(scanResultPtr);

        Marshal.FreeHGlobal(deviceInfo.RemoteId);
        Marshal.FreeHGlobal(deviceInfo.Name);
        Marshal.FreeHGlobal(deviceInfo.AdvName);

        // Free service UUID strings and the array
        if (deviceInfo.ServiceUuidsPtr != IntPtr.Zero && deviceInfo.ServiceUuidsCount > 0)
        {
            for (var i = 0; i < deviceInfo.ServiceUuidsCount; i++)
            {
                var uuidStringPtr = Marshal.ReadIntPtr(deviceInfo.ServiceUuidsPtr, i * IntPtr.Size);
                if (uuidStringPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(uuidStringPtr);
                }
            }
            Marshal.FreeHGlobal(deviceInfo.ServiceUuidsPtr);
        }

        // Free the struct memory
        Marshal.FreeHGlobal(scanResultPtr);
    }
}
