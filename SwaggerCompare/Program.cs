using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace SwaggerCompare
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader1 = new StreamReader("v1.json");
            var reader2 = new StreamReader("v2.json");

            var json1 = reader1.ReadToEnd().Replace("$ref", "reff");
            var json2 = reader2.ReadToEnd().Replace("$ref", "reff");

            var settings = new JsonSerializerSettings();
            settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;

            var v1 = JsonConvert.DeserializeObject<Swagger>(json1, settings);
            var v2 = JsonConvert.DeserializeObject<Swagger>(json2, settings);

            Console.WriteLine($"Count paths: {v1.paths.Count}; Count Models: {v1.definitions.Count}");
            Console.WriteLine($"Count paths: {v2.paths.Count}; Count Models: {v2.definitions.Count}");
            Console.WriteLine();

            var list1 = new Dictionary<string, SwaggerMethod>();
            foreach (var path in v1.paths)
            {
                if (path.Value.get != null)
                    list1.Add($"GET {path.Key}", path.Value.get);
                if (path.Value.post != null)
                    list1.Add($"POST {path.Key}", path.Value.post);
                if (path.Value.put != null)
                    list1.Add($"PUT {path.Key}", path.Value.put);
                if (path.Value.delete != null)
                    list1.Add($"DELETE {path.Key}", path.Value.delete);
            }
            var list2 = new Dictionary<string, SwaggerMethod>();
            foreach (var path in v2.paths)
            {
                if (path.Value.get != null)
                    list2.Add($"GET {path.Key}", path.Value.get);
                if (path.Value.post != null)
                    list2.Add($"POST {path.Key}", path.Value.post);
                if (path.Value.put != null)
                    list2.Add($"PUT {path.Key}", path.Value.put);
                if (path.Value.delete != null)
                    list2.Add($"DELETE {path.Key}", path.Value.delete);
            }

            Console.WriteLine("New methods:");
            foreach (var pair in list2.Where(i => !list1.ContainsKey(i.Key)).ToArray())
            {
                Console.WriteLine(pair.Key);
                list2.Remove(pair.Key);
            }

            Console.WriteLine();
            Console.WriteLine("Deleted methods:");
            foreach (var pair in list1.Where(i => !list2.ContainsKey(i.Key)).ToArray())
            {
                Console.WriteLine(pair.Key);
            }

            Console.WriteLine();
            Console.WriteLine("Update methods:");
            foreach (var m2 in list2)
            {
                var m1 = list1[m2.Key];

                var j1 = JsonConvert.SerializeObject(m1);
                var j2 = JsonConvert.SerializeObject(m2.Value);
                if (JsonConvert.SerializeObject(m1) != JsonConvert.SerializeObject(m2.Value))
                {
                    Console.WriteLine($"{m2.Key}   01");
                    continue;
                }

                foreach (var p2 in m2.Value.parameters)
                {
                    var p1 = m1.parameters.FirstOrDefault(i => i.name == p2.name);
                    if (p1 == null)
                    {
                        Console.WriteLine($"{m2.Key}   02");
                        continue;
                    }

                    if (p1.schema?.reff != null)
                    {
                        var pm1 = v1.definitions[p1.schema.reff.Replace("#/definitions/", "")];
                        var pm2 = v2.definitions[p2.schema.reff.Replace("#/definitions/", "")];
                        if (JsonConvert.SerializeObject(pm1) != JsonConvert.SerializeObject(pm2))
                        {
                            Console.WriteLine($"{m2.Key}   03");
                            continue;
                        }
                    }
                }
                
                foreach (var p2 in m2.Value.responses)
                {
                    var p1 = m1.responses[p2.Key];
                    if (p1 == null)
                    {
                        Console.WriteLine($"{m2.Key}   04");
                        continue;
                    }

                    if (p1.schema?.reff != null)
                    {
                        

                        var pm1 = v1.definitions[p1.schema.reff.Replace("#/definitions/", "")];
                        var pm2 = v2.definitions[p2.Value.schema.reff.Replace("#/definitions/", "")];

                        if (JsonConvert.SerializeObject(pm1) != JsonConvert.SerializeObject(pm2))
                        {
                            Console.WriteLine($"{m2.Key}   05");
                            continue;
                        }
                    }
                }
            }
        }
    }

    public class Swagger
    {
        public Dictionary<string, SwaggerPath> paths { get; set; }

        public Dictionary<string, SwaggerDefinitions> definitions { get; set; }
    }

    public class SwaggerPath
    {
        public SwaggerMethod get { get; set; }
        public SwaggerMethod post { get; set; }
        public SwaggerMethod put { get; set; }
        public SwaggerMethod delete { get; set; }
    }

    public class SwaggerMethod
    {
        public string summary { get; set; }
        //public string description { get; set; }
        public string operationId { get; set; }
        public string[] consumes { get; set; }
        public string[] produces { get; set; }
        public List<SwaggerMethodParameter> parameters { get; set; }
        public Dictionary<string, SwaggerMethodResponce> responses { get; set; }
    }

    public class SwaggerMethodParameter
    {
        public string name { get; set; }
        [JsonPropertyName("in")]
        public string In { get; set; }
        public bool required { get; set; }

        public SwaggerSchema schema { get; set; }
    }

    public class SwaggerMethodResponce
    {
        public string description { get; set; }

        public SwaggerSchema schema { get; set; }

        
    }

    public class SwaggerSchema
    {
        [JsonPropertyName("reff")]
        public string reff { get; set; }
    }

    public class SwaggerDefinitions
    {
        public string description { get; set; }
        public string type { get; set; }
        public Dictionary<string, SwaggerProperty> properties { get; set; }
    }

    public class SwaggerProperty
    {
        public string type { get; set; }
        public string description { get; set; }

        public SwaggerProperty properties { get; set; }
    }
}
