<template>
  <div ref="container" class="flex justify-center items-center h-full w-full">
    <svg 
      :viewBox="`0 0 ${svgWidth} ${svgHeight}`"
      class="w-full h-full">
      <!-- POPRAWKA: Typ refa jest teraz poprawnie zdefiniowany -->
      <g ref="board"></g>
    </svg>
  </div>
</template>
  
<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
// BŁĄD TS2307: Upewnij się, że biblioteka jest zainstalowana: npm install d3 @types/d3
import * as d3 from 'd3';
import { useToast } from 'vue-toastification';
import type { PropType } from 'vue';

// --- DEFINICJE INTERFEJSÓW ---
interface BoardConfig {
  Cols: number;
  Rows: number;
  CellColor?: string;
  BorderColor?: string;
  LabelsUp?: string[];
  LabelsRight?: string[];
  DescriptionLeft?: string;
  DescriptionDown?: string;
  BorderColors?: string[];
}

interface Pawn {
  id: number;
  x: number;
  y: number;
  color: string;
  name?: string;
}

// --- PROPSY ---
const props = defineProps({
  config: {
    type: Object as PropType<BoardConfig>,
    required: true
  },
  gameMode: {
    type: Boolean,
    default: false
  },
  pawns: {
    type: Array as PropType<Pawn[]>,
    default: () => [],
  },
  // POPRAWKA: Dodano brakujące propsy
  posX: {
    type: Number,
    default: 0
  },
  posY: {
    type: Number,
    default: 0
  }
});

// --- Referencje do elementów i konfiguracja ---
const board = ref<SVGGElement | null>(null);
const toast = useToast();
const currentPosX = ref(props.posX);
const currentPosY = ref(props.posY);

const emit = defineEmits(['boardRendered']);

// --- Właściwości Obliczeniowe ---
const cellSize = computed(() => 40);
const marginLeft = computed(() => 40);
const marginRight = computed(() => 40);
const marginTop = computed(() => 80);
const marginBottom = computed(() => 80);
const boardSizeX = computed(() => props.config.Cols * cellSize.value);
const boardSizeY = computed(() => props.config.Rows * cellSize.value);
const svgWidth = computed(() => boardSizeX.value + marginLeft.value + marginRight.value);
const svgHeight = computed(() => boardSizeY.value + marginTop.value + marginBottom.value);
const labelsX = computed(() => Array.from({ length: props.config.Cols }, (_, i) => String.fromCharCode(65 + i)));
const labelsY = computed(() => Array.from({ length: props.config.Rows }, (_, i) => (i + 1).toString()));

// --- Funkcje ---
// POPRAWKA: Dodano typy do parametrów
function splitLabelIntoLines(text: string, maxLength = 15): string[] {
  if (!text || text.length <= maxLength) {
    return [text || ''];
  }
  const words = text.split(' ');
  if (words.length === 1) {
    return [
      text.substring(0, Math.ceil(text.length / 2)),
      text.substring(Math.ceil(text.length / 2))
    ];
  }
  let bestSplitIndex = 0;
  let bestDiff = text.length;
  for (let i = 1; i < words.length; i++) {
    const potentialFirstLine = words.slice(0, i).join(' ');
    const potentialSecondLine = words.slice(i).join(' ');
    const diff = Math.abs(potentialFirstLine.length - potentialSecondLine.length);
    if (diff < bestDiff) {
      bestDiff = diff;
      bestSplitIndex = i;
    }
  }
  return [
    words.slice(0, bestSplitIndex).join(' '),
    words.slice(bestSplitIndex).join(' ')
  ];
}

const drawBoard = () => {
  if (!board.value) return;

  const svg = d3.select(board.value);
  svg.selectAll("*").remove();

  for (let row = 0; row < props.config.Rows; row++) {
    for (let col = 0; col < props.config.Cols; col++) {
      svg.append("rect")
        .attr("x", col * cellSize.value + marginLeft.value)
        .attr("y", (props.config.Rows - 1 - row) * cellSize.value + marginTop.value)
        .attr("width", cellSize.value)
        .attr("height", cellSize.value)
        .attr("fill", props.config.CellColor || '#fefae0')
        .attr("stroke", props.config.BorderColor || '#595959')
        .attr("stroke-width", 1);
    }
  }

  // POPRAWKA: Dodano typy w .forEach
  labelsX.value.forEach((label: string, i: number) => {
    svg.append("text").attr("x", i * cellSize.value + marginLeft.value + cellSize.value / 2).attr("y", boardSizeY.value + marginTop.value + 20).attr("text-anchor", "middle").attr("font-size", cellSize.value * 0.25).attr("fill", "white").text(label);
  });

  if (props.config.LabelsUp) {
    props.config.LabelsUp.forEach((label, i) => {
      const centerX = marginLeft.value + (2 * i + 1) * cellSize.value;
      const baseY = marginTop.value - 20;
      const lines = splitLabelIntoLines(label);
      const textElement = svg.append("text").attr("x", centerX).attr("y", baseY - ((lines.length - 1) * 15)).attr("text-anchor", "middle").attr("font-size", cellSize.value * 0.25).attr("fill", "white");
      lines.forEach((line, j) => {
        textElement.append("tspan").attr("x", centerX).attr("dy", j > 0 ? 15 : 0).text(line);
      });
    });
  }

  labelsY.value.forEach((label: string, i: number) => {
    svg.append("text").attr("x", marginLeft.value - 20).attr("y", (props.config.Rows - 1 - i) * cellSize.value + marginTop.value + cellSize.value / 2).attr("text-anchor", "middle").attr("dominant-baseline", "middle").attr("font-size", cellSize.value * 0.25).attr("fill", "white").text(label);
  });

  if (props.config.LabelsRight) {
    props.config.LabelsRight.forEach((label, i) => {
      svg.append("text").attr("transform", `translate(${marginLeft.value + boardSizeX.value + 20}, ${marginTop.value + (props.config.Rows - i * 2 - 1) * cellSize.value}) rotate(-90)`).attr("text-anchor", "middle").attr("dominant-baseline", "middle").attr("font-size", cellSize.value * 0.25).attr("fill", "white").text(label);
    });
  }

  svg.append("text").attr("transform", `translate(${marginLeft.value - 30}, ${marginTop.value + boardSizeY.value / 2}) rotate(-90)`).attr("text-anchor", "middle").attr("font-size", cellSize.value * 0.3).attr("font-weight", "bold").attr("fill", "white").text(props.config.DescriptionLeft || '');
  svg.append("text").attr("x", marginLeft.value + boardSizeX.value / 2).attr("y", boardSizeY.value + marginTop.value + 40).attr("text-anchor", "middle").attr("font-size", cellSize.value * 0.3).attr("font-weight", "bold").attr("fill", "white").text(props.config.DescriptionDown || '');

  const borderColors = props.config.BorderColors || ['#008000', '#FFFF00', '#FFA500', '#FF0000'];
  for (let i = 0; i < props.config.Cols; i += 2) {
    svg.append("line").attr("x1", marginLeft.value + i * cellSize.value).attr("y1", marginTop.value).attr("x2", marginLeft.value + (i + 2) * cellSize.value).attr("y2", marginTop.value).attr("stroke", borderColors[(i / 2) % borderColors.length]).attr("stroke-width", 3);
  }
  for (let i = 0; i < props.config.Cols; i += 2) {
    svg.append("line").attr("x1", marginLeft.value + i * cellSize.value).attr("y1", marginTop.value + boardSizeY.value).attr("x2", marginLeft.value + (i + 2) * cellSize.value).attr("y2", marginTop.value + boardSizeY.value).attr("stroke", borderColors[(i / 2) % borderColors.length]).attr("stroke-width", 3);
  }
  for (let i = 0; i < props.config.Rows; i += 2) {
    svg.append("line").attr("x1", marginLeft.value).attr("y1", marginTop.value + boardSizeY.value - i * cellSize.value).attr("x2", marginLeft.value).attr("y2", marginTop.value + boardSizeY.value - (i + 2) * cellSize.value).attr("stroke", borderColors[(i / 2) % borderColors.length]).attr("stroke-width", 3);
  }
  for (let i = 0; i < props.config.Rows; i += 2) {
    svg.append("line").attr("x1", marginLeft.value + boardSizeX.value).attr("y1", marginTop.value + boardSizeY.value - i * cellSize.value).attr("x2", marginLeft.value + boardSizeX.value).attr("y2", marginTop.value + boardSizeY.value - (i + 2) * cellSize.value).attr("stroke", borderColors[(i / 2) % borderColors.length]).attr("stroke-width", 3);
  }

  const pawnPath = "M 225.5,294.5 C 231.979,295.491 238.646,295.824 245.5,295.5C 245.5,311.167 245.5,326.833 245.5,342.5C 179.833,342.5 114.167,342.5 48.5,342.5C 48.5,326.5 48.5,310.5 48.5,294.5C 55.1667,294.5 61.8333,294.5 68.5,294.5C 68.8256,290.116 68.4922,285.783 67.5,281.5C 59.5866,274.592 56.7533,265.925 59,255.5C 62.8813,244.72 70.548,238.72 82,237.5C 104.247,207.195 115.413,173.195 115.5,135.5C 92.123,118.375 84.2897,95.7084 92,67.5C 105.287,36.9129 128.453,24.4129 161.5,30C 194.896,41.2769 209.063,64.4436 204,99.5C 200.388,115.436 191.722,127.769 178,136.5C 178.299,166.031 185.633,193.698 200,219.5C 204.448,226.282 209.281,232.782 214.5,239C 235.289,244.415 241.123,256.915 232,276.5C 226.213,280.998 224.047,286.998 225.5,294.5 Z";
  const originalCenterX = 147;
  const originalCenterY = 186;

  if (Array.isArray(props.pawns)) {
    // POPRAWKA: Jawne typowanie obiektu grupującego
    const grouped: Record<string, Pawn[]> = {};
    props.pawns.forEach(pawn => {
      const key = `${pawn.x},${pawn.y}`;
      if (!grouped[key]) grouped[key] = [];
      grouped[key].push(pawn);
    });

    Object.entries(grouped).forEach(([key, group]) => {
      const [x, y] = key.split(',').map(Number);
      const baseX = x * cellSize.value + marginLeft.value + cellSize.value / 2;
      const baseY = (props.config.Rows - 1 - y) * cellSize.value + marginTop.value + cellSize.value / 2;
      const count = group.length;
      const radius = Math.min(cellSize.value / 4, 10 + count * 2);
      const scaleFactor = 1 / Math.sqrt(count);
      const baseScale = cellSize.value * 0.002 * scaleFactor;

      group.forEach((pawn: Pawn, index: number) => {
        const angle = (index / count) * 2 * Math.PI;
        const offsetX = count === 1 ? 0 : Math.cos(angle) * radius;
        const offsetY = count === 1 ? 0 : Math.sin(angle) * radius;
        const pawnGroup = svg.append("g");
        pawnGroup.append("path").attr("class", `pawn pawn-${pawn.id}`).attr("d", pawnPath).attr("fill", pawn.color || "gray").attr("stroke", "black").attr("stroke-width", 1.2 / baseScale).attr("transform", `translate(${baseX + offsetX}, ${baseY + offsetY}) scale(${baseScale}) translate(${-originalCenterX}, ${-originalCenterY})`);
        pawnGroup.append("title").text(pawn.name || `Pionek ${pawn.id}`);
      });
    });
  }
};

const animatePawn = () => {
  if (!board.value) return;
  const pawn = d3.select(board.value).select(".pawn");
  if (pawn.empty()) return;

  const newX = props.posX * cellSize.value + marginLeft.value + cellSize.value / 2;
  const newY = (props.config.Rows - 1 - props.posY) * cellSize.value + marginTop.value + cellSize.value / 2;

  const baseScale = cellSize.value * 0.002;
  const originalCenterX = 147;
  const originalCenterY = 186;

  pawn.transition().duration(500).ease(d3.easeCubicInOut)
    .attr("transform", `translate(${newX}, ${newY}) scale(${baseScale}) translate(${-originalCenterX}, ${-originalCenterY})`)
    .on("end", () => {
      currentPosX.value = props.posX;
      currentPosY.value = props.posY;
    });
}

// --- Watchery i Cykl Życia ---
watch(() => [props.config, props.pawns], drawBoard, { deep: true });
watch([() => props.posX, () => props.posY], animatePawn);

onMounted(() => {
  try {
    drawBoard();
    emit('boardRendered', true);
  } catch (error: any) {
    console.error('Błąd podczas rysowania planszy:', error);
    toast.error(`Błąd: ${error.message}`);
  }
});
</script>