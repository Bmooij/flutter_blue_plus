import 'dart:async';

import 'package:flutter_blue_plus_platform_interface/flutter_blue_plus_platform_interface.dart';

import 'src/flutter_ble_plus_bindings.dart';

final class FlutterBluePlusWindows extends FlutterBluePlusPlatform {

  static void registerWith() {
    FlutterBluePlusPlatform.instance = FlutterBluePlusWindows();
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
    FlutterBlePlusBindings.startScan();
    return Future.value(true);
  }

  @override
  Future<bool> stopScan(
    BmStopScanRequest request,
  ) {
    FlutterBlePlusBindings.stopScan();
    return Future.value(true);
  }
}
