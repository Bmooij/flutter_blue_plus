using System.Runtime.InteropServices;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Google.Protobuf;

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
    
    private static void HandleCall(string methodName, byte[] data)
    {
        if (methodName == "StartScan")
            StartScan(data);
        else if (methodName == "StopScan")
            StopScan(data);
        else
            throw new ArgumentOutOfRangeException(nameof(methodName), methodName, null);
    }
    
    
    private static void StartScan(byte[] data)
    {
        Watcher.Start();
    }
    
    private static void StopScan(byte[] data)
    {
        Watcher.Stop();
    }

    private static async void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        try
        {
            // Call the callback if it's registered
            if (_methodCallback == null) return;
            
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
                RemoteId = device.DeviceId,
                Name = name,
                Rssi = args.RawSignalStrengthInDBm,
                AdvName = args.Advertisement.LocalName
            };
            scanResult.ServiceUuids.AddRange(serviceUuids.Select(x => x.ToString()));
            
            Console.WriteLine($"{device.DeviceId} {name} {device.Name} {scanResult.Rssi}");
            
            CallMethod("OnScanResult", scanResult);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MethodCallHandler")]
    public static void MethodCallHandler(IntPtr byteArrayPtr, int length)
    {
        // Convert the IntPtr to a managed byte array
        var managedByteArray = new byte[length];
        Marshal.Copy(byteArrayPtr, managedByteArray, 0, length);
    
        // Now you can work with managedByteArray in your C# code
        // For example, you could log the array length:
        Console.WriteLine($"Received byte array of length: {managedByteArray.Length}");
        
        var methodCall = MethodCall.Parser.ParseFrom(managedByteArray);
        HandleCall(methodCall.Method, methodCall.Data.ToByteArray());
    }
    
    private static void CallMethod(string method, IMessage obj)
    {
        if (_methodCallback == null) return;
        
        var memStream = new MemoryStream();
        obj.WriteTo(memStream);
        var objData = memStream.ToArray();
        
        var methodCall = new MethodCall
        {
            Method = method,
            Data =  ByteString.CopyFrom(objData)
        };

        memStream.Position = 0;
        methodCall.WriteTo(memStream);
        var bufferData = memStream.ToArray();
            
        // Allocate memory for the struct and copy the struct to it
        var bufferPtr = Marshal.AllocHGlobal(bufferData.Length);
        // Copy the byte array to the allocated memory
        Marshal.Copy(bufferData, 0, bufferPtr, bufferData.Length);
        
        _methodCallback(bufferPtr, bufferData.Length);
    }
    
    // Define the delegate type that matches the callback signature
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MethodCallback(IntPtr byteArrayPtr, int length);
    private static MethodCallback? _methodCallback;
    
    [UnmanagedCallersOnly(EntryPoint = "RegisterMethodCallback")]
    public static void RegisterMethodCallback(IntPtr callbackPtr)
    {
        _methodCallback = callbackPtr == IntPtr.Zero 
            ? null 
            : Marshal.GetDelegateForFunctionPointer<MethodCallback>(callbackPtr);
    }

    [UnmanagedCallersOnly(EntryPoint = "FreeMethodCallback")]
    public static void FreeMethodCallback(IntPtr bufferPtr)
    {
        if (bufferPtr == IntPtr.Zero) return;
        Marshal.FreeHGlobal(bufferPtr);
    }
}
