<template>
  <div id="flowImportContainer">
    <label id="flowImportLabel" for="flowImport">Flow Import</label>
    <svg id="flowImport" ref="svgRef"></svg>
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
  isoParse,
  timeMinute,
} from 'd3';

import networkAnalysisService, {FlowImport} from '~/services/networkAnalysisService';
import _ from "lodash";

let svgRef = ref(null);

onMounted(async() => {
  const flowImportGraphData = await networkAnalysisService.getFlowImport() as FlowImport[];

  console.log(flowImportGraphData)

  const testData = [
    {
      "dateTime": "2024-01-02T17:36:59.5720466Z",
      "endpoints": {
        "127.0.0.1:60198": 60,
        "127.0.0.1:62541": 55,
        "127.0.0.1:60199": 19
      }
    },
    {
      "dateTime": "2024-01-02T17:37:59.5745162Z",
      "endpoints": {
        "127.0.0.1:60198": 46,
        "127.0.0.1:62541": 60,
        "127.0.0.1:60199": 36
      }
    },
    {
      "dateTime": "2024-01-02T17:38:59.581861Z",
      "endpoints": {
        "127.0.0.1:62541": 22,
        "127.0.0.1:60199": 60
      }
    },
    {
      "dateTime": "2024-01-02T17:39:59.5703493Z",
      "endpoints": {
        "127.0.0.1:62541": 45,
        "127.0.0.1:60199": 57
      }
    }
  ];

  const width = 900;
  const height = 600;
  const marginTop = 20;
  const marginRight = 150;
  const marginBottom = 20;
  const marginLeft = 20;

  // const data = flowImportGraphData.flatMap(item => {
  //   return Object.entries(item.endpoints).map(([endpoint, value]) => ({
  //     dateTime: isoParse(item.dateTime),
  //     endpoint,
  //     value
  //   }));
  // });

  const data = testData.flatMap(item => {
    return Object.entries(item.endpoints).map(([endpoint, value]) => ({
      dateTime: isoParse(item.dateTime),
      endpoint,
      value
    }));
  });

  console.log(data)

  const dates = data.map(d => d.dateTime);
  const x = scaleTime()
    .domain(extent(dates))
    .range([marginLeft, width - marginRight])

  const maxValue = Math.max(...data.map(d => d.value));
  const y = scaleLinear()
    .domain([0, maxValue + 20])
    .range([height - marginBottom, marginTop]);

  const svg = select(svgRef.value)
    .attr("width", width)
    .attr("height", height);

  svg.append("g")
    .attr("transform", `translate(0,${height - marginBottom})`)
    .call(axisBottom(x)
      .ticks(timeMinute.every(1)));

  svg.append("g")
    .attr("transform", `translate(${marginLeft},0)`)
    .call(axisLeft(y));

  const lineGenerator = line()
    .x(d => x(d.dateTime))
    .y(d => y(d.value));

  const endpoints = Array.from(new Set(data.map(d => d.endpoint)));
  const colors = ['#795387', '#875F53', '#618753']
  _.zip(endpoints, colors).forEach(([endpoint, color], i) => {
    svg.append('circle').attr('cx',width-marginRight + 20).attr('cy',20 + i * 25).attr('r', 10).style('fill', color ?? 'black')
    svg.append('text').attr('x', width-marginRight + 40).attr('y', 20 + i * 25).text(endpoint!).style('font-size', '15px').attr('alignment-baseline','middle')

    const filteredData = data.filter(d => d.endpoint === endpoint);
    svg.append('path')
      .datum(filteredData)
      .attr('fill', 'none')
      .attr('stroke', color ?? 'black')
      .attr('stroke-width', 3.5)
      .attr('d', lineGenerator);
  });
});
</script>

<style scoped>
#flowImportContainer {
  display: flex;
  flex-direction: column;
  margin: 5vh 0 0 5vw;
}

#flowImportLabel {
  font-weight: bold;
  font-family: 'Open Sans', sans-serif;
  font-size: 3vh;
}
</style>
