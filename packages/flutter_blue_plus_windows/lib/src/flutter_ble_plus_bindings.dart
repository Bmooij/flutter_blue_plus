import 'dart:async';
import 'dart:ffi';
import 'package:ffi/ffi.dart';

// Define the FFI signatures
typedef RegisterCallbackNative = Void Function(Pointer<NativeFunction<ScanResultCallbackNative>>);
typedef RegisterCallbackDart = void Function(Pointer<NativeFunction<ScanResultCallbackNative>>);

typedef StartScanNative = Void Function();
typedef StartScanDart = void Function();

typedef StopScanNative = Void Function();
typedef StopScanDart = void Function();

typedef FreeMemoryNative = Void Function(Pointer<Void>);
typedef FreeMemoryDart = void Function(Pointer<Void>);

// Define the callback signature
typedef ScanResultCallbackNative = Void Function(Pointer<Utf8>);
typedef ScanResultCallbackDart = void Function(Pointer<Utf8>);

class FlutterBlePlusBindings {
  // Native library
  static final DynamicLibrary _nativeLib =
    DynamicLibrary.open('flutter_blue_plus_windows/solution/assets/FlutterBlePlus.dll');

  // Load native functions
  static final RegisterCallbackDart _registerScanResultCallback = _nativeLib
      .lookupFunction<RegisterCallbackNative, RegisterCallbackDart>('RegisterScanResultCallback');

  static final FreeMemoryDart _freeScanResultMemory = _nativeLib
      .lookupFunction<FreeMemoryNative, FreeMemoryDart>('FreeScanResultMemory');

  static final StartScanDart _startScan = _nativeLib
      .lookupFunction<StartScanNative, StartScanDart>('StartScan');

  static final StopScanDart _stopScan = _nativeLib
      .lookupFunction<StopScanNative, StopScanDart>('StopScan');

  // Stream controller for device discovery events
  final _scanResultStreamController = StreamController<String>.broadcast();
  Stream<String> get onScanResult => _scanResultStreamController.stream;

  // Native callback instance
  late final NativeCallable<ScanResultCallbackNative> _scanResultNativeCallback;

  // Singleton implementation
  static final FlutterBlePlusBindings _instance = FlutterBlePlusBindings._internal();
  factory FlutterBlePlusBindings() => _instance;

  FlutterBlePlusBindings._internal() {
    _setupCallbacks();
  }

  void _setupCallbacks() {
    // Create a NativeCallable instance with the listener method
    _scanResultNativeCallback = NativeCallable<ScanResultCallbackNative>.listener(
          (Pointer<Utf8> deviceInfoPtr) async {
        // Convert the deviceInfo to a Dart string
        final deviceInfo = deviceInfoPtr.toDartString();

        // Add the device info to the stream
        _scanResultStreamController.add(deviceInfo);

        // Free the memory allocated in the native code
        _freeScanResultMemory(deviceInfoPtr.cast<Void>());
      },
    );

    // Register the callback with the native code
    _registerScanResultCallback(_scanResultNativeCallback.nativeFunction);
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
    _scanResultStreamController.close();
    _scanResultNativeCallback.close();
  }
}