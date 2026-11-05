using System;
using System.Diagnostics;
using System.Reflection;

using FatturaFacile.Wpf.Contracts.Services;

namespace FatturaFacile.Wpf.Services
{
    public class ApplicationInfoService : IApplicationInfoService
    {
        public ApplicationInfoService()
        {
        }

        public Version GetVersion()
        {
            // Set the app version in FatturaFacile.Wpf > Properties > Package > PackageVersion
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            return new Version(version);
        }
    }
}
