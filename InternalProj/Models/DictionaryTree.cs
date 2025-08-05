// ViewModels/DictionaryTree.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalProj.ViewModels
{
    [Table("DictionaryTree")]
    public class DictionaryTree
    {
        public string Title { get; set; }  // e.g., "Dictionary"
        public List<DictionaryItem> Children { get; set; }
    }

    public class DictionaryItem
    {
        public string Name { get; set; }  // e.g., "State"
        public string Url { get; set; }   // optional, currently unused
    }
}
