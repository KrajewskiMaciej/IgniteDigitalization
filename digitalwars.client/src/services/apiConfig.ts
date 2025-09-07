const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5023/api';

const endpoints = {
  auth: {
    login: '/auth/login',
    register: '/auth/register',
    logout: '/auth/logout',
    me: '/auth/me',
    confirmEmail: (token: string) => `/auth/confirm/${token}`,
    forgotPassword: '/password/reset-password',
    resetPassword: '/password/reset',
    validateResetToken: (token: string) => `/password/validate-token/${token}`,
  },
  admin: {
    deck: {
      getAll: '/admin/deck/get',
      upload: '/admin/deck/upload',
      decisions: (deckId: number) => `/admin/deck/decisions?deckId=${deckId}` // Poprawka: dodanie parametru
    },
    settings: {
      licenses: '/admin/licenses'
    },
    games: {
      // Poprawka: Prawidłowa ścieżka do AdminPanelController
      getGames: (gameId: number) => `/AdminPanel/games/${gameId}`,
      // Poprawka: Prawidłowa ścieżka do AdminPanelController
      getTeams: (gameId: number) => `/AdminPanel/teams/by-game/${gameId}`
    },
    export: {
      // Poprawka: Dodanie parametru deckId
      cards: (deckId: number) => `/admin/exportCards?deckId=${deckId}`,
      boards: `/admin/exportBoards`
    }
  },
  games: {
    create: '/games/create',
    getAll: '/games/active',
    updateStatus: (id: number) => `/games/${id}/status`,
    // Poprawka: Prawidłowa ścieżka do PlayerController
    approveLog: (logId: number) => `/player/approve-log/${logId}`,
    // Poprawka: Prawidłowa ścieżka do PlayerController
    rejectLog: (logId: number) => `/player/reject-log/${logId}`,
    
    // UWAGA: Poniższe endpointy nie zostały znalezione w kodzie backendu.
    // Zostawiam je zgodnie z prośbą, ale mogą wymagać implementacji w C#.
    getById: (id: number) => `/games/${id}`, // Brak implementacji
    stopAll: '/games/stop-all', // Brak implementacji
    endAll: '/games/end-all', // Brak implementacji
    getTeamsManagement: (gameId: number) => `/player/game/${gameId}/teams-management`, // Brak implementacji
    updateTeamBudget: (teamId: number) => `/player/team/${teamId}/budget`, // Brak implementacji
    getDecisionCards: (gameId: number) => `/player/game/${gameId}/decision-cards`, // Brak implementacji
    getItemCards: (gameId: number) => `/player/game/${gameId}/item-cards`, // Brak implementacji
    unlockCard: (gameId: number) => `/player/game/${gameId}/unlock-card`, // Brak implementacji
    getPendingLogs: (gameId: number) => `/player/game/${gameId}/pending-logs`, // Brak implementacji
    getGameEvents: '/player/game-events', // Brak implementacji
    applyEvent: (gameId: number) => `/player/game/${gameId}/apply-event`, // Brak implementacji
    getHistoryVersion: (gameId: number) => `/player/game/${gameId}/history-version`, // Brak implementacji
    getPendingVersion: (gameId: number) => `/player/game/${gameId}/pending-version`, // Brak implementacji
    getGameData: `/player/gameRivalBoard` // Brak implementacji
  },
  boards: {
    create: '/board/add',
    getAll: '/board/get',
    delete: (id: number) => `/board/delete/${id}`,
    update: (id: number) => `/board/edit/${id}`,
  },
  player: {
    // Poprawka: Prawidłowa ścieżka do PlayerController dla danych sesji gracza
    getPlayerSessionData: (teamToken: string) => `/player/team/${teamToken}`,
    playCardSuccess: (cardId: number) => `/player/success/${cardId}`,
    playCardFailure: (cardId: number) => `/player/failure/${cardId}`,
    // Poprawka: Prawidłowa sygnatura z parametrami query
    getCards: (deckId: number, gameId: number, teamId: number) => `/player/deck/${deckId}/unified-cards?gameId=${gameId}&teamId=${teamId}`,
    // Poprawka: Prawidłowa sygnatura z parametrami query
    getPawns: (gameId: number, teamId: number, boardId: number) => `/player/team-board?gameId=${gameId}&teamId=${teamId}&boardId=${boardId}`,
    // Poprawka: Prawidłowa sygnatura z parametrami query
    getRivalPawns: (gameId: number, boardId: number) => `/player/rival-board?gameId=${gameId}&boardId=${boardId}`,

    // UWAGA: Poniższe endpointy nie zostały znalezione w kodzie backendu.
    getTeamInfo: (gameId: number, teamId: number) => `/player/game/${gameId}/team/${teamId}/info`, // Brak implementacji
    getLogs: '/player/getLogs', // Brak implementacji
    getCurrency: '/player/getCurrency', // Brak implementacji
    getPlayerHistory: '/player/player-history', // Brak implementacji
    getPlayerHistoryVersion: (gameId: number, teamId: number) => `/player/game/${gameId}/history-version?teamId=${teamId}` // Brak implementacji
  },
  processes: {
    getByDeck: (deckId: number) => `/processes/by-deck/${deckId}`
  }
};

export default {
  baseURL: API_BASE_URL,
  ...endpoints
};