import 'dart:async';
import 'dart:ffi';
import 'package:ffi/ffi.dart';

// Define the FFI signatures
typedef RegisterCallbackNative = Void Function(Pointer<NativeFunction<DeviceFoundCallbackNative>>);
typedef RegisterCallbackDart = void Function(Pointer<NativeFunction<DeviceFoundCallbackNative>>);

typedef StartScanNative = Void Function();
typedef StartScanDart = void Function();

typedef StopScanNative = Void Function();
typedef StopScanDart = void Function();

typedef FreeMemoryNative = Void Function(Pointer<Void>);
typedef FreeMemoryDart = void Function(Pointer<Void>);

// Define the callback signature
typedef DeviceFoundCallbackNative = Void Function(Pointer<Utf8>);
typedef DeviceFoundCallbackDart = void Function(Pointer<Utf8>);

class FlutterBlePlusBindings {
  // Native library
  static final DynamicLibrary _nativeLib =
    DynamicLibrary.open('flutter_blue_plus_windows/solution/assets/FlutterBlePlus.dll');

  // Load native functions
  static final RegisterCallbackDart _registerCallback = _nativeLib
      .lookupFunction<RegisterCallbackNative, RegisterCallbackDart>('RegisterDeviceFoundCallback');

  static final StartScanDart _startScan = _nativeLib
      .lookupFunction<StartScanNative, StartScanDart>('StartScan');

  static final StopScanDart _stopScan = _nativeLib
      .lookupFunction<StopScanNative, StopScanDart>('StopScan');

  static final FreeMemoryDart _freeMemory = _nativeLib
      .lookupFunction<FreeMemoryNative, FreeMemoryDart>('FreeMemory');

  // Stream controller for device discovery events
  final _deviceStreamController = StreamController<String>.broadcast();
  Stream<String> get onDeviceFound => _deviceStreamController.stream;

  // Native callback instance
  late final NativeCallable<DeviceFoundCallbackNative> _nativeCallback;

  // Singleton implementation
  static final FlutterBlePlusBindings _instance = FlutterBlePlusBindings._internal();
  factory FlutterBlePlusBindings() => _instance;

  FlutterBlePlusBindings._internal() {
    _setupCallback();
  }

  void _setupCallback() {
    // Create a NativeCallable instance with the listener method
    _nativeCallback = NativeCallable<DeviceFoundCallbackNative>.listener(
          (Pointer<Utf8> deviceInfoPtr) {
        // Convert the deviceInfo to a Dart string
        final deviceInfo = deviceInfoPtr.toDartString();

        // Add the device info to the stream
        _deviceStreamController.add(deviceInfo);

        // Free the memory allocated in the native code
        _freeMemory(deviceInfoPtr.cast<Void>());
      },
    );

    // Register the callback with the native code
    _registerCallback(_nativeCallback.nativeFunction);
  }

  // Start scanning
  void startScan() {
    _startScan();
  }

  // Stop scanning
  void stopScan() {
    _stopScan();
  }

  // Clean up resources
  void dispose() {
    stopScan();
    _deviceStreamController.close();
    _nativeCallback.close();
  }
}