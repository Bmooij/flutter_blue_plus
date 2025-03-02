import 'dart:ffi';
import 'package:ffi/ffi.dart';

final class ScanResultBinding extends Struct {
  external Pointer<Utf8> remoteId;
  external Pointer<Utf8> name;
  external Pointer<Utf8> advName;
  @Int16()
  external int rssi;
  external Pointer<Pointer<Utf8>> serviceUuidsPtr;
  @Int32()
  external int serviceUuidsCount;

  // Helper method to convert C string array to Dart List<String>
  List<String> getServiceUuids() {
    final result = <String>[];
    if (serviceUuidsPtr != nullptr && serviceUuidsCount > 0) {
      for (int i = 0; i < serviceUuidsCount; i++) {
        final uuidPtr = serviceUuidsPtr[i];
        if (uuidPtr != nullptr) {
          result.add(uuidPtr.toDartString());
        }
      }
    }
    return result;
  }
}
