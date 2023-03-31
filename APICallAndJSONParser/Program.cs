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
// 03/30/23 - Installed nuget packages
// System.text.json 7.0.2
// System.NET.Http.json 7.0.1
// This addressed error
//      System.IO.FileNotFoundException HResult=0x80070002 Message=Could not load file or assembly 'System.Memory,
//      Version=4.0.1.1, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The system
//      cannot find the file specified. Source=System.Text.Json StackTrace: at
//      System.Text.Json.JsonSerializerOptions.GetDefaultSimpleConverters() at
//      System.Text.Json.JsonSerializerOptions.RootBuiltInConverters() at
//      System.Text.Json.JsonSerializerOptions.InitializeForReflectionSerializer() at
//      System.Text.Json.JsonSerializer.GetTypeInfo(JsonSerializerOptions options, Type runtimeType) at
//      System.Text.Json.JsonSerializer.DeserializeAsync[TValue](Stream utf8Json, JsonSerializerOptions options, CancellationToken cancellationToken) at
//      APICallAndJSONParser.Program.<ProcessRepositoriesAsyncLoop>d__3.MoveNext() in
//      C:\Users\Matthew\source\repos\APICallAndResponseParser\APICallAndJSONParser\Program.cs:line 108 at
//      System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task) at
//      System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) at
//      System.Runtime.CompilerServices.TaskAwaiter.GetResult() at
//      APICallAndJSONParser.Program.<RunAsync>d__1.MoveNext() in
//      C:\Users\Matthew\source\repos\APICallAndResponseParser\APICallAndJSONParser\Program.cs:line 61 at
//      System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task) at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) at
//      System.Runtime.CompilerServices.TaskAwaiter.GetResult() at APICallAndJSONParser.Program.Main(String[] args) in
//      C:\Users\Matthew\source\repos\APICallAndResponseParser\APICallAndJSONParser\Program.cs:line 20

namespace APICallAndJSONParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }
        // In order to get this to run properly, I had to break this out into two separate functions. I found this 
        // out by looking at the below example on MS:
        //
        // https://learn.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
        static async Task RunAsync()
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

            Console.WriteLine("Outputting non-formatted json content");
            // 03/29/2023 - Use the HttpClient class to make HTTP requests.
            // HttpClient supports only async methods for its long-running APIs.
            // So the following steps create an async method and call it from the Main method.
            await ProcessRepositoriesAsyncViaConsole(client);

            Console.WriteLine("Outputting json list");
            // 03/30/23 - This performs an output to the command line, but does this by cycling through each
            // json object and displaying it. This also gives the option to show all of them at once, but wil
            // do this by cycling through each object and not stopping until the end of the list
            await ProcessRepositoriesAsyncLoop(client);

            // 03/30/23 - Create a list of JSON objects and return them from the function to be processed
            // outside of the function
            var repositories = await ProcessRepositoriesAsync(client);

            Console.WriteLine("Outputting json list");
            // 03/30/23 - Cycle through each object in the list
            foreach (var repo in repositories)
            {
                // 03/30/23 - Display those objects on the console window
                Console.WriteLine($"\tName: {repo.Name}");
                Console.WriteLine($"\tHomepage: {repo.Homepage}");
                Console.WriteLine($"\tGitHub: {repo.GitHubHomeUrl}");
                Console.WriteLine($"\tDescription: {repo.Description}");
                Console.WriteLine($"\tWatchers: {repo.Watchers:#,0}");
                Console.WriteLine($"\tLast push: {repo.ConvertToEST(repo.LastPushUtc)}");
                Console.WriteLine();
            }

            // 03/30/2023 - Gives pause so you can see output on the command line
            Console.Write("Press Enter to continue.");
            Console.ReadLine();
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

        static async Task ProcessRepositoriesAsyncLoop(HttpClient client)
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
                    Console.WriteLine($"\tName: {repo.Name}");
                    Console.WriteLine($"\tHomepage: {repo.Homepage}");
                    Console.WriteLine($"\tGitHub: {repo.GitHubHomeUrl}");
                    Console.WriteLine($"\tDescription: {repo.Description}");
                    Console.WriteLine($"\tWatchers: {repo.Watchers:#,0}");
                    Console.WriteLine($"\tLast push: {repo.ConvertToEST(repo.LastPushUtc)}");
                    Console.WriteLine();

                    // 03/29/2023 - Gives pause so you can see output on the command line. If user continues to
                    // press enter, it will show 1 record at a time. If user enters -1, then it will proceed
                    // to set a flag, preventing the input field from showing and displaying all values in the 
                    // the command line
                    Console.WriteLine("Press Enter to continue. Or input -1 to list all");
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
                    Console.WriteLine($"\tName: {repo.Name}");
                    Console.WriteLine($"\tHomepage: {repo.Homepage}");
                    Console.WriteLine($"\tGitHub: {repo.GitHubHomeUrl}");
                    Console.WriteLine($"\tDescription: {repo.Description}");
                    Console.WriteLine($"\tWatchers: {repo.Watchers:#,0}");
                    Console.WriteLine($"\tLast push: { repo.ConvertToEST(repo.LastPushUtc) }");
                    Console.WriteLine();
                }
            }
            // 03/29/2023 - Continue program - this allows a pause in the app, giving the user time to read through the output
            Console.WriteLine("Fin... Press Enter to close the window.");
            Console.ReadLine();
        }
        // 03/30/23 - The ProcessRepositoriesAsync method can do the async work and return a collection of the repositories.
        // Change that method to return Task<List<Repository>>
        static async Task<List<Repository>> ProcessRepositoriesAsync(HttpClient client)
        {
            // 03/30/23 - Return the repositories after processing the JSON response:
            Stream stream = await client.GetStreamAsync("https://api.github.com/orgs/dotnet/repos");
            var repositories = await JsonSerializer.DeserializeAsync<List<Repository>>(stream);

            // 03/30/23 handles if the variable is null, it will set it to, set it to something, even
            // if the object is empty
            if (repositories == null)
            {
                repositories = new List<Repository>();
                // 03/30/23 - Added a warning in case this happens, so I know why nothing was outputted
                // I can then troubleshoot
                Console.WriteLine("WARNING: List returned was empty!");
            }

            // 03/30/23 - Return the repositories after processing the JSON response:
            return repositories;
        }
    }
}
