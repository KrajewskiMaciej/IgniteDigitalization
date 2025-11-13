<template>
  <div ref="chart" class="w-full h-96"></div>
</template>

<script setup lang="ts">
import * as d3 from 'd3'
import { ref, onMounted, watch, nextTick } from 'vue'
import type { PropType } from 'vue'

// --- KROK 1: Definicja interfejsu dla danych wejściowych ---
// Pozwala na elastyczne dane, np. { success: 12, failure: 8, pending: 2 }
interface DecisionData {
  [key: string]: number
}

const props = defineProps({
  // --- KROK 2: Użycie PropType do silnego typowania propsów ---
  data: {
    type: Object as PropType<DecisionData>,
    required: true,
  },
})

// --- KROK 3: Jawne otypowanie referencji do elementu DOM ---
const chart = ref<HTMLElement | null>(null)

const drawChart = () => {
  // --- KROK 4: Zabezpieczenie przed nullem i pustymi danymi ---
  if (!chart.value || !props.data || Object.keys(props.data).length === 0) {
    return
  }

  const width = chart.value.clientWidth
  const height = chart.value.clientHeight
  const radius = Math.min(width, height) / 2 - 40

  // Czyszczenie poprzedniego wykresu
  d3.select(chart.value).selectAll('*').remove()

  const svg = d3
    .select(chart.value)
    .append('svg')
    .attr('width', width)
    .attr('height', height)
    .append('g')
    .attr('transform', `translate(${width / 2},${height / 2})`)

  const dataEntries = Object.entries(props.data)
  const dataKeys = Object.keys(props.data)

  const color = d3
    .scaleOrdinal<string>() // Określenie, że domena i wynik to string
    .domain(dataKeys)
    .range(['#4ade80', '#f87171', '#fbbf24', '#60a5fa']) // zielony, czerwony, żółty, niebieski

  // --- KROK 5: Użycie generyków dla d3.pie ---
  // Informujemy d3.pie, że pracuje na tablicy krotek [string, number]
  const pie = d3
    .pie<[string, number]>()
    .value((d) => d[1]) // Teraz d[1] jest poprawnie rozpoznawane jako number
    .sort(null) // Wyłączenie sortowania, aby zachować kolejność z obiektu

  const data_ready = pie(dataEntries)

  // --- KROK 6: Użycie generyków dla d3.arc ---
  // Generator łuków teraz wie, że otrzyma dane typu PieArcDatum<[string, number]>
  const arcGenerator = d3
    .arc<d3.PieArcDatum<[string, number]>>()
    .innerRadius(radius * 0.5) // Zmieniono na wykres pierścieniowy (doughnut) dla estetyki
    .outerRadius(radius)

  // Rysowanie wycinków
  svg
    .selectAll('path')
    .data(data_ready)
    .enter()
    .append('path')
    // --- KROK 7: Poprawne wywołanie generatora łuków ---
    .attr('d', arcGenerator) // Generator musi być wywołany dla każdego punktu danych
    .attr('fill', (d) => color(d.data[0])) // d.data[0] jest teraz bezpiecznie rozpoznawane jako string (klucz)
    .attr('stroke', '#1e293b')
    .style('stroke-width', '4px')
    .style('stroke-linejoin', 'round')

  // Legenda
  // --- KROK 8: Dodanie sygnatury indeksu dla obiektu etykiet ---
  const labels: { [key: string]: string } = {
    success: 'Sukcesy',
    failure: 'Porażki',
  }

  const legend = svg
    .append('g')
    .attr('transform', `translate(${radius + 30}, ${-radius + 10})`)
    .attr('text-anchor', 'start')

  dataEntries.forEach(([key, value], i) => {
    const yOffset = i * 25

    legend
      .append('rect')
      .attr('y', yOffset - 12)
      .attr('width', 12)
      .attr('height', 12)
      .attr('fill', color(key)) // Bezpieczny dostęp do koloru

    legend
      .append('text')
      .attr('x', 18)
      .attr('y', yOffset)
      .text(`${labels[key] || key}: ${value}`) // Bezpieczny dostęp do etykiety
      .style('fill', '#ffffff')
      .style('font-size', '14px')
  })
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
