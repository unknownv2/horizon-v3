using System.ServiceProcess;

namespace NoDev.Fatx
{
    static class Program
    {
        static void Main()
        {
            FatxDeviceService.StartDebug();

            ServiceBase.Run(new ServiceBase[] { new FatxDeviceService() });
        }
    }
}
