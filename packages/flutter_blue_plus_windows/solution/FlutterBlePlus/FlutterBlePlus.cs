using System.Runtime.InteropServices;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

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
    
    static string UlongToMacAddress(ulong mac)
    {
        return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}",
            (mac >> 40) & 0xFF,
            (mac >> 32) & 0xFF,
            (mac >> 24) & 0xFF,
            (mac >> 16) & 0xFF,
            (mac >> 8) & 0xFF,
            mac & 0xFF);
    }

    private static async void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        try
        {
            var macAddress = UlongToMacAddress(args.BluetoothAddress);
            var localName = args.Advertisement.LocalName;

            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
            var deviceId = device?.DeviceId ?? "Unknown";

            var result = $"{localName} {macAddress} {deviceId}";

            Console.WriteLine(result);

            // Call the callback if it's registered
            if (_deviceFoundCallback == null) return;
            var deviceInfoPtr = Marshal.StringToHGlobalAnsi(result);
            _deviceFoundCallback(deviceInfoPtr);
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
    public delegate void DeviceFoundCallback(IntPtr deviceInfoPtr);
    private static DeviceFoundCallback? _deviceFoundCallback;
    
    [UnmanagedCallersOnly(EntryPoint = "RegisterDeviceFoundCallback")]
    public static void RegisterDeviceFoundCallback(IntPtr callbackPtr)
    {
        _deviceFoundCallback = callbackPtr == IntPtr.Zero 
            ? null 
            : Marshal.GetDelegateForFunctionPointer<DeviceFoundCallback>(callbackPtr);
    }

    [UnmanagedCallersOnly(EntryPoint = "FreeMemory")]
    public static void FreeMemory(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return;
        Marshal.FreeHGlobal(ptr);
    }
}
