using System;
using System.ServiceModel;

namespace ServiceHostApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost svc = null;

            try
            {
                PmsmService service = new PmsmService();
                svc = new ServiceHost(service);
                svc.Open();

                Console.WriteLine("Servis je pokrenut.");
                Console.WriteLine("Pritisni ENTER za zaustavljanje servisa...");
                Console.ReadLine();

                svc.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Greska pri pokretanju servisa: " + e.Message);
                if (svc != null)
                {
                    svc.Abort();
                }
            }
        }
    }
}
