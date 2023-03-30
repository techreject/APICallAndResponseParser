using System;
using System.Collections.Generic;
using System.Net.Http;
// 03/29/2023 - Added a reference to System.Threading.Tasks.Extensions in order to use JsonSerializer
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.IO;
// 03/29/2023 - Added a reference to System.Text.Json under the reference listing in order to use JsonSerializer
using System.Text.Json;
using System.Linq.Expressions;
using System.Threading;
using System.Linq;

namespace APICallAndJSONParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 03/29/2023 - Use an HttpClient to handle requests and responses
            HttpClient client = new HttpClient();

            // 03/29/2023 - Sets up HTTP headers for all requests:
            // An Accept header to accept JSON responses
            //
            // Accept Header - The Accept request HTTP header indicates
            // which content types, expressed as MIME types, the client
            // is able to understand. The server uses content negotiation
            // to select one of the proposals and informs the client of the
            // choice with the Content-Type response header. Browsers set
            // required values for this header based on the context of the
            // request.
            //
            // For example, a browser uses different values in a request when fetching a CSS stylesheet, image, video, or a script
            //
            // More info at https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            // 03/29/2023 - A User Agent header
            // User-Agent Header The User-Agent request header is a characteristic string that lets servers and network peers
            // identify the application, operating system, vendor, and/or version of the requesting user agent.
            //
            // More information at https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent
            //
            // .These headers are checked by the GitHub server code and are necessary to retrieve information from GitHub.
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            // 03/29/2023 - Use the HttpClient class to make HTTP requests.
            // HttpClient supports only async methods for its long-running APIs.
            // So the following steps create an async method and call it from the Main method.
            ProcessRepositoriesAsyncViaConsole(client).Wait();
        }

        // 03/29/2023 - Method calls the GitHub endpoint that returns a list of all repositories under
        // the .NET foundation organization in a console window for viewing
        static async Task ProcessRepositoriesAsyncViaConsole(HttpClient client)
        {
            // 03/29/2023 - Awaits the task returned from calling HttpClient.GetStringAsync(String) method.This method sends an HTTP GET request to the specified URI.The body of the response is returned as a String, which is available when the task completes.
            var json = await client.GetStringAsync("https://api.github.com/orgs/dotnet/repos");

            // 03/29/2023 - The response string json is printed to the console.
            Console.WriteLine(json);

            // 03/29/2023 - Gives pause so you can see output on the command line
            Console.Write("Press Enter to continue.");
            Console.ReadLine();
        }
        // 03/29/2023 - Method calls the GitHub endpoint that returns a list of all repositories under
        // the .NET foundation organization. The list - returned in json format - is then deserailized and
        // saved

        static async Task ProcessRepositoriesAsync(HttpClient client)
        {
            // 03/29/2023 - Use the serializer to convert JSON into C# objects

            // 03/29/2023 - This serializer method uses a stream instead of a string as its source. The first argument
            // to JsonSerializer.DeserializeAsync<TValue>(Stream, JsonSerializerOptions, CancellationToken)
            // is an await expression.await expressions can appear almost anywhere in your code, even though
            // up to now, you've only seen them as part of an assignment statement. The other two parameters,
            // JsonSerializerOptions and CancellationToken, are optional and are omitted in the code snippet.
            Stream stream = await client.GetStreamAsync("https://api.github.com/orgs/dotnet/repos");
            // 03/29/2023 - The DeserializeAsync method is generic, which means you supply type arguments for what kind of
            // objects should be created from the JSON text. In this example, you're deserializing to a List<Repository>,
            // which is another generic object, a System.Collections.Generic.List<T>. The List<T> class stores a collection
            // of objects. The type argument declares the type of objects stored in the List<T>. The type argument is your
            // Repository record, because the JSON text represents a collection of repository objects.
            var repositories = await JsonSerializer.DeserializeAsync<List<Repository>>(stream);

            // 03/29/2023 - allows user to control whether to see ALL repos returned, or one at a time
            var showAll = false;
            foreach (var repo in repositories ?? Enumerable.Empty<Repository>())
            {
                // 03/29/2023 - go through the list at least once and show one item. Then ask user to continue
                // to show one at a time, or all of the items
                if (!showAll)
                {
                    // 03/29/2023 - Output the json value that was deserialized
                    Console.Write(repo.name);

                    // 03/29/2023 - Gives pause so you can see output on the command line. If user continues to
                    // press enter, it will show 1 record at a time. If user enters -1, then it will proceed
                    // to set a flag, preventing the input field from showing and displaying all values in the 
                    // the command line
                    Console.Write("Press Enter to continue. Or input -1 to list all");
                    // 03/29/2023 - Store the response from the user
                    var response = Console.ReadLine();

                    // 03/29/2023 - If the response is -1, set a flag, so that the next time the loop runs
                    // it will continue to run and display ALL records
                    if (response == "-1")
                    {
                        showAll = true;
                    }
                }
                else
                {
                    // 03/29/2023 - Output the json value that was deserialized
                    Console.Write(repo.name);
                }
            }
            // Continue program - this allows a pause in the app, giving the user time to read through the output
            Console.Write("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
