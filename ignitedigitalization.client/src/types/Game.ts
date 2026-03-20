export interface IGameResponse {
  id: number
  name: string
  status: string
  deckId: number
}

export interface ITeamManagmentResponse {
  teamId: number
  teamName: string
  teamColor: string
  boradId: number
  deckId: number
  teamToken: string
}

export interface ICardTypes {
  cards_Id: number
  card_Id: number
  cardType: 'Decision' | 'Software' | 'Hardware'
}

export interface IEnablersMapResponse {
  cardTypes: ICardTypes[]
  enablersMap: Record<number, number[]>
}

export interface IDecisonCard {
  id: number
  deckId: number
  title: string
  description: string
}

export interface IPendingEnablerChange {
  cardId: number
  cardsId: number
  enablers: number[]
}
