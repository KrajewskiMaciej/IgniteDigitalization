using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class GameProcess
    {
        public int Games_Processes_Id { get; set; }
        public int Processes_Id { get; set; }
        public Process Processes { get; set; } = null!;

        public int Games_Id { get; set; }
        public Game Games { get; set; } = null!;

        public int Teams_Id { get; set; }
        public Team Teams { get; set; } = null!;
        public int? Games_Processes_Weights { get; set; }
    }
}