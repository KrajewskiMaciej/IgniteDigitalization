<template>
  <div class="flex flex-wrap justify-center gap-8 p-4">
    <div
      v-for="(teamData, index) in data"
      :key="index"
      class="w-64 h-80 flex flex-col items-center bg-secondary p-4 rounded-lg shadow-lg border border-gray-700"
    >
      <!-- Kontener na wykres z unikalnym ID -->
      <div
        :id="`chart-container-${index}`"
        class="w-full h-48 flex items-center justify-center"
      ></div>

      <p class="mt-4 text-lg text-white font-bold">{{ teamData.team }}</p>

      <div class="text-sm text-white mt-2 text-center">
        <span
          >Sukcesy: <span class="text-green-400 font-bold">{{ teamData.success }}</span></span
        >
        <span class="mx-2">|</span>
        <span
          >Porażki: <span class="text-red-400 font-bold">{{ teamData.failure }}</span></span
        >
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as d3 from 'd3'
import { onMounted, nextTick, watch } from 'vue'
import type { PropType } from 'vue'

// --- KROK 1: Definicja interfejsu dla danych wejściowych ---
interface TeamSuccessData {
  team: string
  success: number
  failure: number
}

const props = defineProps({
  // --- KROK 2: Użycie PropType do silnego typowania propsów ---
  data: {
    type: Array as PropType<TeamSuccessData[]>,
    required: true,
  },
})

const drawCharts = () => {
  // --- KROK 3: Zabezpieczenie przed pustymi danymi ---
  if (!props.data) {
    return
  }

  props.data.forEach((teamData, index) => {
    // Unikalny selektor dla każdego kontenera
    const selector = `#chart-container-${index}`
    const container = d3.select(selector)

    // Czyszczenie poprzedniego wykresu
    container.selectAll('*').remove()

    // Sprawdzenie, czy są jakiekolwiek dane do wyświetlenia
    const total = teamData.success + teamData.failure
    if (total === 0) {
      container.append('p').attr('class', 'text-gray-500 text-center').text('Brak danych')
      return
    }

    const width = 180
    const height = 180
    const radius = Math.min(width, height) / 2 - 10

    const svg = container
      .append('svg')
      .attr('width', width)
      .attr('height', height)
      .append('g')
      .attr('transform', `translate(${width / 2},${height / 2})`)

    const dataEntries: [string, number][] = [
      ['success', teamData.success],
      ['failure', teamData.failure],
    ]

    // --- KROK 4: Użycie generyków dla d3.pie ---
    const pie = d3
      .pie<[string, number]>()
      .value((d) => d[1]) // d[1] jest teraz poprawnie rozpoznawane jako number
      .sort(null)

    const data_ready = pie(dataEntries)

    // --- KROK 5: Użycie generyków dla d3.arc ---
    const arcGenerator = d3
      .arc<d3.PieArcDatum<[string, number]>>()
      .innerRadius(radius * 0.6) // Wykres pierścieniowy
      .outerRadius(radius)
      .cornerRadius(3)

    const color = d3
      .scaleOrdinal<string>()
      .domain(['success', 'failure'])
      .range(['#4ade80', '#f87171'])

    // Rysowanie wycinków
    svg
      .selectAll('path')
      .data(data_ready)
      .enter()
      .append('path')
      .attr('d', arcGenerator) // Poprawne wywołanie generatora
      .attr('fill', (d) => color(d.data[0])) // d.data[0] to klucz ('success' lub 'failure')
      .attr('stroke', '#1e293b')
      .style('stroke-width', '4px')
  })
}

onMounted(() => {
  nextTick(() => {
    drawCharts()
  })
})

watch(
  () => props.data,
  () => {
    nextTick(() => {
      drawCharts()
    })
  },
  { deep: true },
)
</script>
