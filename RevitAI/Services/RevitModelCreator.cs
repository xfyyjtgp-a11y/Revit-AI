using Autodesk.Revit.DB;
using System;
using System.Linq;

namespace RevitAI.Services
{
    public class WallRequest
    {
        public double Length { get; set; }
        public double Height { get; set; }
        public string LevelName { get; set; } = "Level 1";
    }

    public class RevitModelCreator
    {
        private Document _doc;

        public RevitModelCreator(Document doc)
        {
            _doc = doc;
        }

        public void CreateWall(WallRequest request)
        {
            // Convert inputs from Meters to Internal Units (Feet)
            double lengthFeet = UnitUtils.ConvertToInternalUnits(request.Length, UnitTypeId.Meters);
            double heightFeet = UnitUtils.ConvertToInternalUnits(request.Height, UnitTypeId.Meters);

            Level level = GetLevel(request.LevelName);
            // Fallback to first level if named level not found
            if (level == null)
            {
                level = new FilteredElementCollector(_doc)
                    .OfClass(typeof(Level))
                    .FirstElement() as Level;
            }

            if (level == null) throw new InvalidOperationException("No levels found in the document.");

            // Create geometry line (along X axis for simplicity)
            XYZ start = XYZ.Zero;
            XYZ end = new XYZ(lengthFeet, 0, 0);
            Line geomLine = Line.CreateBound(start, end);

            // Get a Wall Type
            WallType wallType = new FilteredElementCollector(_doc)
                .OfClass(typeof(WallType))
                .WhereElementIsElementType()
                .Cast<WallType>()
                .FirstOrDefault(wt => wt.Kind == WallKind.Basic);

            if (wallType == null) throw new InvalidOperationException("No basic WallType found.");

            using (Transaction t = new Transaction(_doc, "AI Create Wall"))
            {
                t.Start();
                
                // Create the wall
                // Wall.Create(Document, Curve, ElementId, ElementId, double height, double offset, bool flip, bool structural)
                Wall.Create(_doc, geomLine, wallType.Id, level.Id, heightFeet, 0.0, false, false);
                
                t.Commit();
            }
        }

        private Level? GetLevel(string name)
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) as Level;
        }
    }
}
