export interface Deck {
  id: number
  title: string
}

export interface Card {
  id: number
  deckId: number
  title: string
  description: string
  cost?: number
  enablers?: unknown[]
  type?: 'decision' | 'hardware' | 'software'
  displayOrder?: number
}

export interface Feedback {
  id: number
  longDescription: string
  status: 'P' | 'N'
}

export interface ISignalRService {
  connection: any
  start: () => Promise<void>
  joinGameRoom: (gameId: string) => Promise<void> | undefined
  leaveGameRoom: (gameId: string) => Promise<void> | undefined
}

export interface ProgressDataPoint {
  round: number
  [key: string]: number
}

export interface Board {
  boardId: number
  name: string
  boards_Id?: number
  labels_Up?: string
  labels_Right?: string
  description_Down?: string
  description_Left?: string
  rows?: number
  cols?: number
  cell_Color?: string
  border_Color?: string
  borders_Colors?: string
}

export interface AvgBitsDataPoint {
  team: string
  avgBits: number
}

export interface Team {
  teamId: number
  teamName: string
  teamBud: number
  deckId?: number
  boardId?: number
  id?: number
  name?: string
  colour?: string
  isAbleToMakeDecisions?: boolean
  color?: string
  token?: string
}

export interface Item {
  id: number
  title: string
  description: string
  cost?: number
  deckId?: number
  shortDesc?: string
  longDesc?: string
  type?: 'Hardware' | 'Software'
}

export interface DecisionLog {
  isEventNotification: boolean
  timestamp: string
  feedbackDescription: string
  cardId?: number
  cardTitle?: string
  tableId?: number | null
  tableName?: string
  result?: 'Pozytywny' | 'Negatywny'
  eventAppliedId?: number | null
}

export interface PendingDecision {
  logId: number
  cardId: number
  cardTitle: string
  tableId: number
  tableName: string
  timestamp: string
}

export interface GameEvent {
  eventId: number | null
  shortDesc: string
  longDesc: string
}

export interface Pawn {
  id: number
  x: number
  y: number
  color: string
  name: string
  maxX: number
  maxY: number
}

export interface RawApiLog {
  logId: number
  gameEventId: number | null
  teamId: number | null
  timestamp: string
  cardId: number
  cardTitle: string
  teamName: string
  feedbackDescription: string
  enablerDescription?: string
  status: boolean
  isEventNotification?: boolean
  eventDescription?: string
}

export interface RawPendingLog {
  logId: number
  cardId: number
  cardTitle: string
  teamId: number
  teamName: string
  timestamp: string
}

export interface RawPawnData {
  teamId: number
  posX: string | number
  posY: string | number
  teamColor: string
  teamName: string
  gpId?: number
  color?: string
  name?: string
  maxPosX?: number
  maxPosY?: number
}

export interface RivalBoardConfigFromApi {
  boardId: number
  name: string
  labelsUp: string[]
  labelsRight: string[]
  descriptionDown: string
  descriptionLeft: string
  rows: number
  cols: number
  cellColor: string
  borderColor: string
  borderColors: string[]
  cellsDescriptions?: string
}

export interface BoardConfigForComponent {
  name: string
  labelsUp: string[]
  labelsRight: string[]
  descriptionDown: string
  descriptionLeft: string
  rows: number
  cols: number
  cellColor: string
  borderColor: string
  borderColors: string[]
  boardId: number
  cellsDescriptions?: string
}

export interface GameDetails {
  deckId: number
}

export interface ApiError {
  response?: {
    data?: {
      message?: string
      title?: string
    }
  }
  message?: string
  toString: () => string
}

export interface AuthState {
  isAuthenticated: boolean
}

export interface BitsDataPoint {
  round: number
  bits: number
}

export interface CardsApiResponse {
  decisionCards: DecisionCard[]
  itemCards: any[]
  hardwareCards?: Card[] // Zakładamy, że API rozdziela karty przedmiotów
  softwareCards?: Card[]
}

export interface DecisionCard {
  id: number
  title: string
  description: string
}

export interface PositionDataPoint {
  name: string
  position: number
}

export interface PdfInfo {
  name: string
  path: string
}

export interface PdfWindow {
  path: string
}

export interface DecisionData {
  [key: string]: number
}

export interface AuthView {
  view: 'login' | 'register' | 'forgotPassword' | 'confirmEmail'
}

export interface User {
  name: string
  email: string
}

export interface ApiService {
  get: <T>(endpoint: string, params?: any) => Promise<any>
  post: <T>(endpoint: string, data: any, config?: any) => Promise<any>
  put: <T>(endpoint: string, data: any) => Promise<any>
  delete: <T>(endpoint: string) => Promise<any>
  getFile: (endpoint: string, params?: any) => Promise<any>
  postForFile: (endpoint: string, data?: any) => Promise<any>
}
export interface BoardConfig {
  boardId: number
  name: string

  // Etykiety
  labelsUp: string[]
  labelsRight: string[]

  // Kolory
  cellColor: string
  borderColor: string
  borderColors: string[]

  // Opisy
  descriptionDown: string
  descriptionLeft: string

  // Wymiary
  rows: number
  cols: number

  // Opisy Ćwiartek
  cellsDescriptions?: string
}

export interface GameData {
  teamName: string
  teamColor: string
  teamBudget: number
  deckId: number
  teamId: number
  gameId: number
  isOnline: boolean
  isIndependent: boolean
  boardConfig: BoardConfig
  rivalBoardConfig?: any
  currentPhaseName?: string | null
  currentStage?: 1 | 2
}

export interface GameStatusError {
  title: string
  message: string
}

export interface Game {
  id: number
  name: string
  status: string
}

export interface Table {
  id: number
  color: string
  token: string
  name?: string
}

export interface Gamelog {
  TeamId: number
  GameId: number
  CardId: number
  DeckId: number | null
  Date: string
  FeedbackId: number | null
  Cost: number | null
  Status: string
}

export interface GamelogFormState {
  TeamId: number | ''
  GameId: number | ''
  CardId: number | ''
  DeckId: number | ''
  Date: string
  FeedbackId: number | ''
  Cost: number | ''
  Status: string
}

export interface DeleteFormState {
  TeamId: number | ''
  GameId: number | ''
  CardId: number | ''
}

export interface TeamSuccessData {
  team: string
  success: number
  failure: number
}

export interface ChartDataPoint {
  game: number
  [key: string]: number
}
export interface SessionData {
  teamId: number
  teamName: string
  teamBud: number
  deckId: number
  boardConfig: BoardConfig & { boardId: number }
}

export interface TeamData {
  teamId: number
  teamName: string
  teamBud: number
  deckId: number
  boardId: number
}

export interface RawHistoryLog {
  isEventNotification: boolean
  eventDescription: string
  timestamp: string
  cardId: number
  cardTitle: string
  teamId: number
  teamName: string
  feedbackDescription: string
  enablerDescription?: string
  status: boolean
  gameEventId: number | null
}

export interface RawPawn {
  gpId: number
  posX: string
  posY: string
  color: string
  name: string
}
export interface GameCreationResponse {
  message?: string
}
export interface DetectedBarcode {
  rawValue: string
}
export interface ApiLogEntry {
  isEventNotification: boolean
  eventDescription?: string
  cardTitle?: string
  cardId: number
  status: boolean
  feedbackDescription?: string
  enablerDescription?: string
  cost: number
  gameEventId: number | null
}
export interface ProcessedLogEntry {
  isEventNotification: boolean
  description: string
  choice: string
  cardId: number
  result: 'Pozytywny' | 'Negatywny'
  eventApplied: boolean
}
