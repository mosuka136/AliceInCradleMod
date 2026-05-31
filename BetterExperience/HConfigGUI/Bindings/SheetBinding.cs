using BetterExperience.HConfigSpace;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI.Bindings
{
    public class SheetBinding
    {
        public List<TableBinding> Sheet { get; set; }

        private SheetBinding(List<TableBinding> sheet)
        {
            Sheet = sheet;
        }

        public static SheetBinding CreateSheet(ConfigSheet sheet)
        {
            var tableBindings = new List<TableBinding>();

            foreach (var table in sheet)
            {
                var entryBindings = new List<IEntryBinding>();
                foreach (var entry in table.Value)
                    entryBindings.Add(new EntryBinding(entry));
                tableBindings.Add(new TableBinding(entryBindings, table.Value.Name, table.Value.Description));
            }

            return new SheetBinding(tableBindings);
        }
    }
}
