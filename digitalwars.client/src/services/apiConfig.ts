const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5023/api';

const endpoints = {
  auth: {
    login: '/auth/login',
    register: '/auth/register',
    logout: '/auth/logout',
    me: '/auth/me',
    // POPRAWKA: Dodano typ `string`
    confirmEmail: (token: string) => `/auth/confirm/${token}`,
    forgotPassword: '/password/reset-password',
    resetPassword: '/password/reset',
    // POPRAWKA: Dodano typ `string`
    validateResetToken: (token: string) => `/password/validate-token/${token}`,
  },
  admin: {
    deck: {
        getAll: '/admin/deck/get',
        upload: '/admin/deck/upload',
        decisions: '/admin/deck/decisions'
    },
    settings: {
        licenses: '/admin/licenses'
    },
    games: {
      // POPRAWKA: Dodano typ `number`
      getGames: (gameId: number) => `adminpanel/games/${gameId}`,
      // POPRAWKA: Dodano typ `number`
      getTeams: (gameId: number) => `adminpanel/teams/by-game/${gameId}`
    },
    export: {
      cards: `admin/exportCards`,
      boards: `admin/exportBoards`
    }
  },
  games: {
    create: '/games/create',
    // POPRAWKA: Dodano typ `number`
    getById: (id: number) => `/games/${id}`,
    getAll: '/games/active',
    // POPRAWKA: Dodano typ `number`
    updateStatus: (id: number) => `/games/${id}/status`,
    stopAll: '/games/stop-all',
    endAll: '/games/end-all',
    // POPRAWKA: Dodano typ `number`
    getTeamsManagement: (gameId: number) => `/player/game/${gameId}/teams-management`,
    // POPRAWKA: Dodano typ `number`
    updateTeamBudget: (teamId: number) => `/player/team/${teamId}/budget`,
    // POPRAWKA: Dodano typ `number`
    getDecisionCards: (gameId: number) => `/player/game/${gameId}/decision-cards`,
    // POPRAWKA: Dodano typ `number`
    getItemCards: (gameId: number) => `/player/game/${gameId}/item-cards`,
    // POPRAWKA: Dodano typ `number`
    unlockCard: (gameId: number) => `/player/game/${gameId}/unlock-card`,
    // POPRAWKA: Dodano typ `number`
    getPendingLogs: (gameId: number) => `/player/game/${gameId}/pending-logs`,
    // POPRAWKA: Dodano typ `number`
    approveLog: (logId: number) => `/player/approve-log/${logId}`,
    // POPRAWKA: Dodano typ `number`
    rejectLog: (logId: number) => `/player/reject-log/${logId}`,
    getGameEvents: '/player/game-events',
    // POPRAWKA: Dodano typ `number`
    applyEvent: (gameId: number) => `/player/game/${gameId}/apply-event`,
    // POPRAWKA: Dodano typ `number`
    getHistoryVersion: (gameId: number) => `/player/game/${gameId}/history-version`,
    // POPRAWKA: Dodano typ `number`
    getPendingVersion: (gameId: number) => `/player/game/${gameId}/pending-version`,
    getGameData: `/player/gameRivalBoard`
  },
  boards: {
    create: '/board/add',
    getAll: '/board/get',
    // POPRAWKA: Dodano typ `number`
    delete: (id: number) => `/board/delete/${id}`,
    // POPRAWKA: Dodano typ `number`
    update: (id: number) => `/board/edit/${id}`,
  },
  player: {
    // POPRAWKA: Dodano typ `string`
    getTeamInfo: (teamToken: string) => `/player/team/${teamToken}`,
    // POPRAWKA: Dodano typ `number`
    playCardSuccess: (cardId: number) => `/player/success/${cardId}`,
    // POPRAWKA: Dodano typ `number`
    playCardFailure: (cardId: number) => `/player/failure/${cardId}`,
    // POPRAWKA: Dodano typ `number`
    getCards: (deckId: number) =>`player/deck/${deckId}/unified-cards`,
    getLogs: '/player/getLogs',
    getCurrency: '/player/getCurrency',
    getPawns: '/player/team-board',
    getRivalPawns: `/player/rival-board`,
    getPlayerHistory: '/player/player-history',
    // POPRAWKA: Dodano typy `number`
    getPlayerHistoryVersion: (gameId: number, teamId: number) => `/player/game/${gameId}/history-version?teamId=${teamId}`
  },
  processes: {
    // POPRAWKA: Dodano typ `number`
    getByDeck: (deckId: number) => `/processes/by-deck/${deckId}`
  }
};

export default {
  baseURL: API_BASE_URL,
  ...endpoints
};