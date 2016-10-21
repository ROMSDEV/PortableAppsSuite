namespace gpuz.exe
{
    using System;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            ProcessEx.Start("%CurDir%\\..\\..\\..\\GPU-ZPortable\\GPU-ZPortable.exe");
        }
    }
}
