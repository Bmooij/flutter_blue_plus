import 'dart:async';
import 'dart:ffi';
import 'package:ffi/ffi.dart';

// imports of the binding models
import 'binding_models/scan_result_binding.dart';

// imports of the models
import 'models//scan_result.dart';

// Define the FFI signatures
typedef RegisterCallbackNative = Void Function(Pointer<NativeFunction<ScanResultCallbackNative>>);
typedef RegisterCallbackDart = void Function(Pointer<NativeFunction<ScanResultCallbackNative>>);

typedef StartScanNative = Void Function();
typedef StartScanDart = void Function();

typedef StopScanNative = Void Function();
typedef StopScanDart = void Function();

typedef FreeScanResultMemoryNative = Void Function(Pointer<ScanResultBinding>);
typedef FreeScanResultMemoryDart = void Function(Pointer<ScanResultBinding>);

// Define the callback signature
typedef ScanResultCallbackNative = Void Function(Pointer<ScanResultBinding>);
typedef ScanResultCallbackDart = void Function(Pointer<ScanResultBinding>);

class FlutterBlePlusBindings {
  // Native library
  static final DynamicLibrary _nativeLib =
    DynamicLibrary.open('flutter_blue_plus_windows/solution/assets/FlutterBlePlus.dll');

  // Load native functions
  static final RegisterCallbackDart _registerScanResultCallback = _nativeLib
      .lookupFunction<RegisterCallbackNative, RegisterCallbackDart>('RegisterScanResultCallback');

  static final FreeScanResultMemoryDart _freeScanResultMemory = _nativeLib
      .lookupFunction<FreeScanResultMemoryNative, FreeScanResultMemoryDart>('FreeScanResultMemory');

  static final StartScanDart _startScan = _nativeLib
      .lookupFunction<StartScanNative, StartScanDart>('StartScan');

  static final StopScanDart _stopScan = _nativeLib
      .lookupFunction<StopScanNative, StopScanDart>('StopScan');

  // Stream controller for device discovery events
  final _scanResultStreamController = StreamController<ScanResult>.broadcast();
  Stream<ScanResult> get onScanResult => _scanResultStreamController.stream;

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
          (Pointer<ScanResultBinding> scanResultPtr) async {

            if (scanResultPtr == nullptr) return;
        // Extract data from the struct
        final scanResult = ScanResult(
          name: scanResultPtr.ref.name.toDartString(),
          macAddress: scanResultPtr.ref.macAddress.toDartString(),
          rssi: scanResultPtr.ref.rssi,
          manufacturerId: scanResultPtr.ref.manufacturerId,
          latitude: scanResultPtr.ref.latitude,
          longitude: scanResultPtr.ref.longitude,
        );

        // Add the device info to the stream
        _scanResultStreamController.add(scanResult);

        // Free the memory allocated in the native code
        _freeScanResultMemory(scanResultPtr);
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