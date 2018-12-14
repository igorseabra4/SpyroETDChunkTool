using System;
using System.Linq;

namespace SpyroETDChunkTool
{
    public static class Extensions
    {
        public static bool isPS2 = false;

        public static int Switch(this int a)
        {
            if (isPS2) return a;
            return BitConverter.ToInt32(BitConverter.GetBytes(a).Reverse().ToArray(), 0);
        }
        public static uint Switch(this uint a)
        {
            if (isPS2) return a;
            return BitConverter.ToUInt32(BitConverter.GetBytes(a).Reverse().ToArray(), 0);
        }
        public static short Switch(this short a)
        {
            if (isPS2) return a;
            return BitConverter.ToInt16(BitConverter.GetBytes(a).Reverse().ToArray(), 0);
        }
        public static ushort Switch(this ushort a)
        {
            if (isPS2) return a;
            return BitConverter.ToUInt16(BitConverter.GetBytes(a).Reverse().ToArray(), 0);
        }
        public static float Switch(this float a)
        {
            if (isPS2) return a;
            return BitConverter.ToSingle(BitConverter.GetBytes(a).Reverse().ToArray(), 0);
        }
        public static byte Switch(this byte a)
        {
            return a;
        }
    }
}
