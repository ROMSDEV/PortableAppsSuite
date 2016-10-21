namespace FurMarkPortable
{
    using System;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            ProcessEx.Start("%CurDir%\\FurMark\\FurMark.exe");
        }
    }
}
