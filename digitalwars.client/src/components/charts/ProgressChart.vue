<template>
  <div ref="chart" class="w-full h-96"></div>
</template>

<script setup lang="ts">
import * as d3 from 'd3'
import { ref, onMounted, watch, nextTick } from 'vue'
import type { PropType } from 'vue'

// --- KROK 1: Definicja interfejsu dla danych wejściowych ---
// Rozwiązuje to większość błędów typu 'unknown' i 'any'.
interface ProgressDataPoint {
  round: number
  [key: string]: number // Sygnatura indeksu pozwala na dynamiczne nazwy graczy
}

const props = defineProps({
  // --- KROK 2: Użycie PropType do silnego typowania propsów ---
  data: {
    type: Array as PropType<ProgressDataPoint[]>,
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

  const margin = { top: 30, right: 100, bottom: 50, left: 50 }
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

  // Nazwy graczy są teraz bezpiecznie wyodrębniane z otypowanych danych
  const players = Object.keys(props.data[0]).filter((key) => key !== 'round')
  const rounds = props.data.map((d) => d.round)

  // Definicja skal
  const x = d3
    .scaleLinear()
    .domain(d3.extent(rounds) as [number, number]) // d3.extent zwraca [min, max]
    .range([0, width])

  const yMax = d3.max(props.data.flatMap((d) => players.map((p) => d[p]))) ?? 20
  const y = d3.scaleLinear().domain([0, yMax]).range([height, 0])

  // --- KROK 5: Poprawne zdefiniowanie skali kolorów ---
  const color = d3.scaleOrdinal(d3.schemeTableau10).domain(players)

  // Rysowanie osi
  svg
    .append('g')
    .attr('transform', `translate(0,${height})`)
    .call(
      d3
        .axisBottom(x)
        .ticks(rounds.length) // Użyj liczby rund jako sugestii dla liczby ticków
        .tickFormat(d3.format('d')),
    )
    .selectAll('text')
    .style('fill', '#ffffff')

  svg.append('g').call(d3.axisLeft(y)).selectAll('text').style('fill', '#ffffff')

  // Stylowanie osi
  svg.selectAll('.domain').style('stroke', '#ffffff')
  svg.selectAll('line').style('stroke', 'rgba(255, 255, 255, 0.2)')

  // Rysowanie linii dla każdego gracza
  players.forEach((player) => {
    // --- KROK 6: Użycie generycznego typu dla d3.line ---
    const line = d3
      .line<ProgressDataPoint>()
      .x((d) => x(d.round) || 0) // Fallback || 0 na wypadek undefined
      .y((d) => y(d[player]) || 0) // Dostęp przez `d[player]` jest teraz bezpieczny

    // --- KROK 7: Poprawne wywołanie generatora linii ---
    svg
      .append('path')
      .datum(props.data)
      .attr('fill', 'none')
      .attr('stroke', color(player)) // Użycie nazwy gracza zamiast indeksu
      .attr('stroke-width', 2)
      .attr('d', line(props.data)) // Generator linii musi być wywołany z danymi

    // Rysowanie punktów na linii
    svg
      .selectAll(`.circle-${player}`)
      .data(props.data)
      .enter()
      .append('circle')
      .attr('cx', (d) => x(d.round) || 0)
      .attr('cy', (d) => y(d[player]) || 0)
      .attr('r', 4)
      .attr('fill', color(player))
  })

  // Opisy osi
  svg
    .append('text')
    .attr('x', width / 2)
    .attr('y', height + 40)
    .attr('text-anchor', 'middle')
    .text('Runda')
    .style('fill', '#ffffff')

  svg
    .append('text')
    .attr('text-anchor', 'middle')
    .attr('transform', `translate(-35,${height / 2}) rotate(-90)`)
    .text('Postęp')
    .style('fill', '#ffffff')

  // Legenda
  const legend = svg
    .selectAll('.legend')
    .data(players)
    .enter()
    .append('g')
    .attr('class', 'legend')
    .attr('transform', (d, i) => `translate(${width + 10}, ${i * 20})`)

  legend
    .append('rect')
    .attr('width', 12)
    .attr('height', 12)
    .attr('fill', (d) => color(d)) // `d` to nazwa gracza

  legend
    .append('text')
    .attr('x', 20)
    .attr('y', 9)
    .style('text-anchor', 'start')
    .style('fill', '#ffffff')
    .style('font-size', '12px')
    .text((d) => d) // `d` to nazwa gracza
}

onMounted(() => {
  nextTick(() => {
    drawChart()
  })
})

watch(
  () => props.data,
  () => {
    if (chart.value) drawChart()
  },
  { deep: true },
)
</script>
