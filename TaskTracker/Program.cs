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

            Option<Status?> statusOption = new("--status")
            {
                Description = "List based on status",
                DefaultValueFactory = _ => null
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
                Options = { statusOption }
            };

            Command markInProgressCommand = new("mark-in-progress", "Update status of a task to 'in progress'")
            {
                Arguments = { taskIdArgument }
            };

            Command markDoneCommand = new("mark-done", "Update status of a task to 'done'")
            {
                Arguments = { taskIdArgument }
            };

            Command markToDoCommand = new("mark-todo", "Update status of a task to 'to do'")
            {
                Arguments = { taskIdArgument }
            };


            RootCommand rootCommand = new("A simple task tracker CLI app")
            {
                Options = { verboseOption, statusOption },
                Subcommands = { addCommand, updateCommand, deleteCommand, listCommand, markInProgressCommand,
                                markDoneCommand, markToDoCommand }
            };


            addCommand.SetAction(parseResult =>
            {
                var tasks = LoadTasks();
                var description = parseResult.GetValue(descriptionArgument)!;
                int id = tasks.Count == 0 ? 1 : (tasks.Last().Id + 1);

                TaskItem task = new(id, description, Status.ToDo, DateTime.Now, DateTime.Now);

                tasks.Add(task);
                SaveTasks(tasks);
                Console.WriteLine($"Task added: {id} - {description}");
            });

            updateCommand.SetAction(parseResult =>
            {
                var tasks = LoadTasks();
                var description = parseResult.GetValue(descriptionArgument)!;
                var id = parseResult.GetValue(taskIdArgument);
                int index = 0;

                try
                {
                    index = tasks.FindIndex(t => t.Id == id);
                    tasks[index] = tasks[index] with
                    {
                        Description = description,
                        UpdatedAt = DateTime.Now
                    };

                    SaveTasks(tasks);
                    Console.WriteLine($"Task updated: {id} - {description}");
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.Error.WriteLine($"Task {id} not found.");
                }

            });

            deleteCommand.SetAction(parseResult =>
            {
                var tasks = LoadTasks();
                var id = parseResult.GetValue(taskIdArgument);

                var task = tasks.FirstOrDefault(t => t.Id == id);

                if (task is null)
                {
                    Console.Error.WriteLine($"Task {id} not found.");
                    return -1;
                }

                tasks.Remove(task);
                SaveTasks(tasks);
                Console.WriteLine($"Task deleted: {id} - {task.Description}");

                return 0;
            });

            listCommand.SetAction(parseResult =>
            {
                var tasks = LoadTasks();

                var verbose = parseResult.GetValue(verboseOption);
                Status? statusList = parseResult.GetValue(statusOption);

                if (statusList != null)
                {
                    Console.WriteLine(statusList);
                }


                string statusString = "";

                foreach (var item in tasks)
                {
                    statusString = item.Status switch
                    {
                        Status.ToDo => "[ ]",
                        Status.InProgress => "[-]",
                        Status.Done => "[✓]",
                        _ => "[ ]",
                    };

                    if (statusList is null)
                    {
                        Console.WriteLine($" {statusString} {item.Id}: {item.Description}");
                    }
                    else if (statusList == item.Status)
                    {
                        Console.WriteLine($" {statusString} {item.Id}: {item.Description}");
                    }

                    if (verbose)
                    {
                        Console.WriteLine($"Date created: {item.CreatedAt}");
                        Console.WriteLine($"Date updated: {item.UpdatedAt}");
                        Console.WriteLine();
                    }
                }
            });

            markInProgressCommand.SetAction(parseResult =>
            {
                var tasks = LoadTasks();
                var id = parseResult.GetValue(taskIdArgument);

                var task = tasks.FirstOrDefault(t => t.Id == id);
                if (task is null)
                {
                    Console.Error.Write($"Task {id} not found.");
                    return -1;
                }

                tasks[tasks.IndexOf(task)] = task with { Status = Status.InProgress };
                SaveTasks(tasks);

                Console.WriteLine($"Task {id} marked in progress.");
                return 0;
            });

            markDoneCommand.SetAction(parseResult =>
            {
                var tasks = LoadTasks();
                var id = parseResult.GetValue(taskIdArgument);

                var task = tasks.FirstOrDefault(t => t.Id == id);

                if (task is null)
                {
                    Console.Error.WriteLine($"Task {id} not found.");
                    return -1;
                }

                tasks[tasks.IndexOf(task)] = task with { Status = Status.Done };
                SaveTasks(tasks);

                Console.WriteLine($"Task {id} marked done.");
                return 0;
            });

            markToDoCommand.SetAction(parseResult =>
            {
                var tasks = LoadTasks();
                var id = parseResult.GetValue(taskIdArgument);

                var task = tasks.FirstOrDefault(t => t.Id == id);

                if (task is null)
                {
                    Console.Error.WriteLine($"Task {id} not found.");
                    return -1;
                }

                tasks[tasks.IndexOf(task)] = task with { Status = Status.ToDo };
                SaveTasks(tasks);

                Console.WriteLine($"Task {id} marked as to do.");
                return 0;
            });

            return rootCommand.Parse(args).Invoke();
        }

        public enum Status { ToDo, InProgress, Done }

        public record TaskItem(int Id, string Description, Status Status, DateTime? CreatedAt, DateTime UpdatedAt);

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