/* Helper methods */
function createRandomId(length) {
    var possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789', id = '', i;

    for (i = 0; i < length; i++) {
        id += possible.charAt(Math.floor(Math.random() * possible.length));
    }

    return id;
}

function roundNumber(number, precision) {
    return Math.floor(number * Math.pow(10, precision)) / Math.pow(10, precision);
}

function getNthOrLastIndexOf(text, substring, n) {
    var times = 0, index = -1, currentIndex = -1;

    while (times < n) {
        currentIndex = text.indexOf(substring, index + 1);
        if (currentIndex === -1) {
            break;
        } else {
            index = currentIndex;
        }

        times++;
    }

    return index;
}

/* Data models */
function ClassViewModel(serializedClass) {
    var self = this;
    self.isNamespace = false;
    self.name = serializedClass.name;
    self.parent = null;
    self.reportPath = serializedClass.reportPath;
    self.coveredLines = serializedClass.coveredLines;
    self.uncoveredLines = serializedClass.uncoveredLines;
    self.coverableLines = serializedClass.coverableLines;
    self.totalLines = serializedClass.totalLines;
    self.coverageType = serializedClass.coverageType;
    self.coveredBranches = serializedClass.coveredBranches,
    self.totalBranches = serializedClass.totalBranches;
    self.lineCoverageHistory = serializedClass.lineCoverageHistory;
    self.branchCoverageHistory = serializedClass.branchCoverageHistory;

    if (serializedClass.coverableLines === 0) {
        if (isNaN(serializedClass.methodCoverage)) {
            self.coverage = NaN;
            self.coveragePercent = '';
            self.coverageTitle = '';
        } else {
            self.coverage = serializedClass.methodCoverage;
            self.coveragePercent = self.coverage + '%';
            self.coverageTitle = serializedClass.coverageType;
        }
    } else {
        self.coverage = roundNumber((100 * serializedClass.coveredLines) / serializedClass.coverableLines, 1);
        self.coveragePercent = self.coverage + '%';
        self.coverageTitle = serializedClass.coverageType;
    }

    if (serializedClass.totalBranches === 0) {
        self.branchCoverage = NaN;
        self.branchCoveragePercent = '';
    } else {
        self.branchCoverage = roundNumber((100 * serializedClass.coveredBranches) / serializedClass.totalBranches, 1);
        self.branchCoveragePercent = self.branchCoverage + '%';
    }

    self.visible = function (filter) {
        return filter === '' || self.name.toLowerCase().indexOf(filter) > -1;
    };
}

function CodeElementViewModel(name, parent) {
    var self = this;
    self.isNamespace = true;
    self.name = name;
    self.parent = parent;
    self.subelements = [];
    self.coverageType = translations.lineCoverage;
    self.collapsed = name.indexOf('Test') > -1 && parent === null;

    self.coveredLines = 0;
    self.uncoveredLines = 0;
    self.coverableLines = 0;
    self.totalLines = 0;

    self.coveredBranches = 0;
    self.totalBranches = 0;

    self.coverage = function () {
        if (self.coverableLines === 0) {
            return NaN;
        }

        return roundNumber(100 * self.coveredLines / self.coverableLines, 1);
    };

    self.branchCoverage = function () {
        if (self.totalBranches === 0) {
            return NaN;
        }

        return roundNumber(100 * self.coveredBranches / self.totalBranches, 1);
    };

    self.visible = function (filter) {
        var i, l;
        for (i = 0, l = self.subelements.length; i < l; i++) {
            if (self.subelements[i].visible(filter)) {
                return true;
            }
        }

        return filter === '' || self.name.toLowerCase().indexOf(self.filter) > -1;
    };

    self.insertClass = function (clazz, grouping) {
        var groupingDotIndex, groupedNamespace, i, l, subNamespace;

        self.coveredLines += clazz.coveredLines;
        self.uncoveredLines += clazz.uncoveredLines;
        self.coverableLines += clazz.coverableLines;
        self.totalLines += clazz.totalLines;

        self.coveredBranches += clazz.coveredBranches;
        self.totalBranches += clazz.totalBranches;

        if (grouping === undefined) {
            clazz.parent = self;
            self.subelements.push(clazz);
            return;
        }

        groupingDotIndex = getNthOrLastIndexOf(clazz.name, '.', grouping);
        groupedNamespace = groupingDotIndex === -1 ? '-' : clazz.name.substr(0, groupingDotIndex);

        for (i = 0, l = self.subelements.length; i < l; i++) {
            if (self.subelements[i].name === groupedNamespace) {
                self.subelements[i].insertClass(clazz);
                return;
            }
        }

        subNamespace = new CodeElementViewModel(groupedNamespace, self);
        self.subelements.push(subNamespace);
        subNamespace.insertClass(clazz);
    };

    self.collapse = function () {
        var i, l, element;

        self.collapsed = true;

        for (i = 0, l = self.subelements.length; i < l; i++) {
            element = self.subelements[i];

            if (element.isNamespace) {
                element.collapse();
            }
        }
    };

    self.expand = function () {
        var i, l, element;

        self.collapsed = false;

        for (i = 0, l = self.subelements.length; i < l; i++) {
            element = self.subelements[i];

            if (element.isNamespace) {
                element.expand();
            }
        }
    };

    self.toggleCollapse = function () {
        self.collapsed = !self.collapsed;
    };

    self.changeSorting = function (sortby, ascending) {
        var smaller = ascending ? -1 : 1, bigger = ascending ? 1 : -1, i, l, element;

        if (sortby === 'name') {
            self.subelements.sort(function (left, right) {
                return left.name === right.name ? 0 : (left.name < right.name ? smaller : bigger);
            });
        } else {
            if (self.subelements.length > 0 && self.subelements[0].isNamespace) {
                // Top level elements are resorted ASC by name if other sort columns than 'name' is selected
                self.subelements.sort(function (left, right) {
                    return left.name === right.name ? 0 : (left.name < right.name ? -1 : 1);
                });
            } else {
                if (sortby === 'covered') {
                    self.subelements.sort(function (left, right) {
                        return left.coveredLines === right.coveredLines ?
                                0
                                : (left.coveredLines < right.coveredLines ? smaller : bigger);
                    });
                } else if (sortby === 'uncovered') {
                    self.subelements.sort(function (left, right) {
                        return left.uncoveredLines === right.uncoveredLines ?
                                0
                                : (left.uncoveredLines < right.uncoveredLines ? smaller : bigger);
                    });
                } else if (sortby === 'coverable') {
                    self.subelements.sort(function (left, right) {
                        return left.coverableLines === right.coverableLines ?
                                0
                                : (left.coverableLines < right.coverableLines ? smaller : bigger);
                    });
                } else if (sortby === 'total') {
                    self.subelements.sort(function (left, right) {
                        return left.totalLines === right.totalLines ?
                                0
                                : (left.totalLines < right.totalLines ? smaller : bigger);
                    });
                } else if (sortby === 'coverage') {
                    self.subelements.sort(function (left, right) {
                        if (left.coverage === right.coverage) {
                            return 0;
                        } else if (isNaN(left.coverage)) {
                            return smaller;
                        } else if (isNaN(right.coverage)) {
                            return bigger;
                        } else {
                            return left.coverage < right.coverage ? smaller : bigger;
                        }
                    });
                } else if (sortby === 'branchcoverage') {
                    self.subelements.sort(function (left, right) {
                        if (left.branchCoverage === right.branchCoverage) {
                            return 0;
                        } else if (isNaN(left.branchCoverage)) {
                            return smaller;
                        } else if (isNaN(right.branchCoverage)) {
                            return bigger;
                        } else {
                            return left.branchCoverage < right.branchCoverage ? smaller : bigger;
                        }
                    });
                }
            }
        }

        for (i = 0, l = self.subelements.length; i < l; i++) {
            element = self.subelements[i];

            if (element.isNamespace) {
                element.changeSorting(sortby, ascending);
            }
        }
    };
}

/* React components */
var AssemblyComponent = React.createClass({
    getAssemblies: function (assemblies, grouping, sortby, sortorder) {
        var i, l, j, l2, assemblyElement, parentElement, cls, smaller, bigger, result;

        result = [];

        if (grouping === '0') { // Group by assembly
            for (i = 0, l = assemblies.length; i < l; i++) {
                assemblyElement = new CodeElementViewModel(assemblies[i].name, null);
                result.push(assemblyElement);

                for (j = 0, l2 = assemblies[i].classes.length; j < l2; j++) {
                    cls = assemblies[i].classes[j];
                    assemblyElement.insertClass(new ClassViewModel(cls));
                }
            }
        } else if (grouping === '-1') { // No grouping
            parentElement = new CodeElementViewModel(translations.all, null);
            result.push(parentElement);

            for (i = 0, l = assemblies.length; i < l; i++) {
                for (j = 0, l2 = assemblies[i].classes.length; j < l2; j++) {
                    cls = assemblies[i].classes[j];
                    parentElement.insertClass(new ClassViewModel(cls));
                }
            }
        } else { // Group by assembly and namespace
            for (i = 0, l = assemblies.length; i < l; i++) {
                assemblyElement = new CodeElementViewModel(assemblies[i].name, null);
                result.push(assemblyElement);

                for (j = 0, l2 = assemblies[i].classes.length; j < l2; j++) {
                    cls = assemblies[i].classes[j];
                    assemblyElement.insertClass(new ClassViewModel(cls), grouping);
                }
            }
        }

        if (sortby === 'name') {
            smaller = sortorder === 'asc' ? -1 : 1;
            bigger = sortorder === 'asc' ? 1 : -1;
        } else {
            smaller = -1;
            bigger = 1;
        }

        result.sort(function (left, right) {
            return left.name === right.name ? 0 : (left.name < right.name ? smaller : bigger);
        });

        for (i = 0, l = result.length; i < l; i++) {
            result[i].changeSorting(sortby, sortorder === 'asc');
        }

        return result;
    },
    getGroupingMaximum: function (assemblies) {
        var i, l, j, l2, result;

        result = 1;

        for (i = 0, l = assemblies.length; i < l; i++) {
            for (j = 0, l2 = assemblies[i].classes.length; j < l2; j++) {
                result = Math.max(
                    result,
                    (assemblies[i].classes[j].name.match(/\./g) || []).length
                );
            }
        }

        console.log("Grouping maximum: " + result);

        return result;
    },
    getInitialState: function () {
        var state, collapseState;

        if (window.history !== undefined && window.history.replaceState !== undefined && window.history.state !== null) {
            state = angular.copy(window.history.state);
            collapseState = state.assemblies;
        } else {
            state = {
                grouping: '0',
                groupingMaximum: this.getGroupingMaximum(this.props.assemblies),
                filter: '',
                sortby: 'name',
                sortorder: 'asc',
                assemblies: null,
                branchCoverageAvailable: this.props.branchCoverageAvailable
            };
        }

        state.assemblies = this.getAssemblies(this.props.assemblies, state.grouping, state.filter, state.sortby, state.sortorder);

        if (collapseState !== undefined) {
            this.restoreCollapseState(collapseState, state.assemblies);
        }

        return state;
    },
    collapseAll: function () {
        console.log("Collapsing all");
        var i, l;
        for (i = 0, l = this.state.assemblies.length; i < l; i++) {
            this.state.assemblies[i].collapse();
        }

        this.setState({ assemblies: this.state.assemblies });
    },
    expandAll: function () {
        console.log("Expanding all");

        var i, l;
        for (i = 0, l = this.state.assemblies.length; i < l; i++) {
            this.state.assemblies[i].expand();
        }

        this.setState({ assemblies: this.state.assemblies });
    },
    toggleCollapse: function (assembly) {
        assembly.toggleCollapse();
        this.setState({ assemblies: this.state.assemblies });
    },
    updateGrouping: function (grouping) {
        console.log("Updating grouping: " + grouping);

        var assemblies = this.getAssemblies(this.props.assemblies, grouping, this.state.sortby, this.state.sortorder);
        this.setState({ grouping: grouping, assemblies: assemblies });
    },
    updateFilter: function (filter) {
        filter = filter.toLowerCase();

        if (filter === this.state.filter) {
            return;
        }

        console.log("Updating filter: " + filter);
        this.setState({ filter: filter });
    },
    updateSorting: function (sortby) {
        var sortorder = 'asc', assemblies;

        if (sortby === this.state.sortby) {
            sortorder = this.state.sortorder === 'asc' ? 'desc' : 'asc';
        }

        console.log("Updating sorting: " + sortby + ", " + sortorder);
        assemblies = this.getAssemblies(this.props.assemblies, this.state.grouping, sortby, sortorder);
        this.setState({ sortby: sortby, sortorder: sortorder, assemblies: assemblies });
    },
    restoreCollapseState: function (source, target) {
        var i;

        try {
            for (i = 0; i < target.length; i++) {
                if (target[i].isNamespace) {
                    target[i].collapsed = source[i].collapsed;
                    this.restoreCollapseState(source[i].subelements, target[i].subelements)
                }
            }
        } catch (e) {
            // This can only happen if assembly structure was changed.
            // That means the complete report was updated in the background and the reloaded in the same tab/window.
            console.log("Restoring of collapse state failed.");
        }
    },
    extractCollapseState: function (target) {
        var i, currentResult, result = [];

        for (i = 0; i < target.length; i++) {
            if (target[i].isNamespace) {
                currentResult = {
                    collapsed: target[i].collapsed,
                    subelements: this.extractCollapseState(target[i].subelements)

                };
                result.push(currentResult);
            }
        }

        return result;
    },
    render: function () {
        if (window.history !== undefined && window.history.replaceState !== undefined) {
            var historyState, i;
            historyState = angular.copy(this.state);

            historyState.assemblies = this.extractCollapseState(historyState.assemblies);

            window.history.replaceState(historyState, null);
        }

        return (
            React.DOM.div(null,
                SearchBar({
                    groupingMaximum: this.state.groupingMaximum,
                    grouping: this.state.grouping,
                    filter: this.state.filter,
                    collapseAll: this.collapseAll,
                    expandAll: this.expandAll,
                    updateGrouping: this.updateGrouping,
                    updateFilter: this.updateFilter
                }),
                AssemblyTable({
                    filter: this.state.filter,
                    assemblies: this.state.assemblies,
                    sortby: this.state.sortby,
                    sortorder: this.state.sortorder,
                    branchCoverageAvailable: this.state.branchCoverageAvailable,
                    updateSorting: this.updateSorting,
                    toggleCollapse: this.toggleCollapse
                }))
        );
    }
});

var SearchBar = React.createClass({
    collapseAllClickHandler: function (event) {
        event.nativeEvent.preventDefault();
        this.props.collapseAll();
    },
    expandAllClickHandler: function (event) {
        event.nativeEvent.preventDefault();
        this.props.expandAll();
    },
    groupingChangedHandler: function () {
        this.props.updateGrouping(this.refs.groupingInput.getDOMNode().value);
    },
    filterChangedHandler: function () {
        this.props.updateFilter(this.refs.filterInput.getDOMNode().value);
    },
    render: function () {
        var groupingDescription = translations.byNamespace + ' ' + this.props.grouping;

        if (this.props.grouping === '-1') {
            groupingDescription = translations.noGrouping;
        } else if (this.props.grouping === '0') {
            groupingDescription = translations.byAssembly;
        }

        return (
            React.DOM.div({ className: 'customizebox' },
                React.DOM.div(null,
                    React.DOM.a({ href: '', onClick: this.collapseAllClickHandler }, translations.collapseAll),
                    React.DOM.span(null, " | "),
                    React.DOM.a({ href: '', onClick: this.expandAllClickHandler }, translations.expandAll)),
                React.DOM.div({ className: 'center' },
                    React.DOM.span(null, groupingDescription),
                    React.DOM.br(),
                    React.DOM.span(null, translations.grouping + ' '),
                    React.DOM.input({
                        ref: 'groupingInput',
                        type: 'range',
                        step: 1,
                        min: -1,
                        max: this.props.groupingMaximum,
                        value: this.props.grouping,
                        onChange: this.groupingChangedHandler
                    })),
                React.DOM.div({ className: 'right' },
                    React.DOM.span(null, translations.filter + ' '),
                    React.DOM.input({
                        ref: 'filterInput',
                        type: 'text',
                        value: this.props.filter,
                        onChange: this.filterChangedHandler,
                        onInput: this.filterChangedHandler /* Handle text input immediately */
                    })))
        );
    }
});

var AssemblyTable = React.createClass({
    renderAllChilds: function (result, currentElement) {
        var i, l;

        if (currentElement.visible(this.props.filter)) {
            if (currentElement.isNamespace) {
                result.push(AssemblyRow({ assembly: currentElement,
                    branchCoverageAvailable: this.props.branchCoverageAvailable,
                    toggleCollapse: this.props.toggleCollapse
                }));

                if (!currentElement.collapsed) {
                    for (i = 0, l = currentElement.subelements.length; i < l; i++) {
                        this.renderAllChilds(result, currentElement.subelements[i]);
                    }
                }
            } else {
                result.push(ClassRow({
                    clazz: currentElement,
                    branchCoverageAvailable: this.props.branchCoverageAvailable
                }));
            }
        }
    },
    render: function () {
        var rows = [], i, l;

        for (i = 0, l = this.props.assemblies.length; i < l; i++) {
            this.renderAllChilds(rows, this.props.assemblies[i]);
        }

        return (
            React.DOM.table({ className: 'overview table-fixed stripped' },
                React.DOM.colgroup(null,
                    React.DOM.col(null),
                    React.DOM.col({ className: 'column90' }),
                    React.DOM.col({ className: 'column105' }),
                    React.DOM.col({ className: 'column100' }),
                    React.DOM.col({ className: 'column70' }),
                    React.DOM.col({ className: 'column98' }),
                    React.DOM.col({ className: 'column112' }),
                    this.props.branchCoverageAvailable ? React.DOM.col({ className: 'column98' }) : null,
                    this.props.branchCoverageAvailable ? React.DOM.col({ className: 'column112' }) : null),
                TableHeader({
                    sortby: this.props.sortby,
                    sortorder: this.props.sortorder,
                    updateSorting: this.props.updateSorting,
                    branchCoverageAvailable: this.props.branchCoverageAvailable
                }),
                React.DOM.tbody(null, rows))
        );
    }
});

var TableHeader = React.createClass({
    sortingChangedHandler: function (event, sortby) {
        event.nativeEvent.preventDefault();
        this.props.updateSorting(sortby);
    },
    render: function () {
        var nameClass = this.props.sortby === 'name' ? (this.props.sortorder === 'desc' ? 'icon-up-dir highlight' : 'icon-down-dir highlight') : 'icon-down-dir';
        var coveredClass = this.props.sortby === 'covered' ? (this.props.sortorder === 'desc' ? 'icon-up-dir highlight' : 'icon-down-dir highlight') : 'icon-down-dir';
        var uncoveredClass = this.props.sortby === 'uncovered' ? (this.props.sortorder === 'desc' ? 'icon-up-dir highlight' : 'icon-down-dir highlight') : 'icon-down-dir';
        var coverableClass = this.props.sortby === 'coverable' ? (this.props.sortorder === 'desc' ? 'icon-up-dir highlight' : 'icon-down-dir highlight') : 'icon-down-dir';
        var totalClass = this.props.sortby === 'total' ? (this.props.sortorder === 'desc' ? 'icon-up-dir highlight' : 'icon-down-dir highlight') : 'icon-down-dir';
        var coverageClass = this.props.sortby === 'coverage' ? (this.props.sortorder === 'desc' ? 'icon-up-dir highlight' : 'icon-down-dir highlight') : 'icon-down-dir';
        var branchCoverageClass = this.props.sortby === 'branchcoverage' ? (this.props.sortorder === 'desc' ? 'icon-up-dir highlight' : 'icon-down-dir highlight') : 'icon-down-dir';

        return (
            React.DOM.thead(null,
                React.DOM.tr(null,
                    React.DOM.th(null,
                        React.DOM.a(
                            { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'name'); }.bind(this) },
                            React.DOM.i({ className: nameClass }),
                            translations.name)),
                    React.DOM.th({ className: 'right' },
                        React.DOM.a(
                            { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'covered'); }.bind(this) },
                            React.DOM.i({ className: coveredClass }),
                            translations.covered)),
                    React.DOM.th({ className: 'right' },
                        React.DOM.a(
                            { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'uncovered'); }.bind(this) },
                            React.DOM.i({ className: uncoveredClass }),
                            translations.uncovered)),
                    React.DOM.th({ className: 'right' },
                        React.DOM.a(
                            { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'coverable'); }.bind(this) },
                            React.DOM.i({ className: coverableClass }),
                            translations.coverable)),
                    React.DOM.th({ className: 'right' },
                        React.DOM.a(
                            { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'total'); }.bind(this) },
                            React.DOM.i({ className: totalClass }),
                            translations.total)),
                    React.DOM.th({ className: 'center', colSpan: '2' },
                        React.DOM.a(
                            { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'coverage'); }.bind(this) },
                            React.DOM.i({ className: coverageClass }),
                            translations.coverage)),
                    this.props.branchCoverageAvailable ? React.DOM.th({ className: 'center', colSpan: '2' },
                        React.DOM.a(
                            { href: '', onClick: function (event) { this.sortingChangedHandler(event, 'branchcoverage'); }.bind(this) },
                            React.DOM.i({ className: branchCoverageClass }),
                            translations.branchCoverage)) : null))
        );
    }
});

var AssemblyRow = React.createClass({
    toggleCollapseClickHandler: function (event) {
        event.nativeEvent.preventDefault();
        this.props.toggleCollapse(this.props.assembly);
    },
    render: function () {
        var greenHidden, redHidden, grayHidden, coverageTable, branchGreenHidden, branchRedHidden, branchGrayHidden, branchCoverageTable, id;

        greenHidden = !isNaN(this.props.assembly.coverage()) && Math.round(this.props.assembly.coverage()) > 0 ? '' : ' hidden';
        redHidden = !isNaN(this.props.assembly.coverage()) && 100 - Math.round(this.props.assembly.coverage()) > 0 ? '' : ' hidden';
        grayHidden = isNaN(this.props.assembly.coverage()) ? '' : ' hidden';

        coverageTable = React.DOM.table(
            { className: 'coverage' },
            React.DOM.tbody(null,
                React.DOM.tr(null,
                    React.DOM.td({ className: 'green covered' + Math.round(this.props.assembly.coverage()) + greenHidden }, ' '),
                    React.DOM.td({ className: 'red covered' + (100 - Math.round(this.props.assembly.coverage())) + redHidden }, ' '),
                    React.DOM.td({ className: 'gray covered100' + grayHidden }, ' '))));

        branchGreenHidden = !isNaN(this.props.assembly.coverage()) && Math.round(this.props.assembly.branchCoverage()) > 0 ? '' : ' hidden';
        branchRedHidden = !isNaN(this.props.assembly.coverage()) && 100 - Math.round(this.props.assembly.branchCoverage()) > 0 ? '' : ' hidden';
        branchGrayHidden = isNaN(this.props.assembly.branchCoverage()) ? '' : ' hidden';

        branchCoverageTable = React.DOM.table(
            { className: 'coverage' },
            React.DOM.tbody(null,
                React.DOM.tr(null,
                    React.DOM.td({ className: 'green covered' + Math.round(this.props.assembly.branchCoverage()) + branchGreenHidden }, ' '),
                    React.DOM.td({ className: 'red covered' + (100 - Math.round(this.props.assembly.branchCoverage())) + branchRedHidden }, ' '),
                    React.DOM.td({ className: 'gray covered100' + branchGrayHidden }, ' '))));

        id = '_' + createRandomId(8);

        return (
          React.DOM.tr({ className: this.props.assembly.parent !== null ? 'namespace' : null },
            React.DOM.th(null,
                React.DOM.a(
                        {
                            id: this.props.assembly.name + id,
                            href: '',
                            onClick: this.toggleCollapseClickHandler
                        },
                        React.DOM.i({ className: this.props.assembly.collapsed ? 'icon-plus' : 'icon-minus highlight'}),
                        this.props.assembly.name)),
            React.DOM.th({ className: 'right' }, this.props.assembly.coveredLines),
            React.DOM.th({ className: 'right' }, this.props.assembly.uncoveredLines),
            React.DOM.th({ className: 'right' }, this.props.assembly.coverableLines),
            React.DOM.th({ className: 'right' }, this.props.assembly.totalLines),
            React.DOM.th(
                    {
                        className: 'right',
                        title: isNaN(this.props.assembly.coverage()) ? '' : this.props.assembly.coverageType
                    },
                    isNaN(this.props.assembly.coverage()) ? '' : this.props.assembly.coverage() + '%'),
            React.DOM.th(null, coverageTable),
            this.props.branchCoverageAvailable ? React.DOM.th(
                    {
                        className: 'right'
                    },
                    isNaN(this.props.assembly.branchCoverage()) ? '' : this.props.assembly.branchCoverage() + '%') : null,
            this.props.branchCoverageAvailable ? React.DOM.th(null, branchCoverageTable) : null)
        );
    }
});

var ClassRow = React.createClass({
    render: function () {
        var nameElement, greenHidden, redHidden, grayHidden, coverageTable, branchGreenHidden, branchRedHidden, branchGrayHidden, branchCoverageTable;

        if (this.props.clazz.reportPath === '') {
            nameElement = React.DOM.span(null, this.props.clazz.name);
        } else {
            nameElement = React.DOM.a({ href: this.props.clazz.reportPath }, this.props.clazz.name);
        }

        greenHidden = !isNaN(this.props.clazz.coverage) && Math.round(this.props.clazz.coverage) > 0 ? '' : ' hidden';
        redHidden = !isNaN(this.props.clazz.coverage) && 100 - Math.round(this.props.clazz.coverage) > 0 ? '' : ' hidden';
        grayHidden = isNaN(this.props.clazz.coverage) ? '' : ' hidden';

        coverageTable = React.DOM.table(
            { className: 'coverage' },
            React.DOM.tbody(null,
                React.DOM.tr(null,
                    React.DOM.td({ className: 'green covered' + Math.round(this.props.clazz.coverage) + greenHidden }, ' '),
                    React.DOM.td({ className: 'red covered' + (100 - Math.round(this.props.clazz.coverage)) + redHidden }, ' '),
                    React.DOM.td({ className: 'gray covered100' + grayHidden }, ' '))));

        branchGreenHidden = !isNaN(this.props.clazz.branchCoverage) && Math.round(this.props.clazz.branchCoverage) > 0 ? '' : ' hidden';
        branchRedHidden = !isNaN(this.props.clazz.branchCoverage) && 100 - Math.round(this.props.clazz.branchCoverage) > 0 ? '' : ' hidden';
        branchGrayHidden = isNaN(this.props.clazz.branchCoverage) ? '' : ' hidden';

        branchCoverageTable = React.DOM.table(
            { className: 'coverage' },
            React.DOM.tbody(null,
                React.DOM.tr(null,
                    React.DOM.td({ className: 'green covered' + Math.round(this.props.clazz.branchCoverage) + branchGreenHidden }, ' '),
                    React.DOM.td({ className: 'red covered' + (100 - Math.round(this.props.clazz.branchCoverage)) + branchRedHidden }, ' '),
                    React.DOM.td({ className: 'gray covered100' + branchGrayHidden }, ' '))));

        return (
            React.DOM.tr({ className: this.props.clazz.parent.parent !== null ? 'namespace' : null },
                React.DOM.td(null, nameElement),
                React.DOM.td({ className: 'right' }, this.props.clazz.coveredLines),
                React.DOM.td({ className: 'right' }, this.props.clazz.uncoveredLines),
                React.DOM.td({ className: 'right' }, this.props.clazz.coverableLines),
                React.DOM.td({ className: 'right' }, this.props.clazz.totalLines),
                React.DOM.td({ className: 'right', title: this.props.clazz.coverageTitle },
                    CoverageHistoryChart({
                        historicCoverage: this.props.clazz.lineCoverageHistory,
                        cssClass: 'tinylinecoveragechart',
                        title: translations.history + ": " + translations.coverage,
                        id: 'chart' + createRandomId(8)
                    }),
                    this.props.clazz.coveragePercent),
                React.DOM.td(null, coverageTable),
                this.props.branchCoverageAvailable ? React.DOM.td({ className: 'right' },
                    CoverageHistoryChart({
                        historicCoverage: this.props.clazz.branchCoverageHistory,
                        cssClass: 'tinybranchcoveragechart',
                        title: translations.history + ": " + translations.branchCoverage,
                        id: 'chart' + createRandomId(8)
                    }),
                    this.props.clazz.branchCoveragePercent) : null,
                this.props.branchCoverageAvailable ? React.DOM.td(null, branchCoverageTable) : null)
        );
    }
});

var CoverageHistoryChart = React.createClass({
    updateChart: function () {
        if (this.props.historicCoverage.length <= 1) {
            return;
        }

        new Chartist.Line('#' + this.props.id, {
            labels: [],
            series: [this.props.historicCoverage]
        }, {
            axisX: {
                offset: 0,
                showLabel: false,
                showGrid: false
            },
            axisY: {
                offset: 0,
                showLabel: false,
                showGrid: false,
                scaleMinSpace: 0.1
            },
            showPoint: false,
            chartPadding: 0,
            lineSmooth: false,
            low: 0,
            high: 100,
            fullWidth: true,
        });
    },
    componentDidMount: function () {
        this.updateChart();
    },
    componentDidUpdate: function () {
        this.updateChart();
    },
    render: function () {
        if (this.props.historicCoverage.length <= 1) {
            return (
                React.DOM.div(
                {
                    id: this.props.id,
                    className: 'hidden',
                })
            );
        } else {
            return (
                React.DOM.div(
                {
                    id: this.props.id,
                    className: this.props.cssClass + ' ct-chart',
                    title: this.props.title
                })
            );
        }
    }
});