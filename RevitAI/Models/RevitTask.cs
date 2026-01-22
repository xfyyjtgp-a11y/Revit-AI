using System.Collections.Generic;

namespace RevitAI.Models
{
    public class RevitTask
    {
        /// <summary>
        /// The name of the action to perform (e.g., "create_wall", "create_door").
        /// </summary>
        public string ActionName { get; set; } = string.Empty;

        /// <summary>
        /// The parameters required for the action.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
