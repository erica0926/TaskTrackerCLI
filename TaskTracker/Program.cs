using System.CommandLine;
using System.Text.Json;


namespace TaskTracker
{
    class Program
    {
        static readonly string filePath = AppContext.BaseDirectory + "/taskitems.json";

        static int Main(string[] args)
        {
            Argument<int> taskIdArgument = new("id")
            {
                Description = "Task ID"
            };

            Argument<string> descriptionArgument = new("description")
            {
                Description = "Task description"
            };

            Option<bool> verboseOption = new("--verbose")
            {
                Description = "Show detailed output",
                Recursive = true
            };

            Command addCommand = new("add", "Add a new task")
            {
                Arguments = { descriptionArgument }
            };

            Command updateCommand = new("update", "Update an existing task")
            {
                Arguments = { taskIdArgument, descriptionArgument }
            };

            Command deleteCommand = new("delete", "Delete an existing task")
            {
                Arguments = { taskIdArgument }
            };

            Command listCommand = new("list", "List all tasks")
            {
                Options = { verboseOption }
            };


            RootCommand rootCommand = new("A simple task tracker CLI")
            {
                Options = { verboseOption },
                Subcommands = { addCommand, updateCommand, deleteCommand, listCommand }
            };


            addCommand.SetAction(ParseResult =>
            {
                var tasks = LoadTasks();

                var description = ParseResult.GetValue(descriptionArgument)!;
                int id = tasks.Count > 0 ? (tasks.Count + 1) : 1;

                TaskItem task = new(id, description, Status.ToDo, DateTime.Now, DateTime.Now);

                tasks.Add(task);
                SaveTasks(tasks);
                Console.WriteLine($"Added task {id}: {description}");
            });

            return rootCommand.Parse(args).Invoke();

        }

        public enum Status { ToDo, InProgress, Done }

        public record TaskItem(int Id, string Description, Status Status, DateTime CreatedAt, DateTime UpdatedAt);


        static List<TaskItem> LoadTasks()
        {
            List<TaskItem> tasks = [];

            if (!File.Exists(filePath))
            {
                using (File.Create(filePath)) { }
                Console.WriteLine("Empty file created.");
            }
            else
            {
                Console.WriteLine(filePath);
                string jsonString = File.ReadAllText(filePath);
                tasks = JsonSerializer.Deserialize<List<TaskItem>>(jsonString)!;
            }

            return tasks;
        }

        async static void SaveTasks(List<TaskItem> content)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(content, options);


            await File.WriteAllTextAsync(filePath, jsonString);
        }
    }
}