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

    /*
    * The coveredMethods.
    */
    cm: number;

    /*
    * The full coveredMethods.
    */
    fcm: number;

    /*
    * The totalMethods.
    */
    tm: number;

    /*
    * The methodCoverageQuota.
    */
    mcq: number;

    /*
    * The full methodCoverageQuota.
    */
    mfcq: number;

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
        this.cm = hc.cm;
        this.fcm = hc.fcm;
        this.tm = hc.tm;
        this.mcq = hc.mcq;
        this.mfcq = hc.mfcq;
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

    get methodCoverageRatioText(): string {
        if (this.tm === 0) {
            return "-";
        }

        return this.cm + "/" + this.tm;
    }

    get methodFullCoverageRatioText(): string {
        if (this.tm === 0) {
            return "-";
        }

        return this.fcm + "/" + this.tm;
    }
}