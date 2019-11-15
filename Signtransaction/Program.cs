using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Signtransaction
{
    class Program
    {
        static void Main(string[] args)
        {
            TestSign2("0x17ce997c70ef691ea5c828c29da57f5e5feb1b39");
            //TestSign2("0x1c36d5f944e6bd62df4f2a9e2bdd610dc932d647");
            //a83c93030a968a8d0ccf37f498743");
            Console.ReadLine();
        }

        static async Task TestSign()
        {
            var client = new HttpClient();

            var sw = new Stopwatch();
            //sw.Start();

            var res = await client.PostAsync("http://quorum-transaction-signer.sensitive-data.svc.cluster.local/api/wallets/0x29dd3193e36b1f35095b287cdabacc03407925ee/batchsign",
                new JsonContent(new List<string>()
                {
                    "cxItdvSIa34E1aHV9edFCFZgT7KODZE8wKWR8YH2VKM=",
                    "RDcvIeGvIYDb2GN8HthZ7O+Qa27W+daNeDF6/19NfUs=",
                    "Hah1LhUou2sAMje9yX2sIoHDIVuALheLgfBpOOjSVXo=",
                    "mZD62kqjYVn2OAy1CxFu5f2COCmLAvk91r4v8MF7DHY=",
                    "zB7poMqjGWmAd5Mna4EmgC6/6HQWf++fooFuMK3ivP4="
                }));

            if (res.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"responce: {res.StatusCode}");
                Console.WriteLine(res.Content.ReadAsStringAsync().Result);
            }

            sw.Start();

            var list = new List<Task>();

            for (int i = 0; i < 5; i++)
            {
                var res1 = await client.PostAsync(
                    "http://quorum-transaction-signer.sensitive-data.svc.cluster.local/api/wallets/0x29dd3193e36b1f35095b287cdabacc03407925ee/batchsign",
                    new JsonContent(new List<string>()
                    {
                        "cxItdvSIa34E1aHV9edFCFZgT7KODZE8wKWR8YH2VKM=",
                        "RDcvIeGvIYDb2GN8HthZ7O+Qa27W+daNeDF6/19NfUs=",
                        "Hah1LhUou2sAMje9yX2sIoHDIVuALheLgfBpOOjSVXo=",
                        "mZD62kqjYVn2OAy1CxFu5f2COCmLAvk91r4v8MF7DHY=",
                        "zB7poMqjGWmAd5Mna4EmgC6/6HQWf++fooFuMK3ivP4="
                    }));

                if (res1.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"responce: {res.StatusCode}");
                    Console.WriteLine(res.Content.ReadAsStringAsync().Result);
                }
            }


            sw.Stop();
            Console.WriteLine($"Execute 100 requests. Time {sw.Elapsed}. RPS: {100m * 1000 / sw.ElapsedMilliseconds}");
        }

        static async Task TestSign2(string wallet)
        {
            var client = new HttpClient();

            var sw = new Stopwatch();
            //sw.Start();

            var res = await client.PostAsync($"http://quorum-transaction-signer.sensitive-data.svc.cluster.local/api/wallets/{wallet}/batchsign",
                new JsonContent(new List<string>()
                {
                    "cxItdvSIa34E1aHV9edFCFZgT7KODZE8wKWR8YH2VKM=",
                    "RDcvIeGvIYDb2GN8HthZ7O+Qa27W+daNeDF6/19NfUs=",
                    "Hah1LhUou2sAMje9yX2sIoHDIVuALheLgfBpOOjSVXo=",
                    "mZD62kqjYVn2OAy1CxFu5f2COCmLAvk91r4v8MF7DHY=",
                    "zB7poMqjGWmAd5Mna4EmgC6/6HQWf++fooFuMK3ivP4="
                }));

            if (res.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"responce: {res.StatusCode}");
                Console.WriteLine(res.Content.ReadAsStringAsync().Result);
            }

            sw.Start();

            var list = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                list.Add(Task.Run(async () =>
                {
                    var res1 = await client.PostAsync($"http://quorum-transaction-signer.sensitive-data.svc.cluster.local/api/wallets/{wallet}/batchsign",
                        new JsonContent(new List<string>()
                        {
                            "cxItdvSIa34E1aHV9edFCFZgT7KODZE8wKWR8YH2VKM=",
                            "RDcvIeGvIYDb2GN8HthZ7O+Qa27W+daNeDF6/19NfUs=",
                            "Hah1LhUou2sAMje9yX2sIoHDIVuALheLgfBpOOjSVXo=",
                            "mZD62kqjYVn2OAy1CxFu5f2COCmLAvk91r4v8MF7DHY=",
                            "zB7poMqjGWmAd5Mna4EmgC6/6HQWf++fooFuMK3ivP4="
                        }));


                    if (res1.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine($"responce: {res.StatusCode}");
                        Console.WriteLine(res.Content.ReadAsStringAsync().Result);
                    }
                }));



            }
            Task.WaitAll(list.ToArray());
            sw.Stop();
            Console.WriteLine($"Execute 100 requests. Time {sw.Elapsed}. RPS: {100m * 1000 / sw.ElapsedMilliseconds}");
        }


        public class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }
    }
}
