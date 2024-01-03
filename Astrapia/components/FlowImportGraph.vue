<template>
  <div>
    <svg ref="svgRef"></svg>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import {
  select,
  scaleTime,
  scaleLinear,
  axisBottom,
  axisLeft,
  line,
  extent,
  isoParse
} from 'd3';

let svgRef = ref(null);

const testData = [
  {
    "dateTime": "2024-01-02T17:36:59.5720466Z",
    "endpoints": {
      "127.0.0.1:60198": 60,
      "127.0.0.1:62541": 60,
      "127.0.0.1:60199": 60
    }
  },
  {
    "dateTime": "2024-01-02T17:37:59.5745162Z",
    "endpoints": {
      "127.0.0.1:60198": 46,
      "127.0.0.1:62541": 60,
      "127.0.0.1:60199": 60
    }
  },
  {
    "dateTime": "2024-01-02T17:38:59.581861Z",
    "endpoints": {
      "127.0.0.1:62541": 60,
      "127.0.0.1:60199": 60
    }
  },
  {
    "dateTime": "2024-01-02T17:39:59.5703493Z",
    "endpoints": {
      "127.0.0.1:62541": 60,
      "127.0.0.1:60199": 60
    }
  }
];

onMounted(() => {
  const width = 900;
  const height = 600;
  const marginTop = 20;
  const marginRight = 20;
  const marginBottom = 30;
  const marginLeft = 40;

  const data = testData.flatMap(item => {
    return Object.entries(item.endpoints).map(([endpoint, value]) => ({
      dateTime: isoParse(item.dateTime),
      endpoint,
      value
    }));
  });

  const dates = data.map(d => d.dateTime);
  const x = scaleTime()
    .domain(extent(dates))
    .range([marginLeft, width - marginRight]);

  const maxValue = Math.max(...data.map(d => d.value));
  const y = scaleLinear()
    .domain([0, maxValue + 20])
    .range([height - marginBottom, marginTop]);

  const svg = select(svgRef.value)
    .attr("width", width)
    .attr("height", height);

  svg.append("g")
    .attr("transform", `translate(0,${height - marginBottom})`)
    .call(axisBottom(x));

  svg.append("g")
    .attr("transform", `translate(${marginLeft},0)`)
    .call(axisLeft(y));

  const lineGenerator = line()
    .x(d => x(d.dateTime))
    .y(d => y(d.value));

  const endpoints = Array.from(new Set(data.map(d => d.endpoint)));
  endpoints.forEach((endpoint) => {
    const filteredData = data.filter(d => d.endpoint === endpoint);
    svg.append('path')
      .datum(filteredData)
      .attr('fill', 'none')
      .attr('stroke', 'steelblue')
      .attr('stroke-width', 1.5)
      .attr('d', lineGenerator);
  });
});
</script>

<style scoped>

</style>
