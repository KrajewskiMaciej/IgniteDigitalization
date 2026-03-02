using backend.Data;
using backend.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IPlayerQueryService
    {
        Task<CategorizedCardsDto> GetCategorizedCardsForDeckAsync(int deckId, int gameId, int teamId);
        Task<object> GetPlayerSessionDataAsync(string teamToken);
        Task<IEnumerable<object>> GetProcessPawnsForBoardAsync(int gameId, int teamId, int boardId);
        Task<IEnumerable<object>> GetRivalPawnsForBoardAsync(int gameId, int boardId);
    }
    public class PlayerQueryService : IPlayerQueryService
    {
        private readonly AppDbContext _context;

        public PlayerQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategorizedCardsDto> GetCategorizedCardsForDeckAsync(int deckId, int gameId, int teamId)
        {
            if (!await _context.Decks.AnyAsync(d => d.Decks_Id == deckId))
                throw new Exception($"Talia o ID {deckId} nie została znaleziona.");

            // Krok 1: Pobierz WEWNĘTRZNE ID kart, które zostały już pomyślnie zagrane przez drużynę.
            var playedCardInternalIds = await _context.GameLogs
                .AsNoTracking()
                .Where(gl => gl.Games_Id == gameId && gl.Teams_Id == teamId && gl.Status == true && gl.Cards_Id.HasValue)
                .Select(gl => gl.Cards_Id!.Value)
                .Distinct()
                .ToListAsync();

            var playedCardInternalIdsSet = new HashSet<int>(playedCardInternalIds);

            //Karty które czekają na zatwierdzenie admina
            var pendingCardInternalIds = await _context.GameLogs
                .AsNoTracking()
                .Where(gl => gl.Games_Id == gameId && gl.Teams_Id == teamId && gl.Cards_Id.HasValue && gl.Is_Approved == false)
                .Select(gl => gl.Cards_Id!.Value)
                .Distinct()
                .ToListAsync();

            var pendingCardInternalIdsSet = new HashSet<int>(pendingCardInternalIds);

            var blockedCardInternalIds = new HashSet<int>(playedCardInternalIdsSet);
            blockedCardInternalIds.UnionWith(pendingCardInternalIdsSet);

            // Oblicz, które fazy należą do "Rynkowa" (Mapa 2)
            var rynkowaPhaseIds = await _context.Phases
                .AsNoTracking()
                .Where(p => p.Decks_Id == deckId && p.Phase_Name == "Rynkowa")
                .Select(p => p.Phases_Id)
                .ToListAsync();

            var teamCurrentPhaseId = await _context.Teams
                .AsNoTracking()
                .Where(t => t.Teams_Id == teamId)
                .Select(t => t.Current_Phase_Id)
                .FirstOrDefaultAsync();

            bool isInRynkowaPhase = teamCurrentPhaseId.HasValue && rynkowaPhaseIds.Contains(teamCurrentPhaseId.Value);

            // Krok 2: Pobierz WEWNĘTRZNE ID kart, które zostały specjalnie odblokowane dla tej drużyny w tej grze.
            var teamSpecificUnlockedCardIds = await _context.CardEnablers
                .AsNoTracking()
                .Where(ce => ce.Games_Id == gameId && ce.Teams_Id == teamId)
                .Select(ce => ce.Cards_Id)
                .Distinct()
                .ToListAsync();

            var teamSpecificUnlockedCardIdsSet = new HashSet<int>(teamSpecificUnlockedCardIds);

            // Krok 3: Stwórz mapę GLOBALNYCH zależności (gdzie Games_Id jest null).
            var enablersMap = await _context.CardEnablers
                .AsNoTracking()
                .Where(ce => ce.Games_Id == null && ce.Enablers_Id.HasValue)
                .Include(ce => ce.Enablers)
                .Where(ce => ce.Enablers != null)
                .Where(ce => !playedCardInternalIdsSet.Contains(ce.Enablers_Id!.Value))
                .GroupBy(ce => ce.Cards_Id)
                .Select(g => new
                {
                    CardId = g.Key,
                    RequiredPublicCardIds = g.Select(ce => ce.Enablers!.Card_Id).ToList()
                })
                .ToDictionaryAsync(x => x.CardId, x => x.RequiredPublicCardIds);

            // Krok 4: Pobierz karty Decyzji
            var decisionCards = await _context.Decisions
                .AsNoTracking()
                .Include(d => d.Card)
                .Where(d => d.Card.Decks_Id == deckId && !blockedCardInternalIds.Contains(d.Cards_Id))
                .Where(d => isInRynkowaPhase || !d.Card.Phases_Id.HasValue || !rynkowaPhaseIds.Contains(d.Card.Phases_Id.Value))
                .OrderBy(d => d.Card.Card_Id)
                .Select(d => new UnifiedCardDto
                {
                    Id = d.Card.Card_Id,
                    DeckId = deckId,
                    Title = d.Decisions_Short_Desc,
                    Description = d.Decisions_Long_Desc,
                    Cost = d.Decisions_Cost_Bits,
                    Enablers = teamSpecificUnlockedCardIdsSet.Contains(d.Cards_Id)
                                ? new List<int>()
                                : (enablersMap.ContainsKey(d.Cards_Id) ? enablersMap[d.Cards_Id] : new List<int>())
                }).ToListAsync();

            // Krok 5: Pobierz karty Sprzętu (Hardware)
            var hardwareCards = await _context.Hardwares
                .AsNoTracking()
                .Include(h => h.Cards)
                .Where(h => h.Cards.Decks_Id == deckId && !blockedCardInternalIds.Contains(h.Cards_Id))
                .Where(h => isInRynkowaPhase || !h.Cards.Phases_Id.HasValue || !rynkowaPhaseIds.Contains(h.Cards.Phases_Id.Value))
                .OrderBy(h => h.Cards.Card_Id) // Dodano sortowanie dla spójności
                .Select(h => new UnifiedCardDto
                {
                    Id = h.Cards.Card_Id,
                    DeckId = deckId,
                    Title = h.Hardwares_Short_Desc,
                    Description = h.Hardwares_Long_Desc,
                    Cost = h.Hardwares_Cost_Bits,
                    Enablers = new List<int>()
                }).ToListAsync();

            // Krok 6: Pobierz karty Oprogramowania (Software)
            var softwareCards = await _context.Softwares
                .AsNoTracking()
                .Include(s => s.Cards)
                .Where(s => s.Cards.Decks_Id == deckId && !blockedCardInternalIds.Contains(s.Cards_Id))
                .Where(s => isInRynkowaPhase || !s.Cards.Phases_Id.HasValue || !rynkowaPhaseIds.Contains(s.Cards.Phases_Id.Value))
                .OrderBy(s => s.Cards.Card_Id) // Dodano sortowanie dla spójności
                .Select(s => new UnifiedCardDto
                {
                    Id = s.Cards.Card_Id,
                    DeckId = deckId,
                    Title = s.Softwares_Short_Desc,
                    Description = s.Softwares_Long_Desc,
                    Cost = s.Softwares_Cost_Bits,
                    Enablers = new List<int>()
                }).ToListAsync();

            // Krok 7: Zwróć obiekt DTO z trzema oddzielnymi listami
            return new CategorizedCardsDto
            {
                DecisionCards = decisionCards,
                SoftwareCards = softwareCards,
                HardwareCards = hardwareCards,

            };
        }
        public async Task<object> GetPlayerSessionDataAsync(string teamToken)
        {
            var team = await _context.Teams
        .AsNoTracking()
        .Include(t => t.Games)
        .FirstOrDefaultAsync(t => t.Teams_Token == teamToken);

            if (team == null) throw new Exception("Nie znaleziono drużyny dla podanego tokena.");
            if (team.Games == null) throw new Exception("Błąd danych: Drużyna nie jest przypisana do gry.");

            // --- KLUCZOWA ZMIANA: Zwracanie obiektów ErrorResponseDto zamiast rzucania wyjątków ---
            switch (team.Games.Game_Status)
            {
                case GameStatus.Paused:
                    return new ErrorResponseDto { ErrorCode = "GamePaused", Message = "Gra jest obecnie wstrzymana." };
                case GameStatus.End:
                    return new ErrorResponseDto { ErrorCode = "GameEnded", Message = "Ta gra została już zakończona." };
            }
            // --- KONIEC ZMIANY ---

            // Jeśli status jest OK, kontynuujemy pobieranie reszty danych
            team = await _context.Teams
                .AsNoTracking()
                .Include(t => t.Games).ThenInclude(g => g.Decks)
                .Include(t => t.Games).ThenInclude(g => g.Teams_Boards)
                .Include(t => t.Games).ThenInclude(g => g.Rivals_Boards)
                .Include(t => t.CurrentPhase)
                .FirstOrDefaultAsync(t => t.Teams_Token == teamToken);

            if (team!.Games.Teams_Boards == null) throw new Exception("Błąd konfiguracji gry: Brak danych planszy.");

            // Reszta kodu pozostaje bez zmian
            return new
            {
                gameId = team.Games.Games_Id,
                gameName = team.Games.Games_Desc,
                teamId = team.Teams_Id,
                teamName = team.Teams_Name,
                teamColor = team.Teams_Color,
                teamBudget = team.Teams_Bud,
                deckId = team.Games.Decks_Id,
                IsOnline = team.Games.Is_Online,
                IsIndependent = team.Is_Independent,
                currentPhaseId = team.Current_Phase_Id,
                currentPhaseName = team.CurrentPhase?.Phase_Name,
                boardConfig = new
                {
                    boardId = team.Games.Teams_Boards.Boards_Id,
                    Name = team.Games.Teams_Boards.Name,
                    LabelsUp = team.Games.Teams_Boards.Labels_Up?.Split(';'),
                    LabelsRight = team.Games.Teams_Boards.Labels_Right?.Split(';'),
                    Borders_Colors = team.Games.Teams_Boards.Borders_Colors?.Split(';'),
                    team.Games.Teams_Boards.Description_Down,
                    team.Games.Teams_Boards.Description_Left,
                    team.Games.Teams_Boards.Rows,
                    team.Games.Teams_Boards.Cols,
                    team.Games.Teams_Boards.Cell_Color,
                    team.Games.Teams_Boards.Border_Color,
                    Cells_Descriptions = team.Games.Teams_Boards.Cells_Descriptions ?? string.Empty
                },
                rivalBoardConfig = team.Games.Rivals_Boards != null ? new
                {
                    boardId = team.Games.Rivals_Boards.Boards_Id,
                    Name = team.Games.Rivals_Boards.Name,
                    LabelsUp = team.Games.Rivals_Boards.Labels_Up?.Split(';'),
                    LabelsRight = team.Games.Rivals_Boards.Labels_Right?.Split(';'),
                    Borders_Colors = team.Games.Rivals_Boards.Borders_Colors?.Split(';'),
                    team.Games.Rivals_Boards.Description_Down,
                    team.Games.Rivals_Boards.Description_Left,
                    team.Games.Rivals_Boards.Rows,
                    team.Games.Rivals_Boards.Cols,
                    team.Games.Rivals_Boards.Cell_Color,
                    team.Games.Rivals_Boards.Border_Color,
                    Cells_Descriptions = team.Games.Rivals_Boards.Cells_Descriptions ?? string.Empty
                } : null
            };
        }

        public async Task<IEnumerable<object>> GetProcessPawnsForBoardAsync(int gameId, int teamId, int boardId)
        {
            // Krok 1: pobierz pionki z podstawowymi danymi i identyfikatorami potrzebnymi do obliczenia MaxPos
            var pawns = await _context.GameBoards
                .AsNoTracking()
                .Where(gb =>
                    gb.Games_Id == gameId &&
                    gb.Teams_Id == teamId &&
                    gb.Boards_Id == boardId &&
                    gb.Games_Processes_Id != null &&
                    gb.Games_Processes != null &&
                    gb.Games_Processes.Processes != null)
                .Select(gb => new
                {
                    GPId      = gb.Games_Processes_Id,
                    PosX      = gb.Poz_X,
                    PosY      = gb.Poz_Y,
                    Color     = gb.Games_Processes!.Processes!.Processes_Color,
                    Name      = gb.Games_Processes!.Processes!.Processes_Desc,
                    ProcessId = gb.Games_Processes!.Processes!.Processes_Id,
                    DecksId   = gb.Games_Processes!.Processes!.Decks_Id,
                })
                .ToListAsync();

            if (!pawns.Any()) return Enumerable.Empty<object>();

            // Krok 2: pobierz wszystkie CardWeights dla procesów jednym zapytaniem
            var processIds = pawns.Select(p => p.ProcessId).Distinct().ToList();
            var allWeights = await _context.CardWeights
                .AsNoTracking()
                .Where(cw => processIds.Contains(cw.Processes_Id))
                .Select(cw => new
                {
                    cw.Processes_Id,
                    cw.Weights_X,
                    cw.Weights_Y,
                    PhasesId = cw.Cards.Phases_Id
                })
                .ToListAsync();

            // Krok 3: pobierz PrepMultiplier_Max dla decki jednym zapytaniem
            var deckIds = pawns.Select(p => p.DecksId).Distinct().ToList();
            var multipliers = await _context.Decks
                .AsNoTracking()
                .Where(d => deckIds.Contains(d.Decks_Id) && d.EconomySettings != null)
                .Select(d => new { d.Decks_Id, d.EconomySettings!.PrepMultiplier_Max })
                .ToDictionaryAsync(d => d.Decks_Id, d => d.PrepMultiplier_Max);

            // Krok 4: oblicz MaxPosX/MaxPosY per proces
            // Faza 1 = Phases_Id != 3 (w tym null i 2 – karta włączająca rynek, dostępna od początku)
            // Faza 2 = Phases_Id == 3 ("Rynkowa")
            // MaxPos = Σ(wagi Fazy1) + Σ(wagi Fazy2) × PrepMultiplier_Max
            var result = pawns.Select(pawn =>
            {
                double multiplier = multipliers.GetValueOrDefault(pawn.DecksId, 2.0);
                if (multiplier <= 0) multiplier = 2.0;

                var weights = allWeights.Where(cw => cw.Processes_Id == pawn.ProcessId).ToList();

                double maxPosX = weights.Where(cw => cw.PhasesId != 3).Sum(cw => cw.Weights_X)
                               + weights.Where(cw => cw.PhasesId == 3).Sum(cw => cw.Weights_X) * multiplier;

                double maxPosY = weights.Where(cw => cw.PhasesId != 3).Sum(cw => cw.Weights_Y)
                               + weights.Where(cw => cw.PhasesId == 3).Sum(cw => cw.Weights_Y) * multiplier;

                return (object)new
                {
                    GPId    = pawn.GPId,
                    PosX    = pawn.PosX,
                    PosY    = pawn.PosY,
                    Color   = pawn.Color,
                    Name    = pawn.Name,
                    MaxPosX = maxPosX,
                    MaxPosY = maxPosY,
                };
            }).ToList();

            return result;
        }

        public async Task<IEnumerable<object>> GetRivalPawnsForBoardAsync(int gameId, int boardId)
        {
            // Krok 1: pobierz pionki drużyn (Games_Processes_Id == null) z danej planszy
            var teamPawns = await _context.GameBoards
                .AsNoTracking()
                .Where(gb => gb.Games_Id == gameId && gb.Boards_Id == boardId && gb.Games_Processes_Id == null)
                .Select(gb => new
                {
                    PosX      = gb.Poz_X,
                    PosY      = gb.Poz_Y,
                    TeamColor = gb.Teams.Teams_Color,
                    TeamId    = gb.Teams.Teams_Id,
                    TeamName  = gb.Teams.Teams_Name
                })
                .ToListAsync();

            if (!teamPawns.Any()) return Enumerable.Empty<object>();

            // Krok 2: pobierz wpisy procesów dla tych drużyn w tej grze (potrzebne do obliczenia MaxPos)
            var teamIds = teamPawns.Select(t => t.TeamId).Distinct().ToList();
            var processEntries = await _context.GameBoards
                .AsNoTracking()
                .Where(gb =>
                    gb.Games_Id == gameId &&
                    teamIds.Contains(gb.Teams_Id) &&
                    gb.Games_Processes_Id != null &&
                    gb.Games_Processes != null &&
                    gb.Games_Processes.Processes != null)
                .Select(gb => new
                {
                    gb.Teams_Id,
                    ProcessId = gb.Games_Processes!.Processes!.Processes_Id,
                    DecksId   = gb.Games_Processes!.Processes!.Decks_Id,
                })
                .ToListAsync();

            // Krok 3: pobierz CardWeights dla wszystkich procesów jednym zapytaniem
            var processIds = processEntries.Select(p => p.ProcessId).Distinct().ToList();
            var allWeights = await _context.CardWeights
                .AsNoTracking()
                .Where(cw => processIds.Contains(cw.Processes_Id))
                .Select(cw => new
                {
                    cw.Processes_Id,
                    cw.Weights_X,
                    cw.Weights_Y,
                    PhasesId = cw.Cards.Phases_Id
                })
                .ToListAsync();

            // Krok 4: pobierz PrepMultiplier_Max dla decki jednym zapytaniem
            var deckIds = processEntries.Select(p => p.DecksId).Distinct().ToList();
            var multipliers = await _context.Decks
                .AsNoTracking()
                .Where(d => deckIds.Contains(d.Decks_Id) && d.EconomySettings != null)
                .Select(d => new { d.Decks_Id, d.EconomySettings!.PrepMultiplier_Max })
                .ToDictionaryAsync(d => d.Decks_Id, d => d.PrepMultiplier_Max);

            // Krok 5: oblicz MaxPosX/MaxPosY per drużyna = Average(MaxPosX_i) dla procesów drużyny
            // Formuła per proces (taka sama jak gameBoardCartesian): Σ(wagi Fazy1) + Σ(wagi Fazy2) × PrepMultiplier_Max
            // MaxPos drużyny = (MaxPos_proc1 + MaxPos_proc2 + ... + MaxPos_procN) / N
            // Frontend oblicza: pct = PosX_team (avg z DB) / MaxPosX_avg
            var maxPosByTeam = new Dictionary<int, (double MaxX, double MaxY)>();
            foreach (var teamId in teamIds)
            {
                var teamProcesses = processEntries.Where(p => p.Teams_Id == teamId).ToList();
                if (!teamProcesses.Any())
                {
                    maxPosByTeam[teamId] = (1.0, 1.0);
                    continue;
                }

                var processMaxes = teamProcesses.Select(proc =>
                {
                    double multiplier = multipliers.GetValueOrDefault(proc.DecksId, 2.0);
                    if (multiplier <= 0) multiplier = 2.0;

                    var weights = allWeights.Where(cw => cw.Processes_Id == proc.ProcessId).ToList();
                    double maxX = weights.Where(cw => cw.PhasesId != 3).Sum(cw => cw.Weights_X)
                                + weights.Where(cw => cw.PhasesId == 3).Sum(cw => cw.Weights_X) * multiplier;
                    double maxY = weights.Where(cw => cw.PhasesId != 3).Sum(cw => cw.Weights_Y)
                                + weights.Where(cw => cw.PhasesId == 3).Sum(cw => cw.Weights_Y) * multiplier;
                    return (MaxX: maxX, MaxY: maxY);
                }).ToList();

                double avgMaxX = processMaxes.Average(m => m.MaxX);
                double avgMaxY = processMaxes.Average(m => m.MaxY);
                maxPosByTeam[teamId] = (avgMaxX > 0 ? avgMaxX : 1.0, avgMaxY > 0 ? avgMaxY : 1.0);
            }

            // Krok 6: złącz dane i zwróć wynik
            // PosX/PosY = surowa pozycja drużyny z DB (= średnia arytmetyczna procesów zapisana przez SetTeamPosAsync)
            // MaxPosX/MaxPosY = średnia arytmetyczna maksimów procesów (taki sam wzór jak gameBoardCartesian per-proces)
            var result = teamPawns.Select(tp =>
            {
                var (maxX, maxY) = maxPosByTeam.GetValueOrDefault(tp.TeamId, (1.0, 1.0));
                return (object)new
                {
                    PosX      = tp.PosX,
                    PosY      = tp.PosY,
                    TeamColor = tp.TeamColor,
                    TeamId    = tp.TeamId,
                    TeamName  = tp.TeamName,
                    MaxPosX   = maxX,
                    MaxPosY   = maxY,
                };
            }).ToList();

            return result;
        }
    }
}