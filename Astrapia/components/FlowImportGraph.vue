<template>
  <div id="flowImportContainer">
    <svg id="flowImport" ref="svgRef"></svg>
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref} from 'vue';
import {axisBottom, axisLeft, extent, isoParse, line, pointer, scaleLinear, scaleTime, select, timeHour,} from 'd3';
import networkAnalysisService, {FlowImport} from '~/services/networkAnalysisService';


let svgRef = ref(null);

onMounted(async() => {
  const flowImportGraphData = await networkAnalysisService.getFlowImport() as FlowImport[];

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
    },
    {
      "dateTime": "2024-01-02T17:40:59.5720466Z",
      "endpoints": {
        "127.0.0.1:60198": 60,
        "127.0.0.1:62541": 55,
        "127.0.0.1:60199": 19
      }
    },
    {
      "dateTime": "2024-01-02T17:41:59.5745162Z",
      "endpoints": {
        "127.0.0.1:60198": 46,
        "127.0.0.1:62541": 60,
        "127.0.0.1:60199": 36
      }
    },
    {
      "dateTime": "2024-01-02T17:42:59.581861Z",
      "endpoints": {
        "127.0.0.1:62541": 22,
        "127.0.0.1:60199": 60
      }
    },
    {
      "dateTime": "2024-01-02T17:43:59.5703493Z",
      "endpoints": {
        "127.0.0.1:62541": 45,
        "127.0.0.1:60199": 57
      }
    },
    {
      "dateTime": "2024-01-02T17:44:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 45,
        "127.0.0.1:60199": 57,
        "127.0.0.1:60198": 11
      }
    },
    {
      "dateTime": "2024-01-02T17:45:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 57,
        "127.0.0.1:60199": 61,
        "127.0.0.1:60198": 24
      }
    },
    {
      "dateTime": "2024-01-02T17:46:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 27,
        "127.0.0.1:60199": 34,
        "127.0.0.1:60198": 9
      }
    },
    {
      "dateTime": "2024-01-02T17:47:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 72,
        "127.0.0.1:60199": 56,
        "127.0.0.1:60198": 37
      }
    },
    {
      "dateTime": "2024-01-02T17:48:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 71,
        "127.0.0.1:60199": 57,
        "127.0.0.1:60198": 12
      }
    },
    {
      "dateTime": "2024-01-02T17:49:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 63,
        "127.0.0.1:60199": 64,
        "127.0.0.1:60198": 19
      }
    },
    {
      "dateTime": "2024-01-02T17:50:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 37,
        "127.0.0.1:60199": 63,
        "127.0.0.1:60198": 19
      }
    },
    {
      "dateTime": "2024-01-02T17:51:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 47,
        "127.0.0.1:60199": 65,
        "127.0.0.1:60198": 18
      }
    },
    {
      "dateTime": "2024-01-02T17:52:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 48,
        "127.0.0.1:60199": 46,
        "127.0.0.1:60198": 26
      }
    },
    {
      "dateTime": "2024-01-02T17:53:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 57,
        "127.0.0.1:60199": 61,
        "127.0.0.1:60198": 24
      }
    },
    {
      "dateTime": "2024-01-02T17:54:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 45,
        "127.0.0.1:60199": 57,
        "127.0.0.1:60198": 11
      }
    },
    {
      "dateTime": "2024-01-02T17:55:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 43,
        "127.0.0.1:60199": 33,
        "127.0.0.1:60198": 37
      }
    },
    {
      "dateTime": "2024-01-02T17:56:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 56,
        "127.0.0.1:60199": 33,
        "127.0.0.1:60198": 43
      }
    },
    {
      "dateTime": "2024-01-02T17:57:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 32,
        "127.0.0.1:60199": 38,
        "127.0.0.1:60198": 53
      }
    },
    {
      "dateTime": "2024-01-02T17:58:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 29,
        "127.0.0.1:60199": 29,
        "127.0.0.1:60198": 51
      }
    },
    {
      "dateTime": "2024-01-02T17:59:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 30,
        "127.0.0.1:60199": 30,
        "127.0.0.1:60198": 49
      }
    },
    {
      "dateTime": "2024-01-02T18:00:59.5720466Z",
      "endpoints": {
        "127.0.0.1:62541": 31,
        "127.0.0.1:60199": 27,
        "127.0.0.1:60198": 44
      }
    },
    {
      "dateTime": "2024-01-02T18:01:59.5745162Z",
      "endpoints": {
        "127.0.0.1:62541": 33,
        "127.0.0.1:60199": 42,
        "127.0.0.1:60198": 41
      }
    },
  ];

  const width = 1600;
  const height = 600;
  const marginTop = 20;
  const marginRight = 150;
  const marginBottom = 20;
  const marginLeft = 20;

  let tooltip = select('#flowImportContainer')
    .append('div')
    .style('position', 'absolute')
    .style('visibility', 'hidden')
    .style('background-color', '#D7DFE7')
    .style('color', 'black')
    .style('padding', '0.8vh')
    .style('border-radius', '4px')
    .style('border', '0.1vh solid #424242')
    .style('font-family', 'Open Sans');

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

  // const data = testData.flatMap(item => {
  //   return Object.entries(item.endpoints).map(([endpoint, value]) => ({
  //     dateTime: isoParse(item.dateTime),
  //     endpoint,
  //     value
  //   }));
  // });

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
    .call(axisBottom(x)
      .ticks(timeHour.every(2)));

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
    .attr('stroke', '#e0e0e0')
    .attr('stroke-width', .5);

  svg.selectAll('yGrid')
    .data(y.ticks((maxValue + 10) / 10))
    .join('line')
    .attr('x1', 0)
    .attr('x2', width - marginRight)
    .attr('y1', d => y(d))
    .attr('y2', d => y(d))
    .attr('stroke', '#e0e0e0')
    .attr('stroke-width', .5)

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
    svg.append('circle').attr('cx',width-marginRight + 20).attr('cy',20 + i * 25).attr('r', 10).style('fill', colors[i] ?? 'black')
    svg.append('text').attr('x', width-marginRight + 40).attr('y', 20 + i * 25).text(endpoint!).style('font-size', '1.5vh').attr('alignment-baseline','middle')

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
            .html(`Date: ${x.invert(mousePos[0]).toString().slice(0, 24)}<br>Packets: ${Math.round(y.invert(mousePos[1]))}`);
        }
      })
      .on('mouseout', function() { tooltip.style('visibility', 'hidden'); });
  });

  svg.append('text')
    .attr('class', 'chart-title')
    .attr('x', marginLeft + 20)
    .attr('y', marginTop)
    .style('font-size', '3vh')
    .style('font-weight', 'bold')
    .style('font-family', 'sans-serif')
    .text('Flow Import');
});
</script>

<style scoped>
#flowImportContainer {
  display: flex;
  flex-direction: column;
  margin: 5vh 0 0 5vw;
}
</style>
