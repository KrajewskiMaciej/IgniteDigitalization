<template>
  <div ref="chart" class="w-full h-96"></div>
</template>

<script setup lang="ts">
import * as d3 from 'd3'
import { ref, onMounted, watch, nextTick } from 'vue'
import type { PropType } from 'vue'

// --- KROK 1: Definicja interfejsu dla danych wejściowych ---
interface AvgBitsDataPoint {
  team: string
  avgBits: number
}

const props = defineProps({
  // --- KROK 2: Użycie PropType do silnego typowania propsów ---
  data: {
    type: Array as PropType<AvgBitsDataPoint[]>,
    required: true,
  },
  xAxisLabel: {
    type: String,
    default: 'Drużyna',
  },
})

// --- KROK 3: Jawne otypowanie referencji do elementu DOM ---
const chart = ref<HTMLElement | null>(null)

const drawChart = () => {
  // --- KROK 4: Zabezpieczenie przed nullem i pustymi danymi ---
  if (!chart.value || !props.data || props.data.length === 0) {
    return
  }

  const margin = { top: 30, right: 30, bottom: 70, left: 50 }
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

  // Skale X i Y (używamy otypowanych danych)
  const x = d3
    .scaleBand<string>() // Dodanie typu generycznego!
    .domain(props.data.map((d) => d.team))
    .range([0, width])
    .padding(0.2)

  const yMax = d3.max(props.data, (d) => d.avgBits) ?? 0
  const y = d3
    .scaleLinear()
    .domain([0, yMax * 1.1]) // Margines na górze
    .range([height, 0])
    .nice() // Zaokrąglenie

  // Osie
  svg
    .append('g')
    .attr('transform', `translate(0,${height})`)
    .call(d3.axisBottom(x))
    .selectAll('text')
    .style('fill', '#ffffff')
    .attr('text-anchor', 'end')
    .attr('dx', '-.8em')
    .attr('dy', '.15em')
    .attr('transform', 'rotate(-45)')

  svg.append('g').call(d3.axisLeft(y)).selectAll('text').style('fill', '#ffffff')

  // Styl osi
  svg.selectAll('.domain').style('stroke', '#ffffff')
  svg.selectAll('g.tick line').style('stroke', 'rgba(255, 255, 255, 0.2)')

  // Słupki
  svg
    .selectAll('rect')
    .data(props.data)
    .enter()
    .append('rect')
    // --- KROK 5: Bezpieczny dostęp do danych dzięki typowaniu ---
    .attr('x', (d) => x(d.team) || 0) // Poprawione
    .attr('y', (d) => y(d.avgBits) || 0) // Poprawione
    .attr('width', x.bandwidth())
    .attr('height', (d) => height - (y(d.avgBits) || 0)) // Poprawione
    .attr('fill', '#a855f7')

  // Dodanie wartości nad słupkami
  svg
    .selectAll('text.value')
    .data(props.data)
    .enter()
    .append('text')
    .attr('class', 'value')
    // --- KROK 6: Bezpieczny dostęp do danych dzięki typowaniu ---
    .attr('x', (d) => (x(d.team) || 0) + x.bandwidth() / 2) // Poprawione
    .attr('y', (d) => (y(d.avgBits) || 0) - 5) // Poprawione
    .attr('text-anchor', 'middle')
    .text((d) => d.avgBits.toFixed(1)) // Formatowanie tekstu
    .style('fill', '#ffffff')
    .style('font-size', '12px')

  // Etykiety osi
  svg
    .append('text')
    .attr('x', width / 2)
    .attr('y', height + 55)
    .attr('text-anchor', 'middle')
    .text(props.xAxisLabel) // Dostęp do props.xAxisLabel jest teraz bezpieczny
    .style('fill', '#ffffff')
    .style('font-size', '14px')

  svg
    .append('text')
    .attr('text-anchor', 'middle')
    .attr('transform', `translate(-35, ${height / 2}) rotate(-90)`)
    .text('Bity')
    .style('fill', '#ffffff')
    .style('font-size', '14px')
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
