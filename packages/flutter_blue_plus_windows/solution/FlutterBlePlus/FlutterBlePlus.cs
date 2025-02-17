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
            Console.Write(args.Advertisement.LocalName);
            Console.Write(" ");
            Console.Write(UlongToMacAddress(args.BluetoothAddress));
            Console.Write(" ");
    
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
            if (device != null)
            {
                Console.WriteLine(device.DeviceId);   
            }
    
            Console.WriteLine();
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
}
