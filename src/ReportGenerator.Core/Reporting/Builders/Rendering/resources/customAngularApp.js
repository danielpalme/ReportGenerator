/* Angular controller for summary report */
function SummaryViewCtrl($scope, $window) {
    var self = this;

    $scope.coverageTableFilteringEnabled = false;
    $scope.assemblies = [];
    $scope.historicCoverageExecutionTimes = [];
    $scope.branchCoverageAvailable = branchCoverageAvailable;

    $scope.riskHotspots = riskHotspots;
    $scope.riskHotspotMetrics = riskHotspotMetrics;

    $scope.enableCoverageTableFiltering = function () {
        console.log("Enabling filtering");

        $scope.assemblies = assemblies;
        $scope.historicCoverageExecutionTimes = historicCoverageExecutionTimes;
        $scope.coverageTableFilteringEnabled = true;
    };

    self.initialize = function () {
        var i, l, numberOfClasses;

        // State is persisted in history. If API or history not available in browser reenable
        if ($window.history === undefined || $window.history.replaceState === undefined || $window.history.state === null) {
            numberOfClasses = 0;

            for (i = 0, l = assemblies.length; i < l; i++) {
                numberOfClasses += assemblies[i].classes.length;
                if (numberOfClasses > 1500) {
                    console.log("Number of classes (filtering disabled): " + numberOfClasses);
                    return;
                }
            }

            console.log("Number of classes (filtering enabled): " + numberOfClasses);
        }

        $scope.enableCoverageTableFiltering();
    };

    self.initialize();
}

/* Angular application */
var coverageApp = angular.module('coverageApp', []);
coverageApp.controller('SummaryViewCtrl', SummaryViewCtrl);

coverageApp.directive('reactiveRiskHotspotTable', function () {
    return {
        restrict: 'A',
        scope: {
            riskHotspots: '=',
            riskHotspotMetrics: '='
        },
        link: function (scope, el, attrs) {
            scope.$watchCollection('riskHotspots', function (newValue, oldValue) {
                React.renderComponent(
                    RiskHotspotsComponent({ riskHotspots: newValue, riskHotspotMetrics: scope.riskHotspotMetrics }),
                    el[0]);
            });
        }
    };
});


coverageApp.directive('reactiveCoverageTable', function () {
    return {
        restrict: 'A',
        scope: {
            assemblies: '=',
            historicCoverageExecutionTimes: '=',
            branchCoverageAvailable: '='
        },
        link: function (scope, el, attrs) {
            scope.$watchCollection('assemblies', function (newValue, oldValue) {
                React.renderComponent(
                    AssemblyComponent({ assemblies: newValue, historicCoverageExecutionTimes: scope.historicCoverageExecutionTimes, branchCoverageAvailable: scope.branchCoverageAvailable }),
                    el[0]);
            });
        }
    };
});