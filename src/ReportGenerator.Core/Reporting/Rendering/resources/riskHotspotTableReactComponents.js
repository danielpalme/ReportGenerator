/* React components */
var RiskHotspotsComponent = React.createClass({
    getRiskHotspots: function (riskHotspots, assembly, numberOfHotspots, filter, sortby, sortorder) {
        var result, i, l, smaller = sortorder === 'asc' ? -1 : 1, bigger = sortorder === 'asc' ? 1 : -1;

        result = [];

        for (i = 0, l = riskHotspots.length; i < l; i++) {
            if (filter !== '' && riskHotspots[i].class.toLowerCase().indexOf(filter) === -1) {
                continue;
            }

            if (assembly !== '' && riskHotspots[i].assembly !== assembly) {
                continue;
            }

            result.push(riskHotspots[i]);
        }

        if (sortby === 'assembly') {
            result.sort(function (left, right) {
                return left.assembly === right.assembly ?
                    0
                    : (left.assembly < right.assembly ? smaller : bigger);
            });
        } else if (sortby === 'class') {
            result.sort(function (left, right) {
                return left.class === right.class ?
                    0
                    : (left.class < right.class ? smaller : bigger);
            });
        } else if (sortby === 'method') {
            result.sort(function (left, right) {
                return left.method === right.method ?
                    0
                    : (left.method < right.method ? smaller : bigger);
            });
        } else if (sortby !== '') {
            sortby = parseInt(sortby);
            result.sort(function (left, right) {
                return left.metrics[sortby].value === right.metrics[sortby].value ?
                    0
                    : (left.metrics[sortby].value < right.metrics[sortby].value ? smaller : bigger);
            });
        }
        
        result.splice(numberOfHotspots);

        return result;
    },
    updateSorting: function (sortby) {
        var sortorder = 'asc', assemblies;

        if (sortby === this.state.sortby) {
            sortorder = this.state.sortorder === 'asc' ? 'desc' : 'asc';
        }

        riskHotspots = this.getRiskHotspots(this.props.riskHotspots, this.state.assembly, this.state.numberOfHotspots, this.state.filter, sortby, sortorder);
        this.setState({ sortby: sortby, sortorder: sortorder, riskHotspots: riskHotspots });
    },
    updateAssembly: function (assembly) {
        riskHotspots = this.getRiskHotspots(this.props.riskHotspots, assembly, this.state.numberOfHotspots, this.state.filter, this.state.sortby, this.state.sortorder);
        this.setState({ assembly: assembly, riskHotspots: riskHotspots });
    },
    updateNumberOfHotspots: function (numberOfHotspots) {
        riskHotspots = this.getRiskHotspots(this.props.riskHotspots, this.state.assembly, numberOfHotspots, this.state.filter, this.state.sortby, this.state.sortorder);
        this.setState({ numberOfHotspots: numberOfHotspots, riskHotspots: riskHotspots });
    },
    updateFilter: function (filter) {
        filter = filter.toLowerCase();

        if (filter === this.state.filter) {
            return;
        }

        riskHotspots = this.getRiskHotspots(this.props.riskHotspots, this.state.assembly, this.state.numberOfHotspots, filter, this.state.sortby, this.state.sortorder);
        this.setState({ filter: filter, riskHotspots: riskHotspots });
    },
    getInitialState: function () {
        var state, i;

        if (window.history !== undefined && window.history.replaceState !== undefined && window.history.state !== null && window.history.state.riskHotspotsHistoryState !== undefined) {
            state = angular.copy(window.history.state.riskHotspotsHistoryState);

            // Delete from state
            state.riskHotspots = null;
            state.riskHotspotMetrics = this.props.riskHotspotMetrics;
            state.assemblies = [];
        } else {
            state = {
                riskHotspots: null,
                riskHotspotMetrics: this.props.riskHotspotMetrics,
                assemblies: [],
                assembly: '',
                sortby: '',
                sortorder: 'asc',
                numberOfHotspots: 10,
                filter: ''
            };
        }

        for (i = 0; i < this.props.riskHotspots.length; i++) {
            if (state.assemblies.indexOf(this.props.riskHotspots[i].assembly) === -1) {
                state.assemblies.push(this.props.riskHotspots[i].assembly);
            }
        }

        state.riskHotspots = this.getRiskHotspots(this.props.riskHotspots, state.assembly, state.numberOfHotspots, state.filter, state.sortby, state.sortorder);

        return state;
    },
    render: function () {
        if (window.history !== undefined && window.history.replaceState !== undefined) {
            var riskHotspotsHistoryState, globalState, i;
            riskHotspotsHistoryState = angular.copy(this.state);

            riskHotspotsHistoryState.riskHotspots = null;
            riskHotspotsHistoryState.riskHotspotMetrics = null;
            riskHotspotsHistoryState.assemblies = null;

            if (window.history.state !== null) {
                globalState = angular.copy(window.history.state);
            } else {
                globalState = {};
            }

            globalState.riskHotspotsHistoryState = riskHotspotsHistoryState;
            window.history.replaceState(globalState, null);
        }

        return (
            React.DOM.div(null,
                RiskHotspotsSearchBar({
                    totalNumberOfRiskHotspots: this.props.riskHotspots.length,
                    assemblies: this.state.assemblies,
                    assembly: this.state.assembly,
                    numberOfHotspots: this.state.numberOfHotspots,
                    filter: this.state.filter,
                    updateAssembly: this.updateAssembly,
                    updateNumberOfHotspots: this.updateNumberOfHotspots,
                    updateFilter: this.updateFilter
                }),
                RiskHotspotsTable({
                    riskHotspots: this.state.riskHotspots,
                    riskHotspotMetrics: this.props.riskHotspotMetrics,
                    sortby: this.state.sortby,
                    sortorder: this.state.sortorder,
                    updateSorting: this.updateSorting
                }))
        );
    }
});

var RiskHotspotsSearchBar = React.createClass({
    assemblyChangedHandler: function () {
        this.props.updateAssembly(this.refs.assemblyInput.getDOMNode().value);
    },
    numberOfHotspotsChangedHandler: function () {
        this.props.updateNumberOfHotspots(this.refs.numberOfHotspotsInput.getDOMNode().value);
    },
    filterChangedHandler: function () {
        this.props.updateFilter(this.refs.filterInput.getDOMNode().value);
    },
    render: function () {
        var assemblyOptions = [React.DOM.option({ value: '' }, translations.assembly)], filterElements = [], numberOptions = [], i, l;

        if (this.props.assemblies.length > 1) {
            for (i = 0, l = this.props.assemblies.length; i < l; i++) {
                assemblyOptions.push(React.DOM.option({ value: this.props.assemblies[i] }, this.props.assemblies[i]));
            }

            filterElements.push(React.DOM.div(null,
                React.DOM.select(
                    { ref: 'assemblyInput', value: this.props.assembly, onChange: this.assemblyChangedHandler },
                    assemblyOptions)));
        } else {
            filterElements.push(React.DOM.div(null));
        }

        if (this.props.totalNumberOfRiskHotspots > 10) {
            numberOptions.push(React.DOM.option({ value: 10 }, 10));
            numberOptions.push(React.DOM.option({ value: 20 }, 20));
        }

        if (this.props.totalNumberOfRiskHotspots > 20) {
            numberOptions.push(React.DOM.option({ value: 50 }, 50));
        }

        if (this.props.totalNumberOfRiskHotspots > 50) {
            numberOptions.push(React.DOM.option({ value: 100 }, 100));
        }

        if (this.props.totalNumberOfRiskHotspots > 100) {
            numberOptions.push(React.DOM.option({ value: this.props.totalNumberOfRiskHotspots }, translations.all));
        }

        if (numberOptions.length > 0) {
            filterElements.push(React.DOM.div({ className: 'center' },
                React.DOM.span(null, translations.top + ' '),
                React.DOM.select(
                    { ref: 'numberOfHotspotsInput', value: this.props.numberOfHotspots, onChange: this.numberOfHotspotsChangedHandler },
                    numberOptions)));
        }

        filterElements.push(React.DOM.div({ className: 'right' },
            React.DOM.span(null, translations.filter + ' '),
            React.DOM.input({
                ref: 'filterInput',
                type: 'text',
                value: this.props.filter,
                onChange: this.filterChangedHandler,
                onInput: this.filterChangedHandler /* Handle text input immediately */
            })));

        return (
            React.DOM.div({ className: 'customizebox' }, filterElements)
        );
    }
});

var RiskHotspotsTable = React.createClass({
    render: function () {
        var cols = [React.DOM.col(null), React.DOM.col(null), React.DOM.col(null)], rows = [], i, l;

        for (i = 0, l = this.props.riskHotspotMetrics.length; i < l; i++) {
            cols.push(React.DOM.col({ className: 'column105' }));
        }

        for (i = 0, l = this.props.riskHotspots.length; i < l; i++) {
            rows.push(RiskHotspotRow({
                riskHotspot: this.props.riskHotspots[i]
            }))
        }

        return (
            React.DOM.table({ className: 'overview table-fixed stripped' },
                React.DOM.colgroup(null, cols),
                RiskHotspotsTableHeader({
                    sortby: this.props.sortby,
                    sortorder: this.props.sortorder,
                    updateSorting: this.props.updateSorting,
                    riskHotspotMetrics: this.props.riskHotspotMetrics
                }),
                React.DOM.tbody(null, rows)
            )
        );
    }
});
var RiskHotspotsTableHeader = React.createClass({
    sortingChangedHandler: function (event, sortby) {
        // Click on explanation url should not trigger resorting
        if (sortby !== null) {
            event.nativeEvent.preventDefault();
            this.props.updateSorting(sortby);
        }
    },
    render: function () {
        var ths, i, l, metricClass;

        var assemblyClass = this.props.sortby === 'assembly' ? (this.props.sortorder === 'desc' ? 'icon-up-dir_active' : 'icon-down-dir_active') : 'icon-down-dir';
        var classClass = this.props.sortby === 'class' ? (this.props.sortorder === 'desc' ? 'icon-up-dir_active' : 'icon-down-dir_active') : 'icon-down-dir';
        var methodClass = this.props.sortby === 'method' ? (this.props.sortorder === 'desc' ? 'icon-up-dir_active' : 'icon-down-dir_active') : 'icon-down-dir';
        
        ths = [
            React.DOM.th(null,
                React.DOM.a(
                    { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'assembly'); }.bind(this) },
                    React.DOM.i({ className: assemblyClass }),
                    translations.assembly)),
            React.DOM.th(null,
                React.DOM.a(
                    { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'class'); }.bind(this) },
                    React.DOM.i({ className: classClass }),
                    translations.class)),
            React.DOM.th(null,
                React.DOM.a(
                    { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'method'); }.bind(this) },
                    React.DOM.i({ className: methodClass }),
                    translations.method))];

        for (i = 0, l = this.props.riskHotspotMetrics.length; i < l; i++) {
            metricClass = this.props.sortby !== '' &&this.props.sortby == i ? (this.props.sortorder === 'desc' ? 'icon-up-dir_active' : 'icon-down-dir_active') : 'icon-down-dir';
            ths.push(React.DOM.th(null,
                React.DOM.a(
                    { href: '', 'data-metric': i, onClick: function (event) { this.sortingChangedHandler(event, $(event.nativeEvent.target).closest('a')[0].getAttribute('data-metric')); }.bind(this) },
                    React.DOM.i({ className: metricClass }),
                    this.props.riskHotspotMetrics[i].name + ' ',
                    React.DOM.a({ href: this.props.riskHotspotMetrics[i].explanationUrl }, React.DOM.i({ className: 'icon-info-circled' })))));
        }

        return (
            React.DOM.thead(null,
                React.DOM.tr(null, ths))
        );
    }
});

var RiskHotspotRow = React.createClass({
    render: function () {
        var tds, nameElement, methodElement;

        if (this.props.riskHotspot.reportPath === '') {
            nameElement = React.DOM.span(null, this.props.riskHotspot.class);
            methodElement = this.props.riskHotspot.methodShortName;
        } else {
            nameElement = React.DOM.a({ href: this.props.riskHotspot.reportPath }, this.props.riskHotspot.class);

            if (this.props.riskHotspot.line !== null) {
                methodElement = React.DOM.a({ href: this.props.riskHotspot.reportPath + '#file' + this.props.riskHotspot.fileIndex + '_line' + this.props.riskHotspot.line }, this.props.riskHotspot.methodShortName)
            } else {
                methodElement = this.props.riskHotspot.methodShortName;
            }
        }

        tds = [
            React.DOM.td(null, this.props.riskHotspot.assembly),
            React.DOM.td(null, nameElement),
            React.DOM.td({ title: this.props.riskHotspot.methodName }, methodElement)
        ];

        for (i = 0, l = this.props.riskHotspot.metrics.length; i < l; i++) {
            tds.push(React.DOM.td({ className: this.props.riskHotspot.metrics[i].exceeded ? 'lightred right' : 'lightgreen right' },
                this.props.riskHotspot.metrics[i].value === null ? '-' : this.props.riskHotspot.metrics[i].value));
        }

        return (
            React.DOM.tr(null, tds)
        );
    }
});
