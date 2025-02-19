namespace FlutterBlePlus;

public class Utils
{
    public static string UlongToMacAddress(ulong mac)
    {
        return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}",
            (mac >> 40) & 0xFF,
            (mac >> 32) & 0xFF,
            (mac >> 24) & 0xFF,
            (mac >> 16) & 0xFF,
            (mac >> 8) & 0xFF,
            mac & 0xFF);
    }
}
