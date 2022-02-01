import { Class } from "../data/class.class";
import { HistoricCoverage } from "../data/historic-coverage.class";
import { ElementBase } from "./elementbase.class";
import { Helper } from "./helper.class";

export class ClassViewModel extends ElementBase {
    reportPath: string = "";
    _coverageType: string = "";
    coverageByMethod: string = "";
    lineCoverageHistory: number[] = [];
    branchCoverageHistory: number[] = [];
    methodCoverageHistory: number[] = [];
    historicCoverages: HistoricCoverage[] = [];

    currentHistoricCoverage: HistoricCoverage|null = null;

    constructor(
        clazz: Class,
        queryString: string) {
            super();
            this.name = clazz.name;
            this.reportPath = clazz.rp ? clazz.rp + queryString : clazz.rp;

            this.coveredLines = clazz.cl;
            this.uncoveredLines = clazz.ucl;
            this.coverableLines = clazz.cal;
            this.totalLines = clazz.tl;

            this._coverageType = clazz.ct;
            this.coverageByMethod = clazz.cbm;

            this.coveredBranches = clazz.cb;
            this.totalBranches = clazz.tb;

            this.coveredMethods = clazz.cm;
            this.totalMethods = clazz.tm;

            this.lineCoverageHistory = clazz.lch;
            this.branchCoverageHistory = clazz.bch;
            this.methodCoverageHistory = clazz.mch;

            clazz.hc.forEach(element => {
                this.historicCoverages.push(new HistoricCoverage(element))
            });
    }

    override get coverage(): number {
        if (this.coverableLines === 0) {
            if (this.coverageByMethod !== "-") {
                return parseFloat(this.coverageByMethod);
            }

            return NaN;
        }

        return Helper.roundNumber(100 * this.coveredLines / this.coverableLines, 1);
    }

    get coverageType(): string {
        if (this.coverableLines === 0) {
            if (this.coverageByMethod !== "-") {
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
                && this.totalBranches === this.currentHistoricCoverage.tb
                && this.coveredMethods === this.currentHistoricCoverage.cm
                && this.totalMethods === this.currentHistoricCoverage.tm) {
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
        } else if (historicCoverageFilter === "methodCoverageIncreaseOnly") {
            let methodCoverage: number = this.methodCoverage;
            if (isNaN(methodCoverage) || methodCoverage <= this.currentHistoricCoverage.mcq) {
                return false;
            }
        } else if (historicCoverageFilter === "methodCoverageDecreaseOnly") {
            let methodCoverage: number = this.methodCoverage;
            if (isNaN(methodCoverage) || methodCoverage >= this.currentHistoricCoverage.mcq) {
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