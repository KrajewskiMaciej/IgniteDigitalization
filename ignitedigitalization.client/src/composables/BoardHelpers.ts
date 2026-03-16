import type { BoardConfig } from '@/interfaces/types'

export function fillBoardConfig(target: BoardConfig, api: any) {
  target.boardId = api.boardId
  target.name = api.name

  // Etykiety
  target.labelsUp = [...api.labelsUp]
  target.labelsRight = [...api.labelsRight]

  // Kolory
  target.cellColor = api.cell_Color
  target.borderColor = api.border_Color
  target.borderColors = [...api.borders_Colors]

  // Opisy
  target.descriptionDown = api.description_Down
  target.descriptionLeft = api.description_Left

  // Opisy komórek
  target.cellsDescriptions = api.cells_Descriptions ?? api.cellsDescriptions ?? ''

  // Wymiary
  target.rows = api.rows
  target.cols = api.cols
}
