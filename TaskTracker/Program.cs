using System.CommandLine;


namespace TaskTracker
{
    class Program
    {
        static void Main(string[] args)
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
        }
    }
}