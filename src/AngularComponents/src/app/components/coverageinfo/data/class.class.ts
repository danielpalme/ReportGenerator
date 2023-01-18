import { HistoricCoverage } from "./historic-coverage.class";

export class Class {
    /*
    * The name.
    */
    name: string = "";

    /*
    * The report path.
    */
    rp: string = "";

    /*
    * The coveredLines.
    */
    cl: number = 0;

    /*
    * The uncoveredLines.
    */
    ucl: number = 0;

    /*
    * The coverableLines.
    */
    cal: number = 0;

    /*
    * The totalLines.
    */
    tl: number = 0;

    /*
    * The coverageType.
    */
    ct: string = "";

    /*
    * The line coverage determined by method coverage.
    */
    cbm: string = "";

    /*
    * The coveredBranches.
    */
    cb: number = 0;

    /*
    * The totalBranches.
    */
    tb: number = 0;

    /*
    * The coveredMethods.
    */
    cm: number = 0;

    /*
    * The totalMethods.
    */
    tm: number = 0;

    /*
    * The lineCoverageHistory.
    */
    lch: number[] = [];

    /*
    * The branchCoverageHistory.
    */
    bch: number[] = [];

    /*
    * The methodCoverageHistory.
    */
    mch: number[] = [];

    /*
    * The historicCoverages.
    */
    hc: HistoricCoverage[] = [];

    /*
    * The metrics.
    */
    metrics: any;
}