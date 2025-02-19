class ScanResult {
  final String name;
  final String macAddress;
  final int rssi;
  final int manufacturerId;
  final double latitude;
  final double longitude;

  ScanResult({
    required this.name,
    required this.macAddress,
    required this.rssi,
    required this.manufacturerId,
    required this.latitude,
    required this.longitude,
  });

  @override
  String toString() {
    return 'ScanResult{name: $name, macAddress: $macAddress, rssi: $rssi, '
        'manufacturerId: $manufacturerId, latitude: $latitude, longitude: $longitude}';
  }
}