<template>
  <div ref="chart" class="w-full h-96"></div>
</template>

<script setup lang="ts">
import * as d3 from 'd3'
import { ref, onMounted, watch, nextTick } from 'vue'
import type { PropType } from 'vue'

// --- KROK 1: Zdefiniowanie interfejsu dla danych ---
// To rozwiązuje większość problemów z typowaniem 'unknown'.
interface ChartDataPoint {
  game: number
  [key: string]: number // Pozwala na dynamiczne metryki takie jak 'positions' i 'bits'
}

const props = defineProps({
  // --- KROK 2: Użycie PropType do silnego typowania propsów ---
  data: {
    type: Array as PropType<ChartDataPoint[]>,
    required: true,
  },
})

// --- KROK 3: Jawne otypowanie referencji do elementu DOM ---
const chart = ref<HTMLElement | null>(null)

const drawChart = () => {
  // --- KROK 4: Zabezpieczenie przed nullem i pustymi danymi ---
  if (!chart.value || props.data.length === 0) {
    return
  }

  const margin = { top: 40, right: 30, bottom: 50, left: 50 }
  // Dostęp do clientWidth/clientHeight jest teraz bezpieczny
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

  // Typy są teraz poprawnie inferowane z ChartDataPoint[]
  const gameNumbers = props.data.map((d) => d.game.toString())
  const metricNames = Object.keys(props.data[0]).filter((k) => k !== 'game')

  const x = d3.scalePoint().domain(gameNumbers).range([0, width]).padding(0.5)

  // Bezpieczne obliczenie maksimum z fallbackiem
  const yMax = d3.max(props.data.flatMap((d) => metricNames.map((m) => d[m]))) ?? 10

  const y = d3
    .scaleLinear()
    .domain([0, yMax + 2])
    .range([height, 0])

  const color = d3.scaleOrdinal(d3.schemeTableau10).domain(metricNames)

  // Rysowanie osi
  svg
    .append('g')
    .attr('transform', `translate(0,${height})`)
    .call(d3.axisBottom(x))
    .selectAll('text')
    .style('fill', '#ffffff')

  svg.append('g').call(d3.axisLeft(y)).selectAll('text').style('fill', '#ffffff')

  // Stylowanie osi
  svg.selectAll('.domain').style('stroke', '#ffffff')
  svg.selectAll('line').style('stroke', 'rgba(255, 255, 255, 0.2)')

  metricNames.forEach((name) => {
    /*
     * Zmienna 'line' nie jest używana, ponieważ rysowanie linii jest zakomentowane.
     * Aby usunąć błąd ESLint, definicja również została zakomentowana.
     * W razie potrzeby odkomentuj obie części.
     */
    // const line = d3.line<ChartDataPoint>() // <-- KROK 5: Użycie generycznego typu dla d3.line
    //   .x(d => x(d.game.toString()) || 0) // <-- KROK 6: Dodanie fallbacków na wypadek `undefined`
    //   .y(d => y(d[name]) || 0)

    /*svg.append('path')
        .datum(props.data)
        .attr('fill', 'none')
        .attr('stroke', color(name))
        .attr('stroke-width', 2)
        .attr('d', line)
      */

    // Rysowanie punktów danych
    svg
      .selectAll(`.dot-${name}`)
      .data(props.data)
      .enter()
      .append('circle')
      .attr('cx', (d) => x(d.game.toString()) || 0) // Fallback na wypadek `undefined`
      .attr('cy', (d) => y(d[name]) || 0) // Fallback na wypadek `undefined`
      .attr('r', 5)
      .attr('fill', color(name))
  })

  // Oznaczenia osi
  svg
    .append('text')
    .attr('x', width / 2)
    .attr('y', height + 40)
    .attr('text-anchor', 'middle')
    .text('Numer gry')
    .style('fill', '#ffffff')
    .style('font-size', '14px')

  svg
    .append('text')
    .attr('text-anchor', 'middle')
    .attr('transform', `translate(-35, ${height / 2}) rotate(-90)`)
    .text('Odchylenie standardowe')
    .style('fill', '#ffffff')
    .style('font-size', '14px')

  // Legenda
  // --- KROK 7: Dodanie sygnatury indeksu, aby usunąć błąd `any` ---
  const labels: { [key: string]: string } = {
    positions: 'Pozycje',
    bits: 'Bity',
  }

  const legend = svg.append('g').attr('transform', `translate(${width - 120}, -30)`)

  metricNames.forEach((name, i) => {
    const yOffset = i * 20

    legend
      .append('rect')
      .attr('x', 0)
      .attr('y', yOffset)
      .attr('width', 12)
      .attr('height', 12)
      .attr('fill', color(name))

    legend
      .append('text')
      .attr('x', 18)
      .attr('y', yOffset + 10)
      .text(`${labels[name] || name}`) // Dostęp jest teraz bezpieczny
      .style('fill', '#ffffff')
      .style('font-size', '12px')
  })
}

onMounted(() => {
  // nextTick zapewnia, że element `ref` jest już w DOM
  nextTick(() => {
    drawChart()
  })
})

watch(
  () => props.data,
  () => {
    if (chart.value) {
      // Dodatkowe zabezpieczenie
      drawChart()
    }
  },
  { deep: true },
)
</script>
