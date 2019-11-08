using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace ApplicationInsight
{
    class Program
    {
        

        static void Main(string[] args)
        { 
            var yamlroot = @"d:\lykke\emaar\emaar-live-service-yamls\";
            var subscriptonId = "96a9bf3a-12ef-49ce-ae1f-c8f9692ab0d4";
            var rgname = "EMR_CT_Prod_InfraStorageAccountsRG";
            var location = "northeurope";

            var deployments = Directory.GetFiles(yamlroot, "deployment.yaml", SearchOption.AllDirectories);

            foreach (var deployment in deployments)
            {
                Console.WriteLine(deployment);
                string content;
                using (var file = new StreamReader(deployment))
                {
                    content = file.ReadToEnd();
                }

                var deserializer = new DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .Build();
                var dict = deserializer.Deserialize<Deployment>(content);

                var appname = $"{dict.metadata.@namespace}.{dict.metadata.name}";

                Console.WriteLine($"application: {appname}");
                
                content = content
                    .Replace("#        - name: APPINSIGHTS_INSTRUMENTATIONKEY", "        - name: APPINSIGHTS_INSTRUMENTATIONKEY")
                    .Replace("#          value: ", "          value: ");

                content = content
                    .Replace(@"        - name: APPINSIGHTS_INSTRUMENTATIONKEY
          value: 
        - name: MyMonitoringUrl", @"        - name: APPINSIGHTS_INSTRUMENTATIONKEY
          value: APPINSIDEKEYVALUE
        - name: MyMonitoringUrl");

                if (!content.Contains("APPINSIGHTS_INSTRUMENTATIONKEY"))
                {
                    content += @"        - name: APPINSIGHTS_INSTRUMENTATIONKEY
          value: APPINSIDEKEYVALUE";
                }

                var cmd =
                    $"monitor app-insights component create --app {appname} --location {location} --kind web -g {rgname}  --application-type web --subscription {subscriptonId}";

                Console.WriteLine($"az {cmd}");

                var res = cmd.AZ();

                var result = JsonConvert.DeserializeObject<AzCreateApp>(res);

                Console.WriteLine($"key: {result.instrumentationKey}");

                content = content.Replace("APPINSIDEKEYVALUE", result.instrumentationKey);

                using (var file = new StreamWriter(deployment))
                {
                    file.Write(content);
                }
            }

            
            
        }
    }

    public class Deployment
    {
        public Metadata metadata { get; set; }

        public class Metadata
        {
            public string name { get; set; }
            public string @namespace { get; set; }
        }
    }

    public class AzCreateApp
    {
        public string instrumentationKey { get; set; }
    }
}
