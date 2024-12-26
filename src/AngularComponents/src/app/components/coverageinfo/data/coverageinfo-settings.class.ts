import { Metric } from "./metric.class";

export class CoverageInfoSettings {
    showLineCoverage: boolean = true;
    showBranchCoverage: boolean = true;
    showMethodCoverage: boolean = true;
    showFullMethodCoverage: boolean = true;
    visibleMetrics: Metric[] = [];

    groupingMaximum: number = 0;
    grouping: number = 0;
    historyComparisionDate: string = "";
    historyComparisionType: string = "";

    filter: string = "";
    lineCoverageMin: number = 0;
    lineCoverageMax: number = 100;
    branchCoverageMin: number = 0;
    branchCoverageMax: number = 100;
    methodCoverageMin: number = 0;
    methodCoverageMax: number = 100;
    methodFullCoverageMin: number = 0;
    methodFullCoverageMax: number = 100;

    sortBy: string = "name";
    sortOrder: string = "asc";

    collapseStates: boolean[] = [];
}
