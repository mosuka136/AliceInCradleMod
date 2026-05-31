using BetterExperience.HTranslatorSpace;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI.Bindings
{
    public class TableBinding
    {
        public IEnumerable<IEntryBinding> Table { get; set; }
        public Translator Name { get; set; }
        public Translator Description { get; set; }

        public TableBinding(IEnumerable<IEntryBinding> table, Translator name, Translator description)
        {
            Table = table;
            Name = name;
            Description = description;
        }
    }
}
