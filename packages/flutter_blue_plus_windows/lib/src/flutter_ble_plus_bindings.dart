import 'dart:ffi' as ffi;

typedef StartScan = void Function();
typedef StopScan = void Function();

class FlutterBlePlusBindings  {
  // Load the DLL
  static final ffi.DynamicLibrary _nativeLib =
      // ffi.DynamicLibrary.open('FlutterBlePlus.dll');
      ffi.DynamicLibrary.open('flutter_blue_plus_windows/assets/FlutterBlePlus.dll');
      // ffi.DynamicLibrary.open('assets/FlutterBlePlus.dll');

  static final StartScan startScan = _nativeLib
      .lookup<ffi.NativeFunction<ffi.Void Function()>>('StartScan')
      .asFunction<StartScan>();

  static final StopScan stopScan = _nativeLib
      .lookup<ffi.NativeFunction<ffi.Void Function()>>('StopScan')
      .asFunction<StopScan>();
}
