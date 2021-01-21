using System;
using System.Threading.Tasks;
using Toef.NhlNetworkSchedule.Xmltv.Services;

namespace Toef.NhlNetworkSchedule.Xmltv
{
    class Program
    {
        async static Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Missing output argument!");
                return;
            }
            var scheduleService = new ScheduleService();
            await scheduleService.Update(args[0]);
        }
    }
}
