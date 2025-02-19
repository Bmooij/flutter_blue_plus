import 'dart:async';

import 'package:flutter_blue_plus_platform_interface/flutter_blue_plus_platform_interface.dart';

import 'src/flutter_ble_plus_bindings.dart';

final class FlutterBluePlusWindows extends FlutterBluePlusPlatform {

  final _onScanResponseController = StreamController<BmScanResponse>.broadcast();

  static final _wrapper = FlutterBlePlusBindings();

  @override
  Stream<BmScanResponse> get onScanResponse {
    return _onScanResponseController.stream;
  }

  FlutterBluePlusWindows._() {
    _wrapper.onScanResult.listen((deviceInfo) {
      print('FROM C# $deviceInfo');
    });
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
}
