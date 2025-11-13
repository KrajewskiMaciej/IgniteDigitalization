<template>
  <div ref="chart" class="w-full h-96"></div>
</template>

<script setup lang="ts">
import * as d3 from 'd3'
import { ref, onMounted, watch, nextTick } from 'vue'
import type { PropType } from 'vue'

// --- KROK 1: Definicja interfejsu dla danych wejściowych ---
interface PositionDataPoint {
  name: string
  position: number
}

const props = defineProps({
  // --- KROK 2: Użycie PropType do silnego typowania propsów ---
  data: {
    type: Array as PropType<PositionDataPoint[]>,
    required: true,
  },
})

// --- KROK 3: Jawne otypowanie referencji do elementu DOM ---
const chart = ref<HTMLElement | null>(null)

const drawChart = () => {
  // --- KROK 4: Zabezpieczenie przed nullem i pustymi danymi ---
  if (!chart.value || !props.data || props.data.length === 0) {
    return
  }

  const margin = { top: 20, right: 30, bottom: 80, left: 50 }
  const width = chart.value.clientWidth - margin.left - margin.right
  const height = chart.value.clientHeight - margin.top - margin.bottom

  // Czyszczenie poprzedniego wykresu
  d3.select(chart.value).selectAll('*').remove()

  const svg = d3
    .select(chart.value)
    .append('svg')
    .attr('width', width + margin.left + margin.right)
    .attr('height', height + margin.top + margin.bottom)
    .append('g')
    .attr('transform', `translate(${margin.left},${margin.top})`)

  // Skala X (dane są teraz poprawnie otypowane)
  const x = d3
    .scaleBand()
    .domain(props.data.map((d) => d.name))
    .range([0, width])
    .padding(0.2)

  // --- ULEPSZENIE: Dynamiczna skala Y ---
  const yMax = d3.max(props.data, (d) => d.position) ?? 30
  const y = d3.scaleLinear().domain([0, yMax]).range([height, 0]).nice() // Zaokrągla domenę do ładnych wartości

  // Rysowanie osi Y
  svg.append('g').call(d3.axisLeft(y)).selectAll('text').style('fill', '#ffffff')

  // Rysowanie osi X
  svg
    .append('g')
    .attr('transform', `translate(0,${height})`)
    .call(d3.axisBottom(x))
    .selectAll('text')
    .attr('transform', 'rotate(-45)')
    .style('text-anchor', 'end')
    .style('fill', '#ffffff')

  // Etykiety osi
  svg
    .append('text')
    .attr('text-anchor', 'middle')
    .attr('transform', `translate(${-35}, ${height / 2}) rotate(-90)`)
    .text('Pozycja')
    .style('fill', '#ffffff')
    .style('font-size', '14px')

  svg
    .append('text')
    .attr('text-anchor', 'middle')
    .attr('x', width / 2)
    .attr('y', height + margin.bottom - 10)
    .text('Drużyna')
    .style('fill', '#ffffff')
    .style('font-size', '14px')

  // Stylowanie osi i siatki
  svg.selectAll('.domain').style('stroke', '#ffffff')
  svg.selectAll('g.tick line').style('stroke', 'rgba(255, 255, 255, 0.2)')

  // Rysowanie słupków
  svg
    .selectAll('rect')
    .data(props.data)
    .enter()
    .append('rect')
    // --- KROK 5: Dodanie fallbacków dla funkcji skali ---
    .attr('x', (d) => x(d.name) || 0)
    .attr('y', (d) => y(d.position) || 0)
    .attr('width', x.bandwidth())
    .attr('height', (d) => height - (y(d.position) || 0))
    .attr('fill', '#a855f7')
}

onMounted(() => {
  nextTick(() => {
    drawChart()
  })
})

watch(
  () => props.data,
  () => {
    if (chart.value) drawChart() // Dodatkowe zabezpieczenie
  },
  { deep: true },
)
</script>
