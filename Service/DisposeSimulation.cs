using System;
using Common;

namespace ServiceHostApp
{
    public static class DisposeSimulation
    {
        public static void SimulateInterruptedTransfer()
        {
            try
            {
                using (SessionFileStorage storage = new SessionFileStorage("Storage"))
                {
                    storage.WriteAcceptedSample(new MotorSample(1, 1, 1000, 1, 25, 0.5));
                    throw new InvalidOperationException("Simulacija prekida veze usred prenosa.");
                }
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Simulacija zavrsena: using blok je pozvao Dispose i zatvorio fajlove.");
            }
        }
    }
}