using System.ServiceProcess;

namespace WebServeurLocal
{
    internal static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new WebServeurService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
