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
            var macAddress = Utils.UlongToMacAddress(args.BluetoothAddress);
            var localName = args.Advertisement.LocalName;

            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
            var deviceId = device?.DeviceId ?? "Unknown";

            var result = $"{localName} {macAddress} {deviceId}";

            Console.WriteLine(result);
            
            // Get manufacturer data if available
            int manufacturerId = 0;
            if (args.Advertisement.ManufacturerData.Count > 0)
            {
                manufacturerId = (int)args.Advertisement.ManufacturerData[0].CompanyId;
            }

            // Call the callback if it's registered
            if (_scanResultCallback == null) return;

            var scanResult = new ScanResult
            {
                MacAddress = Marshal.StringToHGlobalAnsi(macAddress),
                Name = Marshal.StringToHGlobalAnsi(localName),
                Rssi = args.RawSignalStrengthInDBm,
                ManufacturerId = manufacturerId,
                Latitude = 0.0f,
                Longitude = 0.0f
            };
            
            // Allocate memory for the struct and copy the struct to it
            IntPtr deviceInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ScanResult>());
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
        
        Marshal.FreeHGlobal(deviceInfo.Name);
        Marshal.FreeHGlobal(deviceInfo.MacAddress);
        
        // Free the struct memory
        Marshal.FreeHGlobal(scanResultPtr);
    }
}
