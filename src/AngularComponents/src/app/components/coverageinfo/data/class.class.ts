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
    * The coverageType.
    */
    ct: string = "";

    /*
    * The methodCoverage.
    */
    mc: string = "";

    /*
    * The coveredBranches.
    */
    cb: number;

    /*
    * The totalBranches.
    */
    tb: number;

    /*
    * The lineCoverageHistory.
    */
    lch: number[] = [];

    /*
    * The branchCoverageHistory.
    */
    bch: number[] = [];

    /*
    * The historicCoverages.
    */
    hc: HistoricCoverage[] = [];
}