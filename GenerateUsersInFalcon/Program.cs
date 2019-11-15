using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GenerateUsersInFalcon
{
    class Program
    {
        static void Main(string[] args)
        {
            var token = "368386e62ef249418b26ff33bbf3d91621e9918073274696a074b8d99070ae3c";

            Console.WriteLine("1 - Register");
            Console.WriteLine("2 - Login");
            Console.WriteLine("3 - Transfer");
            Console.Write("select command: ");
            var command = Console.ReadLine();
            switch (command)
            {
                case "1": Register(); break;;
                case "2": TEst(); break;
                case "3": Transfer(token).Wait(); break;
            }
            
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

        private static void TEst()
        {
            Console.Write("start index: ");
            var startindex = int.Parse(Console.ReadLine());
            Console.Write("end index: ");
            var endindex = int.Parse(Console.ReadLine());

            var client = new HttpClient();
            for (int i = startindex; i < endindex; i++)
            {
                var url =
                    //"https://customer-api.falcon-dev.open-source.exchange/api/auth/login";
                    "https://cupi.emrtoken.emaar.com/api/auth/login";


                var email = $"anovichikhin.test+{i}@gmail.com";
                //Console.WriteLine(email);
                var res = client.PostAsync(url,
                    new JsonContent(new
                    {
                        Email = email,
                        Password = "Test123456!"
                    })).Result;

                if (res.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"ERROR: {email} : {res.StatusCode}");
                }

                if (i % 100 == 0)
                {
                    Console.WriteLine(email);
                }
            }
        }

        private static void Register()
        {
            var client = new HttpClient();
            var url =
                //"https://cupi.emrtoken.emaar.com/api/customers/register";
                "https://customer-api.falcon-dev.open-source.exchange/api/customers/register";

            Console.Write("start index: ");
            var startindex = int.Parse(Console.ReadLine());
            Console.Write("end index: ");
            var endindex = int.Parse(Console.ReadLine());


            for (int i = startindex; i < endindex; i++)
            {
                var email = $"anovichikhin.test+{i}@gmail.com";
                Console.WriteLine(email);
                var res = client.PostAsync(url,
                    new JsonContent(new
                    {
                        Email = email,
                        Password = "Test123456!",
                        FirstName = "Ivan test",
                        LastName = "Ivanov test",
                        CountryOfNationalityId = 7
                    })).Result;

                if (res.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"ERROR: {res.StatusCode}");
                }

            }


        }

        private static async Task Transfer(string token)
        {
            int startindex;
            int endindex;
            Console.Write("token=");
            var t = Console.ReadLine();
            if (!string.IsNullOrEmpty(token))
                token = t;
            Console.Write("startindex=");
            startindex = int.Parse(Console.ReadLine());
            Console.Write("endindex=");
            endindex = int.Parse(Console.ReadLine());
            


            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = "https://cupi.emrtoken.emaar.com/api/wallets/transfer";

            var index = 0;

            for (int i = startindex; i <= endindex; i++)
            {

                var res = await client.PostAsync(url,
                    new JsonContent(new
                    {
                        ReceiverEmail = $"anovichikhin.test+{1}@gmail.com",
                        Amount = "10"
                    }));

                Console.WriteLine($"{++index}. test+{i}@test.com : {res.StatusCode} | {res.Content.ReadAsStringAsync().Result}");


            }
        }
    }

    public class JsonContent : StringContent
    {
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        { }
    }
}
