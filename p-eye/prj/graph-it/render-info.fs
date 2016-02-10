﻿module GraphIt.RenderInfo
let dataDiv = 
  """
  <div id="$id" class="histo">
  $data
  </div>
  """

let drawInitialFun = 
  """
      function drawInitialChart() {
          // Connect the choose files button:
          document.getElementById('files').addEventListener('change', handleFileSelect, false);

          // Load some static example data:
          //var data1Str = document.querySelector("div#data_1").innerHTML.trim();
          //var data2Str = document.querySelector("div#data_2").innerHTML.trim();
          //var data3Str = document.querySelector("div#data_3").innerHTML.trim();
		  $selectors
          //var histos = [data3Str, data2Str, data1Str];
		  var histos = $histos
          //var names = ['A', 'B', 'C'];
		  var names = $names

          setChartData(names, histos);
          drawChart();
      }

  """

let chart = 
  """
  <html>
  <head>
  <meta charset="UTF-8">
  <style type="text/css">
      div.histo {
          visibility: hidden
      }
  </style>

  <!--Load the AJAX API-->
  <script type="text/javascript" src="https://www.google.com/jsapi"></script>

  <script type="text/javascript">

      if (window.File && window.FileReader && window.FileList && window.Blob) {
          // Great success! All the File APIs are supported.
      } else {
          alert('The File APIs are not fully supported in this browser.');
      }

      // Load the Visualization API and the piechart package.
      google.load('visualization', '1.0', {'packages':['corechart']});

      // Set a callback to run when the Google Visualization API is loaded.
      google.setOnLoadCallback(drawInitialChart);

      var chartData = null;
      var chart = null;

      function setChartData(names, histos) {
          while (names.length < histos.length) {
              names.push('Unknown');
          }

          var series = [];
          for (var i = 0; i < histos.length; i++) {
              series = appendDataSeries(histos[i], names[i], series);
          }

          chartData = google.visualization.arrayToDataTable(series);
      }


  $draw-initial

      var maxPercentile = 1000000;

      function drawChart() {

          var ticks =
                  [{v:1,f:'0%'},
                      {v:10,f:'90%'},
                      {v:100,f:'99%'},
                      {v:1000,f:'99.9%'},
                      {v:10000,f:'99.99%'},
                      {v:100000,f:'99.999%'},
                      {v:1000000,f:'99.9999%'},
                      {v:10000000,f:'99.99999%'},
                      {v:100000000,f:'99.999999%'}];

          var unitSelection = document.getElementById("timeUnitSelection");
          var unitSelIndex = unitSelection.selectedIndex;
          var unitText = unitSelection.options[unitSelIndex].innerHTML;

          var options = {
              title: 'Latency by Percentile Distribution',
              height: 480,
  //            hAxis: {title: 'Percentile', minValue: 0, logScale: true, ticks:ticks },
              hAxis: {
                  title: "Percentile",
                  minValue: 1, logScale: true, ticks:ticks,
                  viewWindowMode:'explicit',
                  viewWindow:{
                      max:maxPercentile,
                      min:1
                  }
              },
              vAxis: {title: 'Latency (' + unitText + ')', minValue: 0 },
              legend: {position: 'bottom'}
          };

          if (chart == null) {
              chart = new google.visualization.LineChart(document.getElementById('chart_div'));
          }

          // add tooptips with correct percentile text to data:
          var columns = [0];
          for (var i = 1; i < chartData.getNumberOfColumns(); i++) {
              columns.push(i);
              columns.push({
                  type: 'string',
                  properties: {
                      role: 'tooltip'
                  },
                  calc: (function (j) {
                      return function (dt, row) {
                          var percentile = 100.0 - (100.0/dt.getValue(row, 0));
                          return dt.getColumnLabel(j) + ': ' +
                                  percentile.toPrecision(7) +
                                  '\%\'ile = ' + dt.getValue(row, j) + ' ' + unitText
                      }
                  })(i)
              });
          }
          var view = new google.visualization.DataView(chartData);
          view.setColumns(columns);

          chart.draw(view, options);

      }
  </script>
  <script type="text/javascript">
      function appendDataSeries(histo, name, dataSeries) {
          var series;
          var seriesCount;
          if (dataSeries.length == 0) {
              series = [ ['X', name] ];
              seriesCount = 1;
          } else {
              series = dataSeries;
              series[0].push(name);
              seriesCount = series[0].length - 1;
          }

          var lines = histo.split("\n");

          var seriesIndex = 1;
          for (var i = 0; i < lines.length; i++) {
              var line = lines[i].trim();
              var values = line.trim().split(/[ ]+/);

              if (line[0] != '#' && values.length == 4) {

                  var y = parseFloat(values[0]);
                  var x = parseFloat(values[3]);

                  if (!isNaN(x) && !isNaN(y)) {

                      if (seriesIndex >= series.length) {
                          series.push([x]);
                      }

                      while (series[seriesIndex].length < seriesCount) {
                          series[seriesIndex].push(null);
                      }

                      series[seriesIndex].push(y);
                      seriesIndex++;
                  }
              }
          }

          while (seriesIndex < series.length) {
              series[seriesIndex].push(null);
              seriesIndex++;
          }

          return series;
      }
  </script>
  <script>
      function timeUnitsSelected(evt) {
          drawChart();
      }

      function doExport(event) {
          saveSvgAsPng(document.querySelector('svg'), 'Histogram', 2.0);
      }
  </script>

  <script>
      function handleFileSelect(evt) {
          var files = evt.target.files; // FileList object
          var fileDisplayArea = document.getElementById('fileDisplayArea');

          var names = [];
          var histos = [];
          var fileCount = 0;

          fileDisplayArea.innerText = "file selected...\n";

          // Loop through the FileList and render image files as thumbnails.
          for (var i = 0, f; f = files[i]; i++) {
              var reader = new FileReader();

              reader.onload = (function(theFile) {
                  return function(e) {
                      histos.push(e.target.result);
                      names.push(escape(theFile.name));
                      fileDisplayArea.innerText = " Plotting input from: " + names + "\n";
                      setChartData(names, histos);
                      drawChart();
                  };
              })(f);

              // Read in the image file as a data URL.
              reader.readAsText(f);
          }

      }

  </script>

  <script type="text/javascript">
      (function() {
          var out$ = typeof exports != 'undefined' && exports || this;

          var doctype = '<?xml version="1.0" standalone="no"?><!DOCTYPE svg PUBLIC "-//W3C//DTD SVG 1.1//EN" "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd">';

          function inlineImages(callback) {
              var images = document.querySelectorAll('svg image');
              var left = images.length;
              if (left == 0) {
                  callback();
              }
              for (var i = 0; i < images.length; i++) {
                  (function(image) {
                      if (image.getAttribute('xlink:href')) {
                          var href = image.getAttribute('xlink:href').value;
                          if (/^http/.test(href) && !(new RegExp('^' + window.location.host).test(href))) {
                              throw new Error("Cannot render embedded images linking to external hosts.");
                          }
                      }
                      var canvas = document.createElement('canvas');
                      var ctx = canvas.getContext('2d');
                      var img = new Image();
                      img.src = image.getAttribute('xlink:href');
                      img.onload = function() {
                          canvas.width = img.width;
                          canvas.height = img.height;
                          ctx.drawImage(img, 0, 0);
                          image.setAttribute('xlink:href', canvas.toDataURL('image/png'));
                          left--;
                          if (left == 0) {
                              callback();
                          }
                      }
                  })(images[i]);
              }
          }

          function styles(dom) {
              var css = "";
              var sheets = document.styleSheets;
              for (var i = 0; i < sheets.length; i++) {
                  var rules = sheets[i].cssRules;
                  if (rules != null) {
                      for (var j = 0; j < rules.length; j++) {
                          var rule = rules[j];
                          if (typeof(rule.style) != "undefined") {
                              css += rule.selectorText + " { " + rule.style.cssText + " }\n";
                          }
                      }
                  }
              }

              var s = document.createElement('style');
              s.setAttribute('type', 'text/css');
              s.innerHTML = "<![CDATA[\n" + css + "\n]]>";

              var defs = document.createElement('defs');
              defs.appendChild(s);
              return defs;
          }

          out$.svgAsDataUri = function(el, scaleFactor, cb) {
              scaleFactor = scaleFactor || 1;

              inlineImages(function() {
                  var outer = document.createElement("div");
                  var clone = el.cloneNode(true);
                  var width = parseInt(
                          clone.getAttribute('width')
                          || clone.style.width
                          || out$.getComputedStyle(el).getPropertyValue('width')
                  );
                  var height = parseInt(
                          clone.getAttribute('height')
                          || clone.style.height
                          || out$.getComputedStyle(el).getPropertyValue('height')
                  );

                  var xmlns = "http://www.w3.org/2000/xmlns/";

                  clone.setAttribute("version", "1.1");
                  clone.setAttributeNS(xmlns, "xmlns", "http://www.w3.org/2000/svg");
                  clone.setAttributeNS(xmlns, "xmlns:xlink", "http://www.w3.org/1999/xlink");
                  clone.setAttribute("width", width * scaleFactor);
                  clone.setAttribute("height", height * scaleFactor);
                  clone.setAttribute("viewBox", "0 0 " + width + " " + height);
                  outer.appendChild(clone);

                  clone.insertBefore(styles(clone), clone.firstChild);

                  var svg = doctype + outer.innerHTML;
                  var uri = 'data:image/svg+xml;base64,' + window.btoa(unescape(encodeURIComponent(svg)));
                  if (cb) {
                      cb(uri);
                  }
              });
          }

          out$.saveSvgAsPng = function(el, name, scaleFactor) {
              out$.svgAsDataUri(el, scaleFactor, function(uri) {
                  var image = new Image();
                  image.src = uri;
                  image.onload = function() {
                      var canvas = document.createElement('canvas');
                      canvas.width = image.width;
                      canvas.height = image.height;
                      var context = canvas.getContext('2d');
                      context.drawImage(image, 0, 0);

                      var a = document.createElement('a');
                      a.download = name;
                      a.href = canvas.toDataURL('image/png');
                      document.body.appendChild(a);
                      a.click();
                  }
              });
          }
      })();
  </script>

  <style>
      .slider-width500
      {
          width: 500px;
      }
  </style>

  </head>

  <body>
  <h2>HdrHistogram Plotter</h2>

  <input type="file" id="files" name="files[]" multiple />

  <pre id="fileDisplayArea">Please select file(s) above.</pre>

  <!--Div that will hold the pie chart-->
  <div id="chart_div">None Loaded</div>

  Latency time units:
  <select name="units" size="1" id="timeUnitSelection" onChange="timeUnitsSelected()">
      <option value="Latency (seconds)">seconds</option>
      <option selected value="Latency (milliseconds)">milliseconds</option>
      <option value="Latency (µs)">microseconds</option>
      <option value="Latency (nanoseconds)">nanoseconds</option>
  </select>
  <button type='button' onclick='doExport(event)'>Export Image</button>

  &nbsp; &nbsp; &nbsp; &nbsp;
  <p>
  Percentile range:

  <input type="range" class="slider-width500"
         min="1" max="8" value="7" step="1"
         width="300px"
         onchange="showValue(this.value)" />
  <span id="percentileRange">99.99999%</span>
  <script type="text/javascript">
      function showValue(newValue) {
          var x = Math.pow(10, newValue);
          percentile = 100.0 - (100.0 / x);
          document.getElementById("percentileRange").innerHTML=percentile + "%";
          maxPercentile = x;
          drawChart();
      }
  </script>
  </p>
  <p>
      <br>
  *** Note: Input files are expected to be in the .hgrm format produced by
  HistogramLogProcessor, or the percentile output format for HdrHistogram.
  See example file format
      <a href="https://github.com/HdrHistogram/HdrHistogram/blob/master/GoogleChartsExample/example1.txt">here</a>
  </p>
  <!--<h4>Expected Service Level:</h4>-->
  <!--<input type="checkbox" name="ESL" value="ESL">Plot Expected Service Level<br>-->
  <!--Percentile:-->
  <!--<input type="text" id="ESLPercentile0" name="ESLPercentile0" size="6" value = 90 />-->
  <!--% &nbsp &nbsp &nbsp Limit:-->
  <!--<input type="text" id="ESLLimit0" name="ESLLimit0" size="12"/>-->
  <!--<br>-->
  <!--Percentile:-->
  <!--<input type="text" id="ESLPercentile1" name="ESLPercentile1" size="6" value = 99 />-->
  <!--% &nbsp &nbsp &nbsp Limit:-->
  <!--<input type="text" id="ESLLimit1" name="ESLLimit1" size="12"/>-->
  <!--<br>-->
  <!--Percentile:-->
  <!--<input type="text" id="ESLPercentile2" name="ESLPercentile2" size="6" value = 99.99 />-->
  <!--% &nbsp &nbsp &nbsp Limit:-->
  <!--<input type="text" id="ESLLimit2" name="ESLLimit2" size="12"/>-->
  <!--<br>-->
  <!--Percentile:-->
  <!--<input type="text" id="ESLPercentile3" name="ESLPercentile2" size="6" value="100.0" readonly/>-->
  <!--% &nbsp &nbsp &nbsp Limit:-->
  <!--<input type="text" id="ESLLimit3" name="ESLLimit2" size="12"/>-->

  $data-divs
  """
