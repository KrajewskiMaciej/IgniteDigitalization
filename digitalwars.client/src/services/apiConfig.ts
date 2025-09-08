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
    // These are likely in an AdminPanelController, assumed to be correct.
    deck: {
      getAll: '/admin/deck/get',
      upload: '/admin/deck/upload',
      cards: (deckId: number) => `/admin/deck/decisions?deckId=${deckId}`,
      items: (deckId: number) => `/admin/deck/items?deckId=${deckId}`,
      updateItem: (cardId: number) => `/admin/deck/items/${cardId}`,
    },
    settings: {
      licenses: '/admin/licenses'
    },
    games: {
      getGames: (gameId: number) => `/AdminPanel/games/${gameId}`,
      getTeams: (gameId: number) => `/AdminPanel/teams/by-game/${gameId}`
    },
    export: {
      cards: (deckId: number) => `/admin/exportCards?deckId=${deckId}`,
      boards: `/admin/exportBoards`
    }
  },
  games: {
    // These are likely in a GamesController, assumed to be correct.
    create: '/games/create',
    getAll: '/games/active',
    updateStatus: (id: number) => `/games/${id}/status`,
    getById: (id: number) => `/games/${id}`,
    stopAll: '/games/stop-all',
    endAll: '/games/end-all',
  },
  boards: {
    // These are likely in a BoardController, assumed to be correct.
    create: '/board/add',
    getAll: '/board/get',
    delete: (id: number) => `/board/delete/${id}`,
    update: (id: number) => `/board/edit/${id}`,
  },
  player: {
    // --- CORRECTED & VERIFIED ENDPOINTS from PlayerController ---
    getTeamInfo: (gameId: number, teamId: number) => `/player/game/${gameId}/team/${teamId}/info`,
    getTeamsManagement: (gameId: number) => `/player/game/${gameId}/teams-management`,
    updateTeamBudget: (teamId: number) => `/player/team/${teamId}/budget`,
    unlockCard: (gameId: number) => `/player/game/${gameId}/unlock-card`,
    getPendingLogs: (gameId: number) => `/player/game/${gameId}/pending-logs`,
    getGameEvents: '/player/game-events',
    applyEvent: (gameId: number) => `/player/game/${gameId}/apply-event`,
    getHistoryVersion: (gameId: number, teamId?: number) => teamId ? `/player/game/${gameId}/history-version?teamId=${teamId}` : `/player/game/${gameId}/history-version`,
    getPendingVersion: (gameId: number) => `/player/game/${gameId}/pending-version`,
    getLogs: '/player/getLogs', // Used for team-specific history. Expects gameId and teamId in query params.
    getPlayerHistory: '/player/player-history', // Used for full game history (admin view). Expects gameId in POST body.
    getCards: (deckId: number, gameId: number, teamId: number) => `/player/deck/${deckId}/unified-cards?gameId=${gameId}&teamId=${teamId}`,
    getPlayerSessionData: (teamToken: string) => `/player/team/${teamToken}`,
    getPawns: (gameId: number, teamId: number, boardId: number) => `/player/team-board?gameId=${gameId}&teamId=${teamId}&boardId=${boardId}`,
    getRivalPawns: (gameId: number, boardId: number) => `/player/rival-board?gameId=${gameId}&boardId=${boardId}`,
    getGameData: (gameId: number) => `/player/game/${gameId}/rival-board-config`,
    playCardSuccess: (cardId: number) => `/player/success/${cardId}`,
    playCardFailure: (cardId: number) => `/player/failure/${cardId}`,
    approveLog: (logId: number) => `/player/approve-log/${logId}`,
    rejectLog: (logId: number) => `/player/reject-log/${logId}`,

    // --- CONFIRMED MISSING from PlayerController.cs ---
    // These endpoints were in your original file but do not exist in the provided C# controller.
    getCurrency: '/player/getCurrency', // NOT IMPLEMENTED
    getDecisionCards: (gameId: number) => `/player/game/${gameId}/decision-cards`, // NOT IMPLEMENTED
    getItemCards: (gameId: number) => `/player/game/${gameId}/item-cards`, // NOT IMPLEMENTED
  },
  processes: {
    // This is likely in a ProcessesController, assumed to be correct.
    getByDeck: (deckId: number) => `/processes/by-deck/${deckId}`
  }
};

export default {
  baseURL: API_BASE_URL,
  ...endpoints
};