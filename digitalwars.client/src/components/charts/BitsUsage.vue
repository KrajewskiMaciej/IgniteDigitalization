<template>
    <div ref="chart" class="w-full h-96"></div>
  </template>
  
<script setup lang="ts">
  import * as d3 from 'd3'
  import { ref, onMounted, watch, nextTick } from 'vue'
  import type { PropType } from 'vue';

  // --- KROK 1: Definicja interfejsu dla danych wejściowych ---
  interface BitsDataPoint {
    round: number;
    bits: number;
  }

  const props = defineProps({
    // --- KROK 2: Użycie PropType do silnego typowania propsów ---
    data: {
      type: Array as PropType<BitsDataPoint[]>,
      required: true
    }
  })

  // --- KROK 3: Jawne otypowanie referencji do elementu DOM ---
  const chart = ref<HTMLElement | null>(null)

  const drawChart = () => {
    // --- KROK 4: Zabezpieczenie przed nullem i pustymi danymi ---
    if (!chart.value || !props.data || props.data.length === 0) {
      return;
    }

    const margin = { top: 30, right: 30, bottom: 50, left: 50 }
    const width = chart.value.clientWidth - margin.left - margin.right
    const height = chart.value.clientHeight - margin.top - margin.bottom

    // Czyszczenie poprzedniego wykresu
    d3.select(chart.value).selectAll('*').remove()

    const svg = d3
      .select(chart.value)
      .append('svg')
      .attr('width', width + margin.left + margin.right)
      .attr('height', height + margin.top + margin.bottom + 20)
      .append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`)

    // Skale X i Y są teraz tworzone na podstawie silnie otypowanych danych
    const x = d3
      .scalePoint()
      .domain(props.data.map((d) => d.round.toString())) // scalePoint wymaga stringów w domenie
      .range([0, width])
      .padding(0.5)

    const yMax = d3.max(props.data, (d) => d.bits) ?? 0; // Bezpieczne obliczenie max
    const y = d3
      .scaleLinear()
      .domain([0, yMax * 1.1]) // Dodanie 10% marginesu na górze
      .range([height, 0])
      .nice();

    // Rysowanie osi
    svg
      .append('g')
      .attr('transform', `translate(0,${height})`)
      .call(d3.axisBottom(x))
      .selectAll('text')
      .style('fill', '#ffffff');

    svg
      .append('g')
      .call(d3.axisLeft(y))
      .selectAll('text')
      .style('fill', '#ffffff');

    // Stylowanie osi i siatki
    svg.selectAll('.domain').style('stroke', '#ffffff');
    svg.selectAll('g.tick line').style('stroke', 'rgba(255, 255, 255, 0.2)');

    // --- KROK 5: Użycie generycznego typu dla d3.line ---
    const line = d3
      .line<BitsDataPoint>() // Informuje D3 o strukturze danych
      .x((d) => x(d.round.toString()) || 0) // Dodanie fallbacku || 0
      .y((d) => y(d.bits) || 0) // Dodanie fallbacku || 0
      .curve(d3.curveMonotoneX); // Wygładzenie linii

    // Rysowanie ścieżki (linii)
    svg
      .append('path')
      .datum(props.data)
      .attr('fill', 'none')
      .attr('stroke', '#38bdf8')
      .attr('stroke-width', 3)
      // --- KROK 6: Poprawne wywołanie generatora linii ---
      .attr('d', line);

    // Rysowanie punktów na linii
    svg
      .selectAll('circle')
      .data(props.data)
      .enter()
      .append('circle')
      .attr('cx', (d) => x(d.round.toString()) || 0)
      .attr('cy', (d) => y(d.bits) || 0)
      .attr('r', 5)
      .attr('fill', '#38bdf8');

    // Etykiety osi
    svg
      .append('text')
      .attr('x', width / 2)
      .attr('y', height + 45)
      .attr('text-anchor', 'middle')
      .text('Runda')
      .style('fill', '#ffffff')
      .style('font-size', '14px');

    svg
      .append('text')
      .attr('text-anchor', 'middle')
      .attr('transform', `translate(-35, ${height / 2}) rotate(-90)`)
      .text('Zużyte Bity')
      .style('fill', '#ffffff')
      .style('font-size', '14px');
  }

  onMounted(() => {
    nextTick(() => {
      drawChart()
    })
  })

  watch(
    () => props.data,
    () => {
      if(chart.value) drawChart() // Dodatkowe zabezpieczenie
    },
    { deep: true }
  )
</script>