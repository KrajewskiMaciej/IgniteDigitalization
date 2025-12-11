using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class Module
    {
        public int Modules_Id { get; set; }
        public int Decks_Id { get; set; }
        public Deck? Deck { get; set; }
        public string Module_Name { get; set; } = string.Empty;
    }
}