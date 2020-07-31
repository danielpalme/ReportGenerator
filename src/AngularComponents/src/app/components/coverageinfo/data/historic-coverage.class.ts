export class HistoricCoverage {
    /*
    * The execution time.
    */
     et: string = "";

    /*
    * The coveredLines.
    */
    cl: number;

    /*
    * The uncoveredLines.
    */
    ucl: number;

    /*
    * The coverableLines.
    */
    cal: number;

    /*
    * The totalLines.
    */
    tl: number;

    /*
    * The coverageQuota.
    */
    lcq: number;

    /*
    * The coveredBranches.
    */
    cb: number;

    /*
    * The totalBranches.
    */
    tb: number;

    /*
    * The branchCoverageQuota.
    */
    bcq: number;

    constructor(hc: HistoricCoverage) {
        this.et = hc.et;
        this.cl = hc.cl;
        this.ucl = hc.ucl;
        this.cal = hc.cal;
        this.tl = hc.tl;
        this.lcq = hc.lcq;
        this.cb = hc.cb;
        this.tb = hc.tb;
        this.bcq = hc.bcq;
    }

    get coverageRatioText(): string {
        if (this.tl === 0) {
            return "-";
        }

        return this.cl + "/" + this.cal;
    }

    get branchCoverageRatioText(): string {
        if (this.tb === 0) {
            return "-";
        }

        return this.cb + "/" + this.tb;
    }
}