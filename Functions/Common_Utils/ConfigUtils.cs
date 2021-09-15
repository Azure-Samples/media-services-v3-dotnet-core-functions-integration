// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Common_Utils
{
    /// <summary>
    /// This class is used to load the configuration and settings
    /// </summary>
    public static class ConfigUtils
    {
        public static ConfigWrapper GetConfig()
        {
            // If Visual Studio is used, let's read the .env file which should be in the root folder (same folder than the solution .sln file).
            // Same code will work in VS Code, but VS Code uses also launch.json to get the .env file.
            // You can create this ".env" file by saving the "sample.env" file as ".env" file and fill it with the right values.
            try
            {
                Load(".env");
            }
            catch
            {

            }

            ConfigWrapper config = new(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables() // parses the values from the optional .env file at the solution root
                .Build());

            // For Azure.Identity environment credential
            // see https://docs.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", config.AadTenantId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", config.AadClientId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", config.AadSecret);

            return config;
        }


        /// <summary>
        /// Loads the .env file and stores the values as variables
        /// </summary>
        /// <param name="filePath"></param>
        private static void Load(string envFileName)
        {
            // let's find the root folder where the .env file can be found
            var rootPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)))));
            var filePath = Path.Combine(rootPath, envFileName);
            // let's remove file://
            filePath = new Uri(filePath).LocalPath;

            if (!File.Exists(filePath))
                return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith("#"))
                {
                    // It's a comment
                    continue;
                }

                var parts = line.Split(
                    '=',
                    2,
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;

                var p0 = parts[0].Trim();
                var p1 = parts[1].Trim();

                if (p1.StartsWith("\""))
                {
                    p1 = p1[1..^1];
                }

                Environment.SetEnvironmentVariable(p0, p1);
            }
        }
    }
}
