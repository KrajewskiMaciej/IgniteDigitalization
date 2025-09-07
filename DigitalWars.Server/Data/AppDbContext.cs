using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Board> Boards { get; set; }
        // ... reszta Twoich DbSet ...
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Decision> Decisions { get; set; }
        public DbSet<Hardware> Hardwares { get; set; }
        public DbSet<Software> Softwares { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<CardWeight> CardWeights { get; set; }
        public DbSet<CardEnabler> CardEnablers { get; set; }
        public DbSet<GameEvent> GameEvents { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<GameBoard> GameBoards { get; set; }
        public DbSet<GameLog> GameLogs { get; set; }
        public DbSet<GameProcess> GameProcesses { get; set; }
        public DbSet<GameLogSpec> GameLogSpecs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Konfiguracja kluczy głównych ---
            modelBuilder.Entity<User>(e => e.HasKey(p => p.Users_Id));
            modelBuilder.Entity<Board>(e => e.HasKey(p => p.Boards_Id));
            // ... reszta konfiguracji kluczy ...
            modelBuilder.Entity<Deck>(e => e.HasKey(p => p.Decks_Id));
            modelBuilder.Entity<Card>(e => e.HasKey(p => p.Cards_Id));
            modelBuilder.Entity<Decision>(e => e.HasKey(p => p.Decisions_Id));
            modelBuilder.Entity<Hardware>(e => e.HasKey(p => p.Hardwares_Id));
            modelBuilder.Entity<Software>(e => e.HasKey(p => p.Softwares_Id));
            modelBuilder.Entity<Process>(e => e.HasKey(p => p.Processes_Id));
            modelBuilder.Entity<Feedback>(e => e.HasKey(p => p.Feedbacks_Id));
            modelBuilder.Entity<CardWeight>(e => e.HasKey(p => p.Cards_Weights_Id));
            modelBuilder.Entity<CardEnabler>(e => e.HasKey(p => p.Cards_Enablers_Id));
            modelBuilder.Entity<GameEvent>(e => e.HasKey(p => p.Games_Events_Id));
            modelBuilder.Entity<Module>(e => e.HasKey(p => p.Modules_Id));
            modelBuilder.Entity<Game>(e => e.HasKey(p => p.Games_Id));
            modelBuilder.Entity<Team>(e => e.HasKey(p => p.Teams_Id));
            modelBuilder.Entity<GameBoard>(e => e.HasKey(p => p.Games_Boards_Id));
            modelBuilder.Entity<GameLog>(e => e.HasKey(p => p.Games_Logs_Id));
            modelBuilder.Entity<GameProcess>(e => e.HasKey(p => p.Games_Processes_Id));
            modelBuilder.Entity<GameLogSpec>(e => e.HasKey(p => p.Games_Logs_Specs_Id));

            // Zastąp istniejącą konfigurację relacji Game -> Board tą poniżej
            modelBuilder.Entity<Board>()
                .HasMany(b => b.TeamGames)
                .WithOne(g => g.Teams_Boards)
                .HasForeignKey(g => g.Teams_Boards_Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Board>()
                .HasMany(b => b.RivalGames)
                .WithOne(g => g.Rivals_Boards)
                .HasForeignKey(g => g.Rivals_Boards_Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Pozostałe relacje bez zmian
            modelBuilder.Entity<Board>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.Users_Id);

            // ... reszta Twoich relacji ...
            modelBuilder.Entity<Card>()
                .HasOne(c => c.Deck)
                .WithMany()
                .HasForeignKey(c => c.Decks_Id);

            modelBuilder.Entity<Card>()
                .HasOne(c => c.Module)
                .WithMany()
                .HasForeignKey(c => c.Modules_Id);

            modelBuilder.Entity<CardEnabler>()
                .HasOne(ce => ce.Cards)
                .WithMany(c => c.DecisionEnablers)
                .HasForeignKey(ce => ce.Cards_Id);

            modelBuilder.Entity<CardEnabler>()
                .HasOne(ce => ce.Enablers)
                .WithMany(c => c.DecisionEnablerOfThis)
                .HasForeignKey(ce => ce.Enablers_Id);

            modelBuilder.Entity<CardEnabler>()
                .HasOne(ce => ce.Games)
                .WithMany()
                .HasForeignKey(ce => ce.Games_Id);

            modelBuilder.Entity<CardEnabler>()
                .HasOne(ce => ce.Teams)
                .WithMany()
                .HasForeignKey(ce => ce.Teams_Id);

            modelBuilder.Entity<CardWeight>()
                .HasOne(cw => cw.Cards)
                .WithMany()
                .HasForeignKey(cw => cw.Cards_Id);

            modelBuilder.Entity<CardWeight>()
                .HasOne(cw => cw.Processes)
                .WithMany()
                .HasForeignKey(cw => cw.Processes_Id);

            modelBuilder.Entity<Decision>()
                .HasOne(d => d.Card)
                .WithMany()
                .HasForeignKey(d => d.Cards_Id);

            modelBuilder.Entity<Deck>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.Users_Id);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Cards)
                .WithMany()
                .HasForeignKey(f => f.Cards_Id);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Decks)
                .WithMany()
                .HasForeignKey(g => g.Decks_Id);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Modules)
                .WithMany()
                .HasForeignKey(g => g.Modules_Id);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Users)
                .WithMany()
                .HasForeignKey(g => g.Users_Id);

            modelBuilder.Entity<GameBoard>()
                .HasOne(gb => gb.Teams)
                .WithMany()
                .HasForeignKey(gb => gb.Teams_Id);

            modelBuilder.Entity<GameBoard>()
                .HasOne(gb => gb.Games)
                .WithMany(g => g.GameBoards)
                .HasForeignKey(gb => gb.Games_Id);

            modelBuilder.Entity<GameBoard>()
                .HasOne(gb => gb.Games_Processes)
                .WithMany()
                .HasForeignKey(gb => gb.Games_Processes_Id);

            modelBuilder.Entity<GameBoard>()
                .HasOne(gb => gb.Boards)
                .WithMany()
                .HasForeignKey(gb => gb.Boards_Id);

            modelBuilder.Entity<GameEvent>()
                .HasOne(ge => ge.Decks)
                .WithMany()
                .HasForeignKey(ge => ge.Decks_Id);

            modelBuilder.Entity<GameEvent>()
                .HasOne(ge => ge.Modules)
                .WithMany()
                .HasForeignKey(ge => ge.Modules_Id);

            modelBuilder.Entity<GameLog>()
                .HasOne(gl => gl.Teams)
                .WithMany()
                .HasForeignKey(gl => gl.Teams_Id);

            modelBuilder.Entity<GameLog>()
                .HasOne(gl => gl.Games)
                .WithMany(g => g.GameLogs)
                .HasForeignKey(gl => gl.Games_Id);

            modelBuilder.Entity<GameLog>()
                .HasOne(gl => gl.Games_Events)
                .WithMany(ge => ge.GameLogs)
                .HasForeignKey(gl => gl.Games_Events_Id);

            modelBuilder.Entity<GameLog>()
                .HasOne(gl => gl.Cards)
                .WithMany()
                .HasForeignKey(gl => gl.Cards_Id);

            modelBuilder.Entity<GameLog>()
                .HasOne(gl => gl.Boards)
                .WithMany()
                .HasForeignKey(gl => gl.Boards_Id);

            modelBuilder.Entity<GameLog>()
                .HasOne(gl => gl.Feedbacks)
                .WithMany()
                .HasForeignKey(gl => gl.Feedbacks_Id);

            modelBuilder.Entity<GameLogSpec>()
                .HasOne(gls => gls.Games_Logs)
                .WithMany(gl => gl.GameLogSpecs)
                .HasForeignKey(gls => gls.Games_Logs_Id);

            modelBuilder.Entity<GameLogSpec>()
                .HasOne(gls => gls.Games_Processes)
                .WithMany()
                .HasForeignKey(gls => gls.Games_Processes_Id);

            modelBuilder.Entity<GameProcess>()
                .HasOne(gp => gp.Processes)
                .WithMany()
                .HasForeignKey(gp => gp.Processes_Id);

            modelBuilder.Entity<GameProcess>()
                .HasOne(gp => gp.Games)
                .WithMany(g => g.GameProcesses)
                .HasForeignKey(gp => gp.Games_Id);

            modelBuilder.Entity<GameProcess>()
                .HasOne(gp => gp.Teams)
                .WithMany(t => t.Game_Processes)
                .HasForeignKey(gp => gp.Teams_Id);

            modelBuilder.Entity<Hardware>()
                .HasOne(h => h.Cards)
                .WithMany()
                .HasForeignKey(h => h.Cards_Id);

            modelBuilder.Entity<Module>()
                .HasOne(m => m.Deck)
                .WithMany()
                .HasForeignKey(m => m.Decks_Id);

            modelBuilder.Entity<Process>()
                .HasOne(p => p.Decks)
                .WithMany(d => d.Processes)
                .HasForeignKey(p => p.Decks_Id);

            modelBuilder.Entity<Process>()
                .HasOne(p => p.Modules)
                .WithMany()
                .HasForeignKey(p => p.Modules_Id);

            modelBuilder.Entity<Software>()
                .HasOne(s => s.Cards)
                .WithMany()
                .HasForeignKey(s => s.Cards_Id);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Games)
                .WithMany(g => g.Teams)
                .HasForeignKey(t => t.Games_Id);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Games_Events)
                .WithMany(ge => ge.Teams)
                .HasForeignKey(t => t.Games_Events_Id);
        }
    }
}