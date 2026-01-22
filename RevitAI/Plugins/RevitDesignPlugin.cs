using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Collections.Generic;
using RevitAI.Models;

namespace RevitAI.Plugins
{
    public class RevitDesignPlugin
    {
        // Store tasks here so the main application can retrieve them
        public List<RevitTask> PendingTasks { get; } = new List<RevitTask>();

        [KernelFunction("create_wall")]
        [Description("Creates a wall in the Revit model with specified dimensions.")]
        public string CreateWall(
            [Description("The length of the wall in meters.")] double length,
            [Description("The height of the wall in meters.")] double height,
            [Description("The name of the level to place the wall on (e.g., 'Level 1').")] string levelName = "Level 1")
        {
            var task = new RevitTask
            {
                ActionName = "create_wall",
                Parameters = new Dictionary<string, object>
                {
                    { "Length", length },
                    { "Height", height },
                    { "LevelName", levelName }
                }
            };
            
            PendingTasks.Add(task);

            return $"Wall creation task added: Length={length}m, Height={height}m, Level={levelName}.";
        }

        // Future methods can be added here, e.g., CreateDoor, CreateWindow
        // [KernelFunction("create_door")] ...
    }
}
