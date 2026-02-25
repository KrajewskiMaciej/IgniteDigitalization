using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class Feedback
    {
        public int Feedbacks_Id { get; set; }
        public int Cards_Id { get; set; }
        public Card Cards { get; set; } = null!;
        public bool Status { get; set; }
        [Column(TypeName = "TEXT")]
        public string Feedbacks_Long_Description { get; set; } = string.Empty;
        [Column(TypeName = "bytea")]
        public byte[]? Feedbacks_PDF { get; set; }
    }
}