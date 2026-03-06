<template>
  <div ref="container" class="flex justify-center items-center h-full w-full">
    <svg :viewBox="`0 0 ${svgWidth} ${svgHeight}`" class="w-full h-full">
      <g ref="board"></g>
    </svg>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import * as d3 from 'd3'
import { useToast } from 'vue-toastification'
import type { PropType, Ref } from 'vue'
import type { BoardConfig, Pawn } from '@/interfaces/types'
const jumpSound = new Audio('/jump.mp3')
jumpSound.volume = 0.2

const props = defineProps({
  config: {
    type: Object as PropType<BoardConfig>,
    required: true,
  },
  gameMode: {
    type: Boolean,
    default: false,
  },
  pawns: {
    type: Array as PropType<Pawn[]>,
    default: () => [],
  },
  usePercentage: {
    type: Boolean,
    default: false,
  },
})

const board: Ref<SVGGElement | null> = ref(null)
const toast = useToast()
const emit = defineEmits(['boardRendered'])

const previousPositions = ref<Map<string | number, { x: number; y: number }>>(new Map())

const ANIMATION = {
  duration: 600,
  hopHeight: 30,
  easing: d3.easeCubicOut,
  staggerDelay: 80,
}

const cellSize = computed(() => 40)
const marginLeft = computed(() => 40)
const marginRight = computed(() => 40)
const marginTop = computed(() => 80)
const marginBottom = computed(() => 80)
const boardSizeX = computed(() => props.config.cols * cellSize.value)
const boardSizeY = computed(() => props.config.rows * cellSize.value)
const svgWidth = computed(() => boardSizeX.value + marginLeft.value + marginRight.value)
const svgHeight = computed(() => boardSizeY.value + marginTop.value + marginBottom.value)
const labelsX = computed(() =>
  Array.from({ length: props.config.cols }, (_, i) => String.fromCharCode(65 + i)),
)
const labelsY = computed(() =>
  Array.from({ length: props.config.rows }, (_, i) => (i + 1).toString()),
)

const pawnPath =
  'M 225.5,294.5 C 231.979,295.491 238.646,295.824 245.5,295.5C 245.5,311.167 245.5,326.833 245.5,342.5C 179.833,342.5 114.167,342.5 48.5,342.5C 48.5,326.5 48.5,310.5 48.5,294.5C 55.1667,294.5 61.8333,294.5 68.5,294.5C 68.8256,290.116 68.4922,285.783 67.5,281.5C 59.5866,274.592 56.7533,265.925 59,255.5C 62.8813,244.72 70.548,238.72 82,237.5C 104.247,207.195 115.413,173.195 115.5,135.5C 92.123,118.375 84.2897,95.7084 92,67.5C 105.287,36.9129 128.453,24.4129 161.5,30C 194.896,41.2769 209.063,64.4436 204,99.5C 200.388,115.436 191.722,127.769 178,136.5C 178.299,166.031 185.633,193.698 200,219.5C 204.448,226.282 209.281,232.782 214.5,239C 235.289,244.415 241.123,256.915 232,276.5C 226.213,280.998 224.047,286.998 225.5,294.5 Z'
const originalCenterX = 147
const originalCenterY = 186

function splitLabelIntoLines(text: string, maxLength = 15): string[] {
  if (!text || text.length <= maxLength) {
    return [text || '']
  }
  const words = text.split(' ')
  if (words.length === 1) {
    return [
      text.substring(0, Math.ceil(text.length / 2)),
      text.substring(Math.ceil(text.length / 2)),
    ]
  }
  let bestSplitIndex = 0
  let bestDiff = text.length
  for (let i = 1; i < words.length; i++) {
    const l1 = words.slice(0, i).join(' ')
    const l2 = words.slice(i).join(' ')
    const diff = Math.abs(l1.length - l2.length)
    if (diff < bestDiff) {
      bestDiff = diff
      bestSplitIndex = i
    }
  }
  return [words.slice(0, bestSplitIndex).join(' '), words.slice(bestSplitIndex).join(' ')]
}

const getScreenPosition = (x: number, y: number) => ({
  screenX: x * cellSize.value + marginLeft.value + cellSize.value / 2,
  screenY: (props.config.rows - 1 - y) * cellSize.value + marginTop.value + cellSize.value / 2,
})

const processedPawns = computed(() => {
  const cols = props.config.cols
  const rows = props.config.rows
  const result = props.pawns.map((pawn) => {
    if (!props.usePercentage) return { ...pawn }
    const pctX = Math.min(pawn.maxX > 0 ? pawn.x / pawn.maxX : 0, 1)
    const pctY = Math.min(pawn.maxY > 0 ? pawn.y / pawn.maxY : 0, 1)
    return { ...pawn, x: pctX * (cols - 1), y: pctY * (rows - 1) }
  })

  if (props.usePercentage && result.length > 0) {
    result.forEach((pawn, i) => {
      const raw = props.pawns[i]
    })
  }

  return result
})

const calculateGroupOffsets = (count: number, index: number) => {
  if (count === 1) return { offsetX: 0, offsetY: 0 }
  const radius = Math.min(cellSize.value / 4, 10)
  const angle = (index / count) * 2 * Math.PI
  return {
    offsetX: Math.cos(angle) * radius,
    offsetY: Math.sin(angle) * radius,
  }
}

const hopInterpolator = (t: number) => Math.sin(t * Math.PI)

const animatePawnMove = (
  pawnGroup: d3.Selection<SVGGElement, unknown, null, undefined>,
  fromX: number,
  fromY: number,
  toX: number,
  toY: number,
  baseScale: number,
  delay: number = 0,
) => {
  jumpSound.play()
  pawnGroup
    .transition()
    .delay(delay)
    .duration(ANIMATION.duration)
    .ease(ANIMATION.easing)
    .attrTween('transform', () => {
      return (t: number) => {
        const currentX = fromX + (toX - fromX) * t
        const currentY = fromY + (toY - fromY) * t
        const hop = hopInterpolator(t) * ANIMATION.hopHeight
        const scale = baseScale * (1 + hopInterpolator(t) * 0.15)

        return `translate(${currentX}, ${currentY - hop}) scale(${scale}) translate(${-originalCenterX}, ${-originalCenterY})`
      }
    })
}

const drawBoard = (animate = true) => {
  if (!board.value) return

  const svg = d3.select(board.value)

  const existingPawns = new Map<
    string | number,
    d3.Selection<SVGGElement, unknown, null, undefined>
  >()
  svg.selectAll<SVGGElement, unknown>('.pawn-group').each(function () {
    const group = d3.select(this)
    const id = group.attr('data-pawn-id')
    if (id) existingPawns.set(id, group)
  })

  svg.selectAll('*:not(.pawn-group)').remove()

  for (let row = 0; row < props.config.rows; row++) {
    for (let col = 0; col < props.config.cols; col++) {
      svg
        .append('rect')
        .attr('x', col * cellSize.value + marginLeft.value)
        .attr('y', (props.config.rows - 1 - row) * cellSize.value + marginTop.value)
        .attr('width', cellSize.value)
        .attr('height', cellSize.value)
        .attr('fill', props.config.cellColor || '#fefae0')
        .attr('stroke', props.config.borderColor || '#595959')
        .attr('stroke-width', 1)
    }
  }

  labelsX.value.forEach((label: string, i: number) => {
    svg
      .append('text')
      .attr('x', i * cellSize.value + marginLeft.value + cellSize.value / 2)
      .attr('y', boardSizeY.value + marginTop.value + 20)
      .attr('text-anchor', 'middle')
      .attr('font-size', cellSize.value * 0.25)
      .attr('fill', 'white')
      .text(label)
  })

  if (props.config.labelsUp) {
    props.config.labelsUp.forEach((label, i) => {
      const centerX = marginLeft.value + (2 * i + 1) * cellSize.value
      const baseY = marginTop.value - 20
      const lines = splitLabelIntoLines(label)
      const textElement = svg
        .append('text')
        .attr('x', centerX)
        .attr('y', baseY - (lines.length - 1) * 15)
        .attr('text-anchor', 'middle')
        .attr('font-size', cellSize.value * 0.25)
        .attr('fill', 'white')
      lines.forEach((line, j) => {
        textElement
          .append('tspan')
          .attr('x', centerX)
          .attr('dy', j > 0 ? 15 : 0)
          .text(line)
      })
    })
  }

  labelsY.value.forEach((label: string, i: number) => {
    svg
      .append('text')
      .attr('x', marginLeft.value - 20)
      .attr(
        'y',
        (props.config.rows - 1 - i) * cellSize.value + marginTop.value + cellSize.value / 2,
      )
      .attr('text-anchor', 'middle')
      .attr('dominant-baseline', 'middle')
      .attr('font-size', cellSize.value * 0.25)
      .attr('fill', 'white')
      .text(label)
  })

  if (props.config.labelsRight) {
    props.config.labelsRight.forEach((label, i) => {
      svg
        .append('text')
        .attr(
          'transform',
          `translate(${marginLeft.value + boardSizeX.value + 20}, ${marginTop.value + (props.config.rows - i * 2 - 1) * cellSize.value}) rotate(-90)`,
        )
        .attr('text-anchor', 'middle')
        .attr('dominant-baseline', 'middle')
        .attr('font-size', cellSize.value * 0.25)
        .attr('fill', 'white')
        .text(label)
    })
  }

  const descLeft = (props.config as any).description_Left || props.config.descriptionLeft || ''
  const descDown = (props.config as any).description_Down || props.config.descriptionDown || ''

  svg
    .append('text')
    .attr(
      'transform',
      `translate(${marginLeft.value - 30}, ${marginTop.value + boardSizeY.value / 2}) rotate(-90)`,
    )
    .attr('text-anchor', 'middle')
    .attr('font-size', cellSize.value * 0.3)
    .attr('font-weight', 'bold')
    .attr('fill', 'white')
    .text(descLeft)

  svg
    .append('text')
    .attr('x', marginLeft.value + boardSizeX.value / 2)
    .attr('y', boardSizeY.value + marginTop.value + 40)
    .attr('text-anchor', 'middle')
    .attr('font-size', cellSize.value * 0.3)
    .attr('font-weight', 'bold')
    .attr('fill', 'white')
    .text(descDown)

  const borderColors = (props.config as any).Borders_Colors || props.config.borderColors

  for (let i = 0; i < props.config.cols; i += 2) {
    svg
      .append('line')
      .attr('x1', marginLeft.value + i * cellSize.value)
      .attr('y1', marginTop.value)
      .attr('x2', marginLeft.value + (i + 2) * cellSize.value)
      .attr('y2', marginTop.value)
      .attr('stroke', borderColors[(i / 2) % borderColors.length])
      .attr('stroke-width', 3)
  }
  for (let i = 0; i < props.config.cols; i += 2) {
    svg
      .append('line')
      .attr('x1', marginLeft.value + i * cellSize.value)
      .attr('y1', marginTop.value + boardSizeY.value)
      .attr('x2', marginLeft.value + (i + 2) * cellSize.value)
      .attr('y2', marginTop.value + boardSizeY.value)
      .attr('stroke', borderColors[(i / 2) % borderColors.length])
      .attr('stroke-width', 3)
  }
  for (let i = 0; i < props.config.rows; i += 2) {
    svg
      .append('line')
      .attr('x1', marginLeft.value)
      .attr('y1', marginTop.value + boardSizeY.value - i * cellSize.value)
      .attr('x2', marginLeft.value)
      .attr('y2', marginTop.value + boardSizeY.value - (i + 2) * cellSize.value)
      .attr('stroke', borderColors[(i / 2) % borderColors.length])
      .attr('stroke-width', 3)
  }
  for (let i = 0; i < props.config.rows; i += 2) {
    svg
      .append('line')
      .attr('x1', marginLeft.value + boardSizeX.value)
      .attr('y1', marginTop.value + boardSizeY.value - i * cellSize.value)
      .attr('x2', marginLeft.value + boardSizeX.value)
      .attr('y2', marginTop.value + boardSizeY.value - (i + 2) * cellSize.value)
      .attr('stroke', borderColors[(i / 2) % borderColors.length])
      .attr('stroke-width', 3)
  }

  if (Array.isArray(processedPawns.value) && processedPawns.value.length > 0) {
    existingPawns.forEach((group) => group.remove())

    const grouped = d3.group(processedPawns.value, (d) => `${d.x},${d.y}`)

    let animationIndex = 0

    grouped.forEach((group) => {
      const count = group.length
      const scaleFactor = 1 / Math.sqrt(count)
      const baseScale = cellSize.value * 0.002 * scaleFactor

      group.forEach((pawn, index: number) => {
        const { screenX, screenY } = getScreenPosition(pawn.x, pawn.y)
        const { offsetX, offsetY } = calculateGroupOffsets(count, index)
        const targetX = screenX + offsetX
        const targetY = screenY + offsetY

        const prevPos = previousPositions.value.get(pawn.id)
        const hasMoved = prevPos && (prevPos.x !== pawn.x || prevPos.y !== pawn.y)

        const pawnGroup = svg
          .append('g')
          .attr('class', 'pawn-group')
          .attr('data-pawn-id', String(pawn.id))

        pawnGroup
          .append('path')
          .attr('class', `pawn pawn-${pawn.id}`)
          .attr('d', pawnPath)
          .attr('fill', pawn.color || 'gray')
          .attr('stroke', 'black')
          .attr('stroke-width', 1.2 / baseScale)

        if (animate && hasMoved && prevPos) {
          const { screenX: fromScreenX, screenY: fromScreenY } = getScreenPosition(
            prevPos.x,
            prevPos.y,
          )

          pawnGroup.attr(
            'transform',
            `translate(${fromScreenX}, ${fromScreenY}) scale(${baseScale}) translate(${-originalCenterX}, ${-originalCenterY})`,
          )

          animatePawnMove(
            pawnGroup,
            fromScreenX,
            fromScreenY,
            targetX,
            targetY,
            baseScale,
            animationIndex * ANIMATION.staggerDelay,
          )
          animationIndex++
        } else {
          pawnGroup.attr(
            'transform',
            `translate(${targetX}, ${targetY}) scale(${baseScale}) translate(${-originalCenterX}, ${-originalCenterY})`,
          )
        }

        pawnGroup.append('title').text(pawn.name || `Pionek ${pawn.id}`)

        previousPositions.value.set(pawn.id, { x: pawn.x, y: pawn.y })
      })
    })

    const currentIds = new Set(processedPawns.value.map((p) => p.id))
    previousPositions.value.forEach((_, id) => {
      if (!currentIds.has(id as any)) {
        previousPositions.value.delete(id)
      }
    })
  }
}

watch(
  () => props.config,
  () => drawBoard(false),
  { deep: true },
)
watch(
  processedPawns,
  () => drawBoard(true),
  { deep: true },
)

onMounted(() => {
  try {
    processedPawns.value.forEach((pawn) => {
      previousPositions.value.set(pawn.id, { x: pawn.x, y: pawn.y })
    })
    drawBoard(false)
    emit('boardRendered', true)
  } catch (error: any) {
    console.error('Błąd podczas pierwszego rysowania planszy:', error)
    toast.error(`Wystąpił krytyczny błąd podczas rysowania planszy: ${error.message}`)
  }
})
</script>
