import { Metric } from "./metric.class";

export class CoverageInfoSettings {
    showLineCoverage: boolean = true;
    showBranchCoverage: boolean = true;
    showMethodCoverage: boolean = true;
    visibleMetrics: Metric[] = [];

    groupingMaximum: number = 0;
    grouping: number = 0;
    historyComparisionDate: string = "";
    historyComparisionType: string = "";
    filter: string = "";

    sortBy: string = "name";
    sortOrder: string = "asc";

    collapseStates: boolean[] = [];
}
