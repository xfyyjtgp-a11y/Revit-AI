using Autodesk.Revit.DB;
using RevitAI.Attributes;
using RevitAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace RevitAI.Services
{
    public class RevitTaskProcessor
    {
        private readonly Document _doc;
        private readonly Dictionary<string, MethodInfo> _actionHandlers;

        public RevitTaskProcessor(Document doc)
        {
            _doc = doc;
            _actionHandlers = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            // Scan current class for methods with RevitTaskAction attribute
            var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<RevitTaskActionAttribute>();
                if (attr != null)
                {
                    _actionHandlers[attr.ActionName] = method;
                }
            }
        }

        public void ProcessTasks(List<RevitTask> tasks)
        {
            using Transaction t = new(_doc, "AI Model Generation");
            t.Start();

            foreach (var task in tasks)
            {
                try
                {
                    if (_actionHandlers.TryGetValue(task.ActionName, out var method))
                    {
                        // Invoke the handler dynamically
                        method.Invoke(this, new object[] { task.Parameters });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: No handler found for action '{task.ActionName}'");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing task {task.ActionName}: {ex.Message}");
                }
            }

            t.Commit();
        }

        [RevitTaskAction("create_wall")]
        private void ProcessCreateWall(Dictionary<string, object> parameters)
        {
            // Extract parameters safely
            double length = GetDouble(parameters, "Length");
            double height = GetDouble(parameters, "Height");
            string levelName = GetString(parameters, "LevelName") ?? "Level 1";

            // Logic from original RevitModelCreator
            
            // Convert inputs from Meters to Internal Units (Feet)
            double lengthFeet = UnitUtils.ConvertToInternalUnits(length, UnitTypeId.Meters);
            double heightFeet = UnitUtils.ConvertToInternalUnits(height, UnitTypeId.Meters);

            Level level = GetLevel(levelName);
            // Fallback to first level if named level not found
            if (level == null)
            {
                level = new FilteredElementCollector(_doc)
                    .OfClass(typeof(Level))
                    .FirstElement() as Level;
            }

            if (level == null) throw new InvalidOperationException("No levels found in the document.");

            // Create geometry line (along X axis for simplicity)
            // Ideally we should calculate position based on previous elements or user input
            XYZ start = XYZ.Zero;
            XYZ end = new XYZ(lengthFeet, 0, 0);
            Line geomLine = Line.CreateBound(start, end);

            // Get a Wall Type
            WallType? wallType = new FilteredElementCollector(_doc)
                .OfClass(typeof(WallType))
                .WhereElementIsElementType()
                .Cast<WallType>()
                .FirstOrDefault(wt => wt.Kind == WallKind.Basic);

            if (wallType == null) throw new InvalidOperationException("No basic WallType found.");

            // Create the wall
            Wall.Create(_doc, geomLine, wallType.Id, level.Id, heightFeet, 0.0, false, false);
        }

        // Example for future extension:
        // [RevitTaskAction("create_door")]
        // private void ProcessCreateDoor(Dictionary<string, object> parameters) { ... }

        private Level? GetLevel(string name)
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) as Level;
        }

        // Helper methods to handle JSON element conversion if needed
        private double GetDouble(Dictionary<string, object> dict, string key)
        {
            if (!dict.TryGetValue(key, out var val)) return 0.0;
            
            if (val is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Number)
                    return element.GetDouble();
            }
            if (val is double d) return d;
            if (val is int i) return (double)i;
            if (val is long l) return (double)l;
            
            return Convert.ToDouble(val);
        }

        private string? GetString(Dictionary<string, object> dict, string key)
        {
            if (!dict.TryGetValue(key, out var val)) return null;
            
            if (val is JsonElement element)
            {
                return element.ToString();
            }
            return val?.ToString();
        }
    }
}
