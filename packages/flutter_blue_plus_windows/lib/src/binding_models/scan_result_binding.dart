import 'dart:ffi';
import 'package:ffi/ffi.dart';

final class ScanResultBinding extends Struct {
  external Pointer<Utf8> name;
  external Pointer<Utf8> macAddress;
  @Int32()
  external int rssi;
  @Int32()
  external int manufacturerId;
  @Float()
  external double latitude;
  @Float()
  external double longitude;

  // Factory constructor
  static Pointer<ScanResultBinding> allocate(
      String name, String macAddress, int rssi, int manufacturerId, double latitude, double longitude) {
    final result = calloc<ScanResultBinding>();
    result.ref.name = name.toNativeUtf8();
    result.ref.macAddress = macAddress.toNativeUtf8();
    result.ref.rssi = rssi;
    result.ref.manufacturerId = manufacturerId;
    result.ref.latitude = latitude;
    result.ref.longitude = longitude;
    return result;
  }
}
