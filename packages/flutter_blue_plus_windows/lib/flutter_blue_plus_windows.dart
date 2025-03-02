import 'dart:async';

import 'package:flutter_blue_plus_platform_interface/flutter_blue_plus_platform_interface.dart';

import 'src/flutter_ble_plus_bindings.dart';

final class FlutterBluePlusWindows extends FlutterBluePlusPlatform {

  static final _wrapper = FlutterBlePlusBindings();

  @override
  Stream<BmScanResponse> get onScanResponse => _wrapper.onScanResult;

  FlutterBluePlusWindows._() {
  }

  static void registerWith() {
    FlutterBluePlusPlatform.instance = FlutterBluePlusWindows._();
  }

  @override
  Future<BmBluetoothAdapterState> getAdapterState(
      BmBluetoothAdapterStateRequest request,
      ) {
    return Future.value(
      BmBluetoothAdapterState(
        adapterState: BmAdapterStateEnum.on,
      ),
    );
  }

  @override
  Future<bool> isSupported(
      BmIsSupportedRequest request,
      ) {
    return Future.value(true);
  }

  @override
  Future<bool> startScan(
    BmScanSettings request,
  ) {
    _wrapper.startScan();
    return Future.value(true);
  }

  @override
  Future<bool> stopScan(
    BmStopScanRequest request,
  ) {
    _wrapper.stopScan();
    return Future.value(true);
  }

  Future<bool> connect(
      BmConnectRequest request,
      ) {
    _wrapper.connect(request.remoteId.toString());
    return Future.value(false);
  }
}
