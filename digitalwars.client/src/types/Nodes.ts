import type { IDecisonCard } from './Game'

export interface ICardNode {
  id: string
  type: string
  position: { x: number; y: number }
  data: {
    label: string
    cardType: 'Decision' | 'Software' | 'Hardware'
    card: IDecisonCard
    tables: Array<{ teamId: number; teamName: string; teamColor: string }>
    layoutDirection: 'TB' | 'LR'
  }
}
export interface ICardEdge {
  id: string
  source: string
  target: string
  markerEnd?: any
  style: any
  class: any
}
