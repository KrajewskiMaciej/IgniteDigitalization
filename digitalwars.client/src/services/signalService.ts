// Upewnij się, że masz zainstalowany pakiet: npm install @microsoft/signalr
import * as signalR from '@microsoft/signalr'

// Pobierz URL huba z zmiennych środowiskowych lub użyj domyślnego.
const HUB_URL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL.replace(/^http/, 'ws')}/gameHub`
  : '/gameHub'

const connection = new signalR.HubConnectionBuilder()
  .withUrl(HUB_URL)
  .withAutomaticReconnect()
  .build()

// Przechowuje obietnicę startu, aby uniknąć wielokrotnego wywoływania .start()
let startPromise: Promise<void> | null = null

// Definicja interfejsu dla serwisu dla lepszego typowania
interface ISignalRService {
  connection: signalR.HubConnection
  start: () => Promise<void>
  joinGameRoom: (gameId: string) => Promise<void> | undefined
  leaveGameRoom: (gameId: string) => Promise<void> | undefined
}

const signalRService: ISignalRService = {
  connection,

  start() {
    // Jeśli połączenie nie zostało jeszcze zainicjowane, stwórz nową obietnicę startu.
    // To zapobiega wielokrotnym próbom połączenia, gdy wiele komponentów próbuje to zrobić jednocześnie.
    if (!startPromise) {
      console.log('SignalR: Inicjowanie nowego połączenia...')
      startPromise = connection.start().catch((err: any) => {
        console.error('SignalR: Błąd podczas startu, resetowanie obietnicy.', err)
        startPromise = null // Zresetuj w razie błędu, aby umożliwić ponowną próbę
        throw err // Rzuć błąd dalej, aby kod wywołujący mógł na niego zareagować
      })
    }

    return startPromise
  },

  // POPRAWKA: Typ parametru 'gameId' został zmieniony na 'string', aby pasował do backendu (GameHub.cs)
  // i sposobu wywołania z playerView.vue.
  joinGameRoom: (gameId: string) => {
    if (connection.state === signalR.HubConnectionState.Connected) {
      // Nie ma potrzeby konwertować na string, ponieważ już nim jest.
      return connection.invoke('JoinGameRoom', gameId)
    }
    console.warn('SignalR: Próba dołączenia do pokoju bez aktywnego połączenia.')
    return undefined
  },

  // POPRAWKA: Typ parametru 'gameId' również zmieniony na 'string'.
  leaveGameRoom: (gameId: string) => {
    if (connection.state === signalR.HubConnectionState.Connected) {
      return connection.invoke('LeaveGameRoom', gameId)
    }
    console.warn('SignalR: Próba opuszczenia pokoju bez aktywnego połączenia.')
    return undefined
  },
}

export default signalRService
