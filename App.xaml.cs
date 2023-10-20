using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace CisoConverter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string DllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dll");

        // The Startup event
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Add Assembly Resolver
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Form the path of the DLL in the "dll" directory
            string assemblyPath = Path.Combine(DllPath, new AssemblyName(args.Name).Name + ".dll");

           
            // Check if the file exists in the path
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            else
            {
                return null;
            }
        }
    }
}