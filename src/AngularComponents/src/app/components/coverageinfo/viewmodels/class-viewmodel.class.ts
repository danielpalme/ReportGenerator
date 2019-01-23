import { Class } from "../data/class.class";
import { HistoricCoverage } from "../data/historic-coverage.class";
import { ElementBase } from "./elementbase.class";
import { Helper } from "./helper.class";

export class ClassViewModel extends ElementBase {
    reportPath: string = "";
    _coverageType: string = "";
    methodCoverage: string = "";
    lineCoverageHistory: number[] = [];
    branchCoverageHistory: number[] = [];
    historicCoverages: HistoricCoverage[] = [];

    currentHistoricCoverage: HistoricCoverage = null;

    constructor(
        clazz: Class) {
            super();
            this.name = clazz.name;
            this.reportPath = clazz.rp;

            this.coveredLines = clazz.cl;
            this.uncoveredLines = clazz.ucl;
            this.coverableLines = clazz.cal;
            this.totalLines = clazz.tl;

            this._coverageType = clazz.ct;
            this.methodCoverage = clazz.mc;

            this.coveredBranches = clazz.cb;
            this.totalBranches = clazz.tb;

            this.lineCoverageHistory = clazz.lch;
            this.branchCoverageHistory = clazz.bch;
            this.historicCoverages = clazz.hc;
    }

    get coverage(): number {
        if (this.coverableLines === 0) {
            if (this.methodCoverage !== "-") {
                return parseFloat(this.methodCoverage);
            }

            return NaN;
        }

        return Helper.roundNumber(100 * this.coveredLines / this.coverableLines, 1);
    }

    get coverageType(): string {
        if (this.coverableLines === 0) {
            if (this.methodCoverage !== "-") {
                return this._coverageType;
            }

            return "";
        }

        return this._coverageType;
    }

    visible(filter: string, historicCoverageFilter: string): boolean {
        if (filter !== "" && this.name.toLowerCase().indexOf(filter.toLowerCase()) === -1) {
            return false;
        }

        if (historicCoverageFilter === "" || this.currentHistoricCoverage === null) {
            return true;
        }

        if (historicCoverageFilter === "allChanges") {
            if (this.coveredLines === this.currentHistoricCoverage.cl
                && this.uncoveredLines === this.currentHistoricCoverage.ucl
                && this.coverableLines === this.currentHistoricCoverage.cal
                && this.totalLines === this.currentHistoricCoverage.tl
                && this.coveredBranches === this.currentHistoricCoverage.cb
                && this.totalBranches === this.currentHistoricCoverage.tb) {
                return false;
            }
        } else if (historicCoverageFilter === "lineCoverageIncreaseOnly") {
            let coverage: number = this.coverage;
            if (isNaN(coverage) || coverage <= this.currentHistoricCoverage.lcq) {
                return false;
            }
        } else if (historicCoverageFilter === "lineCoverageDecreaseOnly") {
            let coverage: number = this.coverage;
            if (isNaN(coverage) || coverage >= this.currentHistoricCoverage.lcq) {
                return false;
            }
        } else if (historicCoverageFilter === "branchCoverageIncreaseOnly") {
            let branchCoverage: number = this.branchCoverage;
            if (isNaN(branchCoverage) || branchCoverage <= this.currentHistoricCoverage.bcq) {
                return false;
            }
        } else if (historicCoverageFilter === "branchCoverageDecreaseOnly") {
            let branchCoverage: number = this.branchCoverage;
            if (isNaN(branchCoverage) || branchCoverage >= this.currentHistoricCoverage.bcq) {
                return false;
            }
        }

        return true;
    }

    updateCurrentHistoricCoverage(historyComparisionDate: string): void {
        this.currentHistoricCoverage = null;

        if (historyComparisionDate !== "") {
            for (let i: number = 0; i < this.historicCoverages.length; i++) {

                if (this.historicCoverages[i].et === historyComparisionDate) {
                    this.currentHistoricCoverage = this.historicCoverages[i];
                    break;
                }
            }
        }
    }
}