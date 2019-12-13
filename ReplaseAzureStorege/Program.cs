using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApplicationInsight;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ReplaseAzureStorege
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("help") || !args.Any())
            {
                Console.WriteLine("params:");
                Console.WriteLine("  list - show list of all storage accounts");
                Console.WriteLine("  replace oldprefix newprefix resourcegroup subscription - replace to new storage account");

                Console.Write("Command (list, replace): ");
                var command = Console.ReadLine();
                if (command == "list")
                {
                    args = new string[] { "list" };
                }

                if (command == "replace")
                {
                    args = new string[5];
                    args[0] = "replace";
                    Console.Write("oldprefix: "); args[1] = Console.ReadLine();
                    Console.Write("newprefix: "); args[2] = Console.ReadLine();
                    Console.Write("resourcegroup: "); args[3] = Console.ReadLine();
                    Console.Write("subscription: "); args[4] = Console.ReadLine();
                }
            }



            //var connection = "";
            Console.Write("Connection string: "); var connection = Console.ReadLine();

            var connectionString = ConstantReloadingManager.From(connection);

            var keyValueTableStorage =
                AzureTableStorage<KeyValueEntity>.Create(
                    connectionString,
                    "KeyValues",
                    EmptyLogFactory.Instance);

            var data = keyValueTableStorage.GetDataAsync().Result;

            var azureStorage = data.Where(i =>
                i.Value != null &&
                i.Value.Contains("DefaultEndpointsProtocol=https") && i.Value.Contains("AccountKey=") &&
                i.Value.Contains("EndpointSuffix=core.windows.net")).ToList();
                

            Console.WriteLine($"Count keys: {data.Count}, count storages: {azureStorage.Count}");

            var names = new List<(string, KeyValueEntity)>();
            foreach (var entity in azureStorage)
            {
                var pattern = "AccountName=(.*?);";
                var r = Regex.Match(entity.Value, pattern);
                var name = r.Groups[1].Value;
                names.Add((name, entity));
            }

            if (args.Contains("list"))
            {
                foreach (var name in names.Select(e => e.Item1).OrderBy(e => e))
                {
                    Console.WriteLine(name);
                }
                return;
            }

            if (args.Length == 5 && args[0] == "replace")
            {
                var oldprefix = args[1];
                var newPrefix = args[2];
                var resourcegroup = args[3];
                var subscription = args[4];

                foreach (var entity in names)
                {
                    try
                    {

                        if (entity.Item1.StartsWith(oldprefix))
                        {
                            var newName = entity.Item1.Replace(oldprefix, newPrefix);

                            var responce = $" storage account check-name -n {newName}".AZ();
                            var result = JsonConvert.DeserializeObject<StorageAccountCheckNameResponce>(responce);

                            if (!result.nameAvailable)
                            {
                                Console.WriteLine(
                                    $"WARNING. {entity.Item2.RowKey}: {entity.Item1} --> {newName} - name not available");
                                //continue;
                            }

                            responce =
                                $"storage account create --name {newName} --resource-group {resourcegroup} --subscription {subscription}".AZ();
                            if (!responce.Contains("Id") || !responce.Contains("creationTime"))
                            {
                                Console.WriteLine(
                                    $"ERROR. {entity.Item2.RowKey}: {entity.Item1} --> {newName} - {responce}");
                                continue;
                            }

                            responce = $"storage account show-connection-string -g {resourcegroup} -n {newName}".AZ();
                            var connString = JsonConvert.DeserializeObject<StorageAccountConnectionString>(responce);

                            entity.Item2.Value = connString.connectionString;

                            keyValueTableStorage.ReplaceAsync(entity.Item2).Wait();

                            Console.WriteLine(
                                $"OK. {entity.Item2.RowKey}: {entity.Item1} --> {newName}");
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"EXCEPTION. {entity.Item2.RowKey}");
                        Console.WriteLine(ex);
                    }

                }

            }
        }
    }
}
