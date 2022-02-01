var i, l, selectedLine = null;

/* Navigate to hash without browser history entry */
var navigateToHash = function () {
    if (window.history !== undefined && window.history.replaceState !== undefined) {
        window.history.replaceState(undefined, undefined, this.getAttribute("href"));
    }
};

var hashLinks = document.getElementsByClassName('navigatetohash');
for (i = 0, l = hashLinks.length; i < l; i++) {
    hashLinks[i].addEventListener('click', navigateToHash);
}

/* Switch test method */
var switchTestMethod = function () {
    var method = this.getAttribute("value");
    console.log("Selected test method: " + method);

    var lines, i, l, coverageData, lineAnalysis, cells;

    lines = document.querySelectorAll('.lineAnalysis tr');

    for (i = 1, l = lines.length; i < l; i++) {
        coverageData = JSON.parse(lines[i].getAttribute('data-coverage').replace(/'/g, '"'));
        lineAnalysis = coverageData[method];
        cells = lines[i].querySelectorAll('td');
        if (lineAnalysis === undefined) {
            lineAnalysis = coverageData.AllTestMethods;
            if (lineAnalysis.LVS !== 'gray') {
                cells[0].setAttribute('class', 'red');
                cells[1].innerText = cells[1].textContent = '0';
                cells[4].setAttribute('class', 'lightred');
            }
        } else {
            cells[0].setAttribute('class', lineAnalysis.LVS);
            cells[1].innerText = cells[1].textContent = lineAnalysis.VC;
            cells[4].setAttribute('class', 'light' + lineAnalysis.LVS);
        }
    }
};

var testMethods = document.getElementsByClassName('switchtestmethod');
for (i = 0, l = testMethods.length; i < l; i++) {
    testMethods[i].addEventListener('change', switchTestMethod);
}

/* Highlight test method by line */
var toggleLine = function () {
    if (selectedLine === this) {
        selectedLine = null;
    } else {
        selectedLine = null;
        unhighlightTestMethods();
        highlightTestMethods.call(this);
        selectedLine = this;
    }
    
};
var highlightTestMethods = function () {
    if (selectedLine !== null) {
        return;
    }

    var lineAnalysis;
    var coverageData = JSON.parse(this.getAttribute('data-coverage').replace(/'/g, '"'));
    var testMethods = document.getElementsByClassName('testmethod');

    for (i = 0, l = testMethods.length; i < l; i++) {
        lineAnalysis = coverageData[testMethods[i].id];
        if (lineAnalysis === undefined) {
            testMethods[i].className = testMethods[i].className.replace(/\s*light.+/g, "");
        } else {
            testMethods[i].className += ' light' + lineAnalysis.LVS;
        }
    }
};
var unhighlightTestMethods = function () {
    if (selectedLine !== null) {
        return;
    }

    var testMethods = document.getElementsByClassName('testmethod');
    for (i = 0, l = testMethods.length; i < l; i++) {
        testMethods[i].className = testMethods[i].className.replace(/\s*light.+/g, "");
    }
};
var coverableLines = document.getElementsByClassName('coverableline');
for (i = 0, l = coverableLines.length; i < l; i++) {
    coverableLines[i].addEventListener('click', toggleLine);
    coverableLines[i].addEventListener('mouseenter', highlightTestMethods);
    coverableLines[i].addEventListener('mouseleave', unhighlightTestMethods);
}

/* History charts */
var renderChart = function (chart) {
    // Remove current children (e.g. PNG placeholder)
    while (chart.firstChild) {
        chart.firstChild.remove();
    }

    var chartData = window[chart.getAttribute('data-data')];
    var options = {
        axisY: {
            type: undefined,
            onlyInteger: true
        },
        lineSmooth: false,
        low: 0,
        high: 100,
        scaleMinSpace: 20,
        onlyInteger: true,
        fullWidth: true
    };
    var lineChart = new Chartist.Line(chart, {
        labels: [],
        series: chartData.series
    }, options);

    /* Zoom */
    var zoomButtonDiv = document.createElement("div");
    zoomButtonDiv.className = "toggleZoom";
    var zoomButtonLink = document.createElement("a");
    zoomButtonLink.setAttribute("href", "");
    var zoomButtonText = document.createElement("i");
    zoomButtonText.className = "icon-search-plus";

    zoomButtonLink.appendChild(zoomButtonText);
    zoomButtonDiv.appendChild(zoomButtonLink);

    chart.appendChild(zoomButtonDiv);

    zoomButtonDiv.addEventListener('click', function (event) {
        event.preventDefault();

        if (options.axisY.type === undefined) {
            options.axisY.type = Chartist.AutoScaleAxis;
            zoomButtonText.className = "icon-search-minus";
        } else {
            options.axisY.type = undefined;
            zoomButtonText.className = "icon-search-plus";
        }

        lineChart.update(null, options);
    });

    var tooltip = document.createElement("div");
    tooltip.className = "tooltip";

    chart.appendChild(tooltip);

    /* Tooltips */
    var showToolTip = function () {
        var index = this.getAttribute('ct:meta');

        tooltip.innerHTML = chartData.tooltips[index];
        tooltip.style.display = 'block';
    };

    var moveToolTip = function (event) {
        var box = chart.getBoundingClientRect();
        var left = event.pageX - box.left - window.pageXOffset;
        var top = event.pageY - box.top - window.pageYOffset;

        left = left + 20;
        top = top - tooltip.offsetHeight / 2;

        if (left + tooltip.offsetWidth > box.width) {
            left -= tooltip.offsetWidth + 40;
        }

        if (top < 0) {
            top = 0;
        }

        if (top + tooltip.offsetHeight > box.height) {
            top = box.height - tooltip.offsetHeight;
        }

        tooltip.style.left = left + 'px';
        tooltip.style.top = top + 'px';
    };

    var hideToolTip = function () {
        tooltip.style.display = 'none';
    };
    chart.addEventListener('mousemove', moveToolTip);

    lineChart.on('created', function () {
        var chartPoints = chart.getElementsByClassName('ct-point');
        for (i = 0, l = chartPoints.length; i < l; i++) {
            chartPoints[i].addEventListener('mousemove', showToolTip);
            chartPoints[i].addEventListener('mouseout', hideToolTip);
        }
    });
};

var charts = document.getElementsByClassName('historychart');
for (i = 0, l = charts.length; i < l; i++) {
    renderChart(charts[i]);
}