// BŁĄD TS2307: Ten błąd oznacza, że TypeScript nie może znaleźć typów dla biblioteki SignalR.
// Aby to naprawić, upewnij się, że masz zainstalowany pakiet. Uruchom w terminalu:
// npm install @microsoft/signalr
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5023/gameHub") // Upewnij się, że URL jest poprawny dla Twojego backendu
    .withAutomaticReconnect()
    .build();

// POPRAWKA: Jawnie typujemy `startPromise`. Może to być obietnica lub null.
let startPromise: Promise<void> | null = null;

// Dla lepszej organizacji i typowania, możemy zdefiniować interfejs dla naszego serwisu
interface ISignalRService {
    connection: signalR.HubConnection;
    start: () => Promise<void>;
    joinGameRoom: (gameId: number) => Promise<void> | undefined;
    leaveGameRoom: (gameId: number) => Promise<void> | undefined;
}

const signalRService: ISignalRService = {
    connection,
    
    start() {
        if (!startPromise) {
            console.log("SignalR: Inicjowanie nowego połączenia...");
            startPromise = connection.start().catch((err: any) => { // POPRAWKA: Typujemy parametr `err`
                console.error("SignalR: Błąd podczas startu, resetowanie obietnicy.", err);
                startPromise = null; // Zresetuj w razie błędu, aby umożliwić ponowną próbę
                throw err; // Rzuć błąd dalej, aby kod wywołujący mógł na niego zareagować
            });
        }
        
        return startPromise;
    },
    
    joinGameRoom: (gameId: number) => { // POPRAWKA: Typujemy parametr `gameId`
        if (connection.state === signalR.HubConnectionState.Connected) {
            return connection.invoke("JoinGameRoom", gameId.toString());
        }
        // Opcjonalnie: można zwrócić odrzuconą obietnicę lub zalogować błąd, jeśli połączenie nie jest aktywne
        console.warn("SignalR: Próba dołączenia do pokoju bez aktywnego połączenia.");
        return undefined;
    },
    
    leaveGameRoom: (gameId: number) => { // POPRAWKA: Typujemy parametr `gameId`
        if (connection.state === signalR.HubConnectionState.Connected) {
            return connection.invoke("LeaveGameRoom", gameId.toString());
        }
        console.warn("SignalR: Próba opuszczenia pokoju bez aktywnego połączenia.");
        return undefined;
    }
};

export default signalRService;