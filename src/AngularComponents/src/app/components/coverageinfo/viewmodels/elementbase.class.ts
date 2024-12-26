import { CoverageInfoSettings } from "../data/coverageinfo-settings.class";
import { Helper } from "./helper.class";

export abstract class ElementBase {
    name: string = "";

    coveredLines: number = 0;
    uncoveredLines: number = 0;
    coverableLines: number = 0;
    totalLines: number = 0;

    coveredBranches: number = 0;
    totalBranches: number = 0;

    coveredMethods: number = 0;
    fullyCoveredMethods: number = 0;
    totalMethods: number = 0;

    get coverage(): number {
        if (this.coverableLines === 0) {
            return NaN;
        }

        return Helper.roundNumber(100 * this.coveredLines / this.coverableLines);
    }

    get coveragePercentage(): string {
        if (this.coverableLines === 0) {
            return "";
        }

        return this.coverage + "%";
    }

    get coverageRatioText(): string {
        if (this.coverableLines === 0) {
            return "-";
        }

        return this.coveredLines + "/" + this.coverableLines;
    }

    get branchCoverage(): number {
        if (this.totalBranches === 0) {
            return NaN;
        }

        return Helper.roundNumber(100 * this.coveredBranches / this.totalBranches);
    }

    get branchCoveragePercentage(): string {
        if (this.totalBranches === 0) {
            return "";
        }

        return this.branchCoverage + "%";
    }

    get branchCoverageRatioText(): string {
        if (this.totalBranches === 0) {
            return "-";
        }

        return this.coveredBranches + "/" + this.totalBranches;
    }

    get methodCoverage(): number {
        if (this.totalMethods === 0) {
            return NaN;
        }

        return Helper.roundNumber(100 * this.coveredMethods / this.totalMethods);
    }
    
    get methodCoveragePercentage(): string {
        if (this.totalMethods === 0) {
            return "";
        }

        return this.methodCoverage + "%";
    }

    get methodCoverageRatioText(): string {
        if (this.totalMethods === 0) {
            return "-";
        }

        return this.coveredMethods + "/" + this.totalMethods;
    }

    get methodFullCoverage(): number {
        if (this.totalMethods === 0) {
            return NaN;
        }

        return Helper.roundNumber(100 * this.fullyCoveredMethods / this.totalMethods);
    }
    
    get methodFullCoveragePercentage(): string {
        if (this.totalMethods === 0) {
            return "";
        }

        return this.methodFullCoverage + "%";
    }

    get methodFullCoverageRatioText(): string {
        if (this.totalMethods === 0) {
            return "-";
        }

        return this.fullyCoveredMethods + "/" + this.totalMethods;
    }

    abstract visible(settings: CoverageInfoSettings): boolean;

    abstract updateCurrentHistoricCoverage(historyComparisionDate: string): void;
}