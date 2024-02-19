<template>
  <div id="flowImportContainer">
    <svg id="flowImport" ref="svgRef"></svg>
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref} from 'vue';
import {axisBottom, axisLeft, extent, isoParse, line, pointer, scaleLinear, scaleTime, select,} from 'd3';
import networkAnalysisService from '~/services/networkAnalysisService';


let svgRef = ref(null);

onMounted(async() => {
  const flowImportGraphData = await networkAnalysisService.getFlowImport() as FlowImport[];

  const width = 1600;
  const height = 600;
  const marginTop = 20;
  const marginRight = 170;
  const marginBottom = 20;
  const marginLeft = 20;

  let tooltip = select('#flowImportContainer')
    .append('div')
    .classed('tooltip', true);

  // creating a flat array of objects with endpoint, dateTime, and value
  // date would be converted to local datetime by date constructor, so it is converted back to utc with timezone offset
  const data = flowImportGraphData.flatMap(item => {
    const dateTime = isoParse(item.dateTime);
    const offset = dateTime!.getTimezoneOffset();
    const utcDateTime = new Date(dateTime!.getTime() + offset * 60000);
    return Object.entries(item.endpoints).map(([endpoint, value]) => ({
      dateTime: utcDateTime,
      endpoint,
      value
    }));
  });

  const dates = data.map(d => d.dateTime);
  const x = scaleTime()
    .domain(extent(dates))
    .range([marginLeft, width - marginRight])

  const maxValue = Math.max(...data.map(d => d.value));
  const y = scaleLinear()
    .domain([0, maxValue + 10])
    .range([height - marginBottom, marginTop]);

  const svg = select(svgRef.value)
    .attr('width', width)
    .attr('height', height);

  svg.append('g')
    .attr('transform', `translate(0,${height - marginBottom})`)
    .call(axisBottom(x));

  svg.append('g')
    .attr('transform', `translate(${marginLeft},0)`)
    .call(axisLeft(y));

  svg.selectAll('xGrid')
    .data(x.ticks())
    .join('line')
    .attr('x1', d => x(d))
    .attr('x2', d => x(d))
    .attr('y1', 0)
    .attr('y2', height)

  svg.selectAll('yGrid')
    .data(y.ticks((maxValue + 10) / 10))
    .join('line')
    .attr('x1', 0)
    .attr('x2', width - marginRight)
    .attr('y1', d => y(d))
    .attr('y2', d => y(d))

  const lineGenerator = line()
    .x(d => x(d.dateTime))
    .y(d => y(d.value));

  const endpoints = Array.from(new Set(data.map(d => d.endpoint)));
  const colors = ['#7EA0A9', '#C78750', '#EB5050', '#294D61']

  const timestamps = Array.from(new Set(data.map(d => d.dateTime)));
  const processedData = endpoints.map(endpoint => {
    const existingValues = data.filter(d => d.endpoint === endpoint);
    return timestamps.map(timestamp => {
      const existingValue = existingValues.find(d => d.dateTime.getTime() === timestamp.getTime());
      return existingValue || {dateTime: timestamp, endpoint, value: 0};
    });
  });

  endpoints.forEach((endpoint, i) => {
    svg.append('circle')
      .attr('cx',width-marginRight + 20)
      .attr('cy',20 + i * 25)
      .attr('r', 10)
      .style('fill', colors[i] ?? 'black')
    svg.append('text')
      .attr('x', width-marginRight + 40)
      .attr('y', 20 + i * 25)
      .text(endpoint!)
      .style('font-size', '1.5vh')
      .style('font-family', 'Open Sans')
      .attr('alignment-baseline','middle')

    const flattenedData = processedData.flatMap(d => d);
    const filteredData = flattenedData.filter(d => d.endpoint === endpoint);
    svg.append('path')
      .datum(filteredData)
      .attr('class', `line-${endpoint}`)
      .attr('fill', 'none')
      .attr('stroke', colors[i] ?? 'black')
      .attr('stroke-width', 3.5)
      .attr('d', lineGenerator)
      .on('mouseover', function() { tooltip.style('visibility', 'visible'); })
      .on('mousemove', function(event) {
        if (event) {
          let mousePos = pointer(event);
          tooltip.style('top', (event.pageY - 10) + 'px')
            .style('left', (event.pageX + 10) + 'px')
            .html(`Endpoint: ${endpoint}<br>Date: ${x.invert(mousePos[0]).toString().slice(0, 24)}<br>Packets: ${Math.round(y.invert(mousePos[1]))}`);
        }
      })
      .on('mouseout', function() { tooltip.style('visibility', 'hidden'); });
  });

  svg.append('text')
    .attr('class', 'chart-title')
    .attr('x', marginLeft + 20)
    .attr('y', marginTop)
    .classed('chart-title', true)
    .text('Flow Import');
});
export interface FlowImport {
  dateTime: string;
  endpoints: { [key: string]: number };
}
</script>

<style scoped>
#flowImportContainer {
  display: flex;
  flex-direction: column;
  margin: 5vh 0 0 5vw;
}

.xGrid, .yGrid {
  stroke: #e0e0e0;
  stroke-width: 0.5;
}
#flowImportContainer:deep(.tooltip) {
  position: absolute;
  visibility: hidden;
  background-color: #D7DFE7;
  color: black;
  padding: 0.8vh;
  border-radius: 4px;
  border: 0.1vh solid #424242;
  font-family: "Open Sans", sans-serif;
}

#flowImportContainer:deep(.chart-title) {
  font-size: 3vh;
  font-weight: bold;
  font-family: "Open Sans", sans-serif;
}
</style>
