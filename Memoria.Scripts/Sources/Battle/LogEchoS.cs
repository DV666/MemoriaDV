using Memoria.Prime;

namespace Memoria.EchoS
{
    public static class LogEchoS
    {
        public static bool DebugEnable = false;

        public static void Debug(string text)
        {
            if (DebugEnable)
                Log.Message("[Echo-S] == DEBUG == : " + text);
        }
    }
}
