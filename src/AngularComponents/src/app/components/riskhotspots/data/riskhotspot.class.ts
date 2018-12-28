import { Metric } from "./metric.class";

export class RiskHotspot {
    assembly: string;
    class: string;
    reportPath: string;
    methodName: string;
    methodShortName: string;
    fileIndex: number;
    line: number;
    metrics: Metric[] = [];
}