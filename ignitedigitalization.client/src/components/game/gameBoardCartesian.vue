<template>
  <div ref="container" class="flex justify-center items-center h-full w-full">
    <svg :viewBox="`0 0 ${svgWidth} ${svgHeight}`" class="w-full h-full max-h-screen">
      <g ref="board"></g>
    </svg>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import * as d3 from 'd3'
import type { PropType, Ref } from 'vue'
import type { BoardConfig, Pawn } from '@/interfaces/types'

const jumpSound = new Audio('/jump.mp3')
jumpSound.volume = 0.2

const props = defineProps({
  config: {
    type: Object as PropType<BoardConfig>,
    required: true,
  },
  pawns: {
    type: Array as PropType<Pawn[]>,
    default: () => [],
  },
  circleMode: {
    type: Boolean,
    default: false,
  },
})

const board: Ref<SVGGElement | null> = ref(null)
const emit = defineEmits(['boardRendered'])
const previousPositions = ref<Map<string | number, { x: number; y: number }>>(new Map())

const ANIMATION = {
  duration: 600,
  hopHeight: 30,
  easing: d3.easeCubicOut,
  staggerDelay: 80,
}

// Obliczenia wymiarów
const cellSize = computed(() => 45)
const margin = 110 
const boardSizeX = computed(() => props.config.cols * cellSize.value)
const boardSizeY = computed(() => props.config.rows * cellSize.value)
const svgWidth = computed(() => boardSizeX.value + margin * 1.6)
const svgHeight = computed(() => boardSizeY.value + margin * 1.6)
const centerX = computed(() => svgWidth.value / 2)
const centerY = computed(() => svgHeight.value / 2)

// Ścieżka pionka i punkt obrotu/skalowania
const pawnPath = 'M 225.5,294.5 C 231.979,295.491 238.646,295.824 245.5,295.5C 245.5,311.167 245.5,326.833 245.5,342.5C 179.833,342.5 114.167,342.5 48.5,342.5C 48.5,326.5 48.5,310.5 48.5,294.5C 55.1667,294.5 61.8333,294.5 68.5,294.5C 68.8256,290.116 68.4922,285.783 67.5,281.5C 59.5866,274.592 56.7533,265.925 59,255.5C 62.8813,244.72 70.548,238.72 82,237.5C 104.247,207.195 115.413,173.195 115.5,135.5C 92.123,118.375 84.2897,95.7084 92,67.5C 105.287,36.9129 128.453,24.4129 161.5,30C 194.896,41.2769 209.063,64.4436 204,99.5C 200.388,115.436 191.722,127.769 178,136.5C 178.299,166.031 185.633,193.698 200,219.5C 204.448,226.282 209.281,232.782 214.5,239C 235.289,244.415 241.123,256.915 232,276.5C 226.213,280.998 224.047,286.998 225.5,294.5 Z'
const originalCenterX = 147
const originalCenterY = 186

const getScreenPosition = (x: number, y: number) => ({
  screenX: centerX.value + x * cellSize.value,
  screenY: centerY.value - y * cellSize.value,
})

const calculateGroupOffsets = (count: number, index: number) => {
  if (count <= 1) return { offsetX: 0, offsetY: 0 }
  const radius = 14
  const angle = (index / count) * 2 * Math.PI
  return { offsetX: Math.cos(angle) * radius, offsetY: Math.sin(angle) * radius }
}

// Pomocnicza funkcja zawijania tekstu – max maxLen znaków na wiersz, bez łamania wyrazów
const wrapText = (text: string, maxLen = 14): string[] => {
  const words = text.split(/\s+/)
  const lines: string[] = []
  let current = ''
  for (const word of words) {
    if (!current) {
      current = word
    } else if ((current + ' ' + word).length <= maxLen) {
      current += ' ' + word
    } else {
      lines.push(current)
      current = word
    }
  }
  if (current) lines.push(current)
  return lines
}

const processedPawns = computed(() => {
  const halfCols = props.config.cols / 2
  const halfRows = props.config.rows / 2

  // Dynamiczne mapowanie nazw ćwiartek z cells_Descriptions
  // Kolejność: 0=TL, 1=TR, 2=BL, 3=BR
  const cfg = props.config as any
  const descRaw: string = cfg.cells_Descriptions ?? cfg.cellsDescriptions ?? ''
  const quadrantNames = descRaw.split(';').map((s: string) => s.trim().toUpperCase())

  const nameToQuadrantIndex: Record<string, number> = {}
  quadrantNames.forEach((name, idx) => {
    if (name) nameToQuadrantIndex[name] = idx
  })

  return props.pawns.map((pawn, index) => {
    const nameKey = pawn.name?.toUpperCase() || ''
    const quadrant = nameToQuadrantIndex[nameKey] !== undefined
      ? nameToQuadrantIndex[nameKey]
      : (index % 4)

    // Procent pozycji względem maksymalnej osiągalnej (0.0 – 1.0, clamp do 1)
    const pctX = Math.min(pawn.maxX > 0 ? pawn.x / pawn.maxX : 0, 1)
    const pctY = Math.min(pawn.maxY > 0 ? pawn.y / pawn.maxY : 0, 1)

    let targetX: number
    let targetY: number
    switch (quadrant) {
      case 0: targetX = -pctX * halfCols; targetY =  pctY * halfRows; break // Top-Left
      case 1: targetX =  pctX * halfCols; targetY =  pctY * halfRows; break // Top-Right
      case 2: targetX =  pctX * halfCols; targetY = -pctY * halfRows; break // Bottom-Right
      case 3: targetX = -pctX * halfCols; targetY = -pctY * halfRows; break // Bottom-Left
      default: targetX = 0; targetY = 0
    }

    return { ...pawn, x: targetX, y: targetY }
  })
})

const drawBoard = (animate = true) => {
  if (!board.value) return
  const svg = d3.select(board.value)
  svg.selectAll('*').remove()

  const halfCols = props.config.cols / 2
  const halfRows = props.config.rows / 2

  // Normalizacja nazw pól – backend może wysyłać snake_case lub camelCase
  const cfg = props.config as any
  const descRaw: string = cfg.cells_Descriptions ?? cfg.cellsDescriptions ?? ''
  const colors: string[] = cfg.borders_Colors ?? cfg.borderColors ?? []
  const labelsUp: string[] = cfg.labelsUp ?? []
  const labelsRight: string[] = cfg.labelsRight ?? []
  const descLeft: string = cfg.description_Left ?? cfg.descriptionLeft ?? ''
  const descDown: string = cfg.description_Down ?? cfg.descriptionDown ?? ''
  const cellColor: string = cfg.cell_Color ?? cfg.cellColor ?? '#fefae0'
  const borderColor: string = cfg.border_Color ?? cfg.borderColor ?? '#333'

  // 1. Ćwiartki
  const names = descRaw.split(';').map((s: string) => s.trim())
  
  const quadConfigs = [
    { x: -halfCols, y: 0,         label: names[0], color: colors[0] }, // TL [0]
    { x: 0,         y: 0,         label: names[1], color: colors[1] }, // TR [1]
    { x: 0,         y: -halfRows, label: names[2], color: colors[2] }, // BR [2]
    { x: -halfCols, y: -halfRows, label: names[3], color: colors[3] }, // BL [3]
  ]

  quadConfigs.forEach(q => {
    const pos = getScreenPosition(q.x, q.y + halfRows)
    svg.append('rect')
      .attr('x', pos.screenX).attr('y', pos.screenY)
      .attr('width', halfCols * cellSize.value).attr('height', halfRows * cellSize.value)
      .attr('fill', q.color || '#eee')

    const labelPos = getScreenPosition(q.x + halfCols/2, q.y + halfRows/2)
    const lines = wrapText(q.label || '')
    const lineHeight = cellSize.value * 0.5
    const totalHeight = (lines.length - 1) * lineHeight
    const textEl = svg.append('text')
      .attr('text-anchor', 'middle')
      .attr('dominant-baseline', 'middle')
      .attr('font-size', '22px')
      .attr('fill', cellColor).attr('opacity', 0.8)
    lines.forEach((line, i) => {
      textEl.append('tspan')
        .attr('x', labelPos.screenX)
        .attr('y', labelPos.screenY - totalHeight / 2 + i * lineHeight)
        .text(line)
    })
  })

  // 2. Siatka
  for (let i = -halfCols; i <= halfCols; i++) {
    const s = getScreenPosition(i, -halfRows), e = getScreenPosition(i, halfRows)
    svg.append('line').attr('x1', s.screenX).attr('y1', s.screenY).attr('x2', e.screenX).attr('y2', e.screenY)
      .attr('stroke', borderColor).attr('stroke-width', i === 0 ? 3 : 0.5).attr('opacity', 0.5)
  }
  for (let j = -halfRows; j <= halfRows; j++) {
    const s = getScreenPosition(-halfCols, j), e = getScreenPosition(halfCols, j)
    svg.append('line').attr('x1', s.screenX).attr('y1', s.screenY).attr('x2', e.screenX).attr('y2', e.screenY)
      .attr('stroke', borderColor).attr('stroke-width', j === 0 ? 3 : 0.5).attr('opacity', 0.5)
  }

  // 3. Etykiety według schematu z planszy
  const topEdgeY = centerY.value - halfRows * cellSize.value
  const bottomEdgeY = centerY.value + halfRows * cellSize.value
  const leftEdgeX = centerX.value - halfCols * cellSize.value
  const rightEdgeX = centerX.value + halfCols * cellSize.value
  const qHalfW = (halfCols / 2) * cellSize.value  // środek każdej ćwiartki X
  const qHalfH = (halfRows / 2) * cellSize.value  // środek każdej ćwiartki Y
  const labelFontSm = `${cellSize.value * 0.28}px`


  // labelsUp[0] = nad lewą górą, [1] = nad prawą górą
  // labelsUp[2] = pod lewą dolną, [3] = pod prawą dolną
  const labelsUpPositions = [
    { x: centerX.value - qHalfW, y: topEdgeY - 16 },
    { x: centerX.value + qHalfW, y: topEdgeY - 16 },
    { x: centerX.value - qHalfW, y: bottomEdgeY + 30 },
    { x: centerX.value + qHalfW, y: bottomEdgeY + 30 },
  ]
  labelsUp.forEach((label: string, i: number) => {
    if (!label || i >= labelsUpPositions.length) return
    svg.append('text')
      .attr('x', labelsUpPositions[i].x)
      .attr('y', labelsUpPositions[i].y)
      .attr('text-anchor', 'middle')
      .attr('font-size', labelFontSm)
      .attr('fill', '#1e293b')
      .text(label)
  })

  // labelsRight[0] = lewa strona, górna ćwiartka (rotate -90)
  // labelsRight[1] = lewa strona, dolna ćwiartka
  // labelsRight[2] = prawa strona, górna ćwiartka
  // labelsRight[3] = prawa strona, dolna ćwiartka
  const labelsRightPositions = [
    { x: leftEdgeX - 25, y: centerY.value - qHalfH },
    { x: leftEdgeX - 25, y: centerY.value + qHalfH },
    { x: rightEdgeX + 25, y: centerY.value - qHalfH },
    { x: rightEdgeX + 25, y: centerY.value + qHalfH },
  ]
  labelsRight.forEach((label: string, i: number) => {
    if (!label || i >= labelsRightPositions.length) return
    svg.append('text')
      .attr('transform', `translate(${labelsRightPositions[i].x}, ${labelsRightPositions[i].y}) rotate(-90)`)
      .attr('text-anchor', 'middle')
      .attr('dominant-baseline', 'middle')
      .attr('font-size', labelFontSm)
      .attr('fill', '#1e293b')
      .text(label)
  })

  // Litery kolumn: lewa połowa D→A (od lewej do środka), prawa A→D
  for (let i = 0; i < halfCols; i++) {
    const leftLetter = String.fromCharCode(65 + halfCols - 1 - i) // D,C,B,A
    const rightLetter = String.fromCharCode(65 + i)               // A,B,C,D
    const xLeft = leftEdgeX + (i + 0.5) * cellSize.value
    const xRight = centerX.value + (i + 0.5) * cellSize.value
    // Górny rząd (nad planszą)
    svg.append('text').attr('x', xLeft).attr('y', topEdgeY - 2)
      .attr('text-anchor', 'middle').attr('font-size', labelFontSm).attr('fill', '#1e293b').text(leftLetter)
    svg.append('text').attr('x', xRight).attr('y', topEdgeY - 2)
      .attr('text-anchor', 'middle').attr('font-size', labelFontSm).attr('fill', '#1e293b').text(rightLetter)
    // Dolny rząd (pod planszą)
    svg.append('text').attr('x', xLeft).attr('y', bottomEdgeY + 14)
      .attr('text-anchor', 'middle').attr('font-size', labelFontSm).attr('fill', '#1e293b').text(leftLetter)
    svg.append('text').attr('x', xRight).attr('y', bottomEdgeY + 14)
      .attr('text-anchor', 'middle').attr('font-size', labelFontSm).attr('fill', '#1e293b').text(rightLetter)
  }

  // Numery wierszy: górna połowa 4→1 (od góry do środka), dolna 1→4
  for (let i = 0; i < halfRows; i++) {
    const topNum = halfRows - i      // 4,3,2,1
    const botNum = i + 1             // 1,2,3,4
    const yTop = topEdgeY + (i + 0.5) * cellSize.value
    const yBot = centerY.value + (i + 0.5) * cellSize.value
    // lewa strona
    svg.append('text').attr('x', leftEdgeX - 4).attr('y', yTop)
      .attr('text-anchor', 'end').attr('dominant-baseline', 'middle')
      .attr('font-size', labelFontSm).attr('fill', '#1e293b').attr('opacity', 0.7).text(topNum)
    svg.append('text').attr('x', leftEdgeX - 4).attr('y', yBot)
      .attr('text-anchor', 'end').attr('dominant-baseline', 'middle')
      .attr('font-size', labelFontSm).attr('fill', '#1e293b').attr('opacity', 0.7).text(botNum)
    // prawa strona
    svg.append('text').attr('x', rightEdgeX + 4).attr('y', yTop)
      .attr('text-anchor', 'start').attr('dominant-baseline', 'middle')
      .attr('font-size', labelFontSm).attr('fill', '#1e293b').attr('opacity', 0.7).text(topNum)
    svg.append('text').attr('x', rightEdgeX + 4).attr('y', yBot)
      .attr('text-anchor', 'start').attr('dominant-baseline', 'middle')
      .attr('font-size', labelFontSm).attr('fill', '#1e293b').attr('opacity', 0.7).text(botNum)
  }

  // 3. Logo / Środek (rysujemy PRZED pionkami żeby pionki były na wierzchu)
  const logo = svg.append('g').attr('transform', `translate(${centerX.value}, ${centerY.value})`)
  logo.append('circle').attr('r', 25).attr('fill', '#fff').attr('stroke', borderColor).attr('stroke-width', 2)

  if (!props.circleMode) {
    // 4. Wielokąt łączący pionki 1-2-3-4-1 + animacja śledząca hop pionków
    if (processedPawns.value && processedPawns.value.length >= 4) {
      const pawns4 = processedPawns.value.slice(0, 4)

      const toPoints = pawns4.map(pawn => {
        const { screenX, screenY } = getScreenPosition(pawn.x, pawn.y)
        return { x: screenX, y: screenY }
      })

      const fromPoints = pawns4.map((pawn, i) => {
        const prev = previousPositions.value.get(pawn.id)
        if (animate && prev && (prev.x !== pawn.x || prev.y !== pawn.y)) {
          const { screenX, screenY } = getScreenPosition(prev.x, prev.y)
          return { x: screenX, y: screenY }
        }
        return { x: toPoints[i].x, y: toPoints[i].y }
      })

      const hasMovement = fromPoints.some((f, i) => f.x !== toPoints[i].x || f.y !== toPoints[i].y)

      const interpolate = (t: number) =>
        toPoints.map((to, i) => {
          const from = fromPoints[i]
          const moving = from.x !== to.x || from.y !== to.y
          return {
            x: from.x + (to.x - from.x) * t,
            y: from.y + (to.y - from.y) * t - (moving ? Math.sin(t * Math.PI) * ANIMATION.hopHeight : 0),
          }
        })

      const pointsStr = (pts: { x: number; y: number }[]) =>
        pts.map(p => `${p.x},${p.y}`).join(' ')

      const polygon = svg
        .append('polygon')
        .attr('points', pointsStr(fromPoints))
        .attr('fill', 'gray')
        .attr('fill-opacity', 0.5)
        .attr('stroke', 'none')

      if (hasMovement) {
        polygon
          .transition()
          .duration(ANIMATION.duration)
          .ease(ANIMATION.easing)
          .attrTween('points', () => (t: number) => pointsStr(interpolate(t)))
      }

      const lineOrder = [0, 1, 2, 3, 0]
      for (let li = 0; li < 4; li++) {
        const ai = lineOrder[li]
        const bi = lineOrder[li + 1]

        const line = svg
          .append('line')
          .attr('x1', fromPoints[ai].x).attr('y1', fromPoints[ai].y)
          .attr('x2', fromPoints[bi].x).attr('y2', fromPoints[bi].y)
          .attr('stroke', '#475569')
          .attr('stroke-width', 1.5)
          .attr('stroke-opacity', 0.7)

        if (hasMovement) {
          line
            .transition()
            .duration(ANIMATION.duration)
            .ease(ANIMATION.easing)
            .attrTween('x1', () => (t: number) => String(interpolate(t)[ai].x))
            .attrTween('y1', () => (t: number) => String(interpolate(t)[ai].y))
            .attrTween('x2', () => (t: number) => String(interpolate(t)[bi].x))
            .attrTween('y2', () => (t: number) => String(interpolate(t)[bi].y))
        }
      }
    }

    // 5. Pionki (Procesy)
    if (processedPawns.value && processedPawns.value.length > 0) {
      const grouped = d3.group(processedPawns.value, d => `${d.x},${d.y}`)

      grouped.forEach((pawnsInCell) => {
        const count = pawnsInCell.length

        pawnsInCell.forEach((pawn, index) => {
          const { screenX, screenY } = getScreenPosition(pawn.x, pawn.y)
          const { offsetX, offsetY } = calculateGroupOffsets(count, index)
          const targetX = screenX + offsetX
          const targetY = screenY + offsetY

          const baseScale = cellSize.value * 0.002

          const pawnGroup = svg.append('g')
            .attr('class', 'pawn-group')
            .attr('data-id', pawn.id)

          pawnGroup.append('path')
            .attr('d', pawnPath)
            .attr('fill', pawn.color)
            .attr('stroke', '#000')
            .attr('stroke-width', 1.5 / baseScale)

          const prev = previousPositions.value.get(pawn.id)
          if (animate && prev && (prev.x !== pawn.x || prev.y !== pawn.y)) {
            const fromPos = getScreenPosition(prev.x, prev.y)
            pawnGroup
              .attr('transform', `translate(${fromPos.screenX}, ${fromPos.screenY}) scale(${baseScale}) translate(${-originalCenterX}, ${-originalCenterY})`)
              .transition().duration(ANIMATION.duration).ease(ANIMATION.easing)
              .attrTween('transform', () => (t: number) => {
                const curX = fromPos.screenX + (targetX - fromPos.screenX) * t
                const curY = fromPos.screenY + (targetY - fromPos.screenY) * t
                const hop = Math.sin(t * Math.PI) * ANIMATION.hopHeight
                return `translate(${curX}, ${curY - hop}) scale(${baseScale}) translate(${-originalCenterX}, ${-originalCenterY})`
              })
          } else {
            pawnGroup.attr('transform', `translate(${targetX}, ${targetY}) scale(${baseScale}) translate(${-originalCenterX}, ${-originalCenterY})`)
          }

          previousPositions.value.set(pawn.id, { x: pawn.x, y: pawn.y })
        })
      })
    }
  } else {
    // circleMode: polygon per team (grouped by color) + circles instead of pawn shapes
    const makeInterpolate = (fromPts: {x:number;y:number}[], toPts: {x:number;y:number}[]) =>
      (t: number) => toPts.map((to, i) => {
        const from = fromPts[i]
        const moving = from.x !== to.x || from.y !== to.y
        return {
          x: from.x + (to.x - from.x) * t,
          y: from.y + (to.y - from.y) * t - (moving ? Math.sin(t * Math.PI) * ANIMATION.hopHeight : 0),
        }
      })
    const pointsStr = (pts: { x: number; y: number }[]) =>
      pts.map(p => `${p.x},${p.y}`).join(' ')

    // 4. Polygons per team color
    const byColor = d3.group(processedPawns.value, (d) => d.color)
    byColor.forEach((teamPawns, teamColor) => {
      if (teamPawns.length < 2) return
      const toPoints = teamPawns.map((pawn) => {
        const { screenX, screenY } = getScreenPosition(pawn.x, pawn.y)
        return { x: screenX, y: screenY }
      })
      const fromPoints = teamPawns.map((pawn, i) => {
        const prev = previousPositions.value.get(pawn.id)
        if (animate && prev && (prev.x !== pawn.x || prev.y !== pawn.y)) {
          const { screenX, screenY } = getScreenPosition(prev.x, prev.y)
          return { x: screenX, y: screenY }
        }
        return { x: toPoints[i].x, y: toPoints[i].y }
      })
      const hasMovement = fromPoints.some((f, i) => f.x !== toPoints[i].x || f.y !== toPoints[i].y)
      const interp = makeInterpolate(fromPoints, toPoints)

      const polygon = svg.append('polygon')
        .attr('points', pointsStr(fromPoints))
        .attr('fill', teamColor)
        .attr('fill-opacity', 0.2)
        .attr('stroke', teamColor)
        .attr('stroke-width', 1.5)
        .attr('stroke-opacity', 0.75)
      if (hasMovement) {
        polygon.transition().duration(ANIMATION.duration).ease(ANIMATION.easing)
          .attrTween('points', () => (t: number) => pointsStr(interp(t)))
      }
    })

    // 5. Circles per pawn
    if (processedPawns.value && processedPawns.value.length > 0) {
      const circleR = cellSize.value * 0.15
      const grouped = d3.group(processedPawns.value, (d) => `${d.x},${d.y}`)
      grouped.forEach((pawnsInCell) => {
        const count = pawnsInCell.length
        pawnsInCell.forEach((pawn, index) => {
          const { screenX, screenY } = getScreenPosition(pawn.x, pawn.y)
          const { offsetX, offsetY } = calculateGroupOffsets(count, index)
          const targetX = screenX + offsetX
          const targetY = screenY + offsetY

          const g = svg.append('g').attr('class', 'pawn-group').attr('data-id', pawn.id)
          g.append('circle')
            .attr('r', circleR)
            .attr('fill', pawn.color)
            .attr('stroke', '#000')
            .attr('stroke-width', 1.5)

          const prev = previousPositions.value.get(pawn.id)
          if (animate && prev && (prev.x !== pawn.x || prev.y !== pawn.y)) {
            const fromPos = getScreenPosition(prev.x, prev.y)
            g.attr('transform', `translate(${fromPos.screenX}, ${fromPos.screenY})`)
              .transition().duration(ANIMATION.duration).ease(ANIMATION.easing)
              .attrTween('transform', () => (t: number) => {
                const curX = fromPos.screenX + (targetX - fromPos.screenX) * t
                const curY = fromPos.screenY + (targetY - fromPos.screenY) * t
                const hop = Math.sin(t * Math.PI) * ANIMATION.hopHeight
                return `translate(${curX}, ${curY - hop})`
              })
          } else {
            g.attr('transform', `translate(${targetX}, ${targetY})`)
          }
          previousPositions.value.set(pawn.id, { x: pawn.x, y: pawn.y })
        })
      })
    }
  }
};

// Obserwatorzy
watch(processedPawns, () => drawBoard(true), { deep: true })

onMounted(() => {
  // Zainicjuj pozycje początkowe przetworzonymi danymi
  processedPawns.value.forEach(p => previousPositions.value.set(p.id, { x: p.x, y: p.y }))
  drawBoard(false)
  emit('boardRendered', true)
})
</script>