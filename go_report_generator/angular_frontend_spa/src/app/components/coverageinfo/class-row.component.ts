import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { Metric } from "./data/metric.class";
import { ClassViewModel } from "./viewmodels/class-viewmodel.class";

@Component({
    selector: "[class-row]",
    template: `
<td>
  <a [href]="clazz.reportPath" *ngIf="clazz.reportPath !== ''">{{clazz.name}}</a>
  <ng-container *ngIf="clazz.reportPath === ''">{{clazz.name}}</ng-container>
</td>
<td class="right" *ngIf="lineCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.coveredLines, clazz.currentHistoricCoverage.cl)}}">
      {{clazz.coveredLines}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et">
      {{clazz.currentHistoricCoverage.cl}}
    </div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.coveredLines}}
  </ng-container>
</td>
<td class="right" *ngIf="lineCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.currentHistoricCoverage.ucl, clazz.uncoveredLines)}}">
      {{clazz.uncoveredLines}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et">
      {{clazz.currentHistoricCoverage.ucl}}
    </div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.uncoveredLines}}
  </ng-container>
</td>
<td class="right" *ngIf="lineCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory">{{clazz.coverableLines}}</div>
    <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.cal}}</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.coverableLines}}
  </ng-container>
</td>
<td class="right" *ngIf="lineCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory">{{clazz.totalLines}}</div>
    <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.tl}}</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.totalLines}}
  </ng-container>
</td>
<td class="right" [title]="clazz.coverageRatioText" *ngIf="lineCoverageAvailable">
  <div coverage-history-chart [historicCoverages]="clazz.lineCoverageHistory"
    *ngIf="clazz.lineCoverageHistory.length > 1"
    [ngClass]="{'historiccoverageoffset': clazz.currentHistoricCoverage !== null}"
    class="tinylinecoveragechart ct-chart" title="{{translations.history + ': ' + translations.coverage}}">
  </div>
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.coverage, clazz.currentHistoricCoverage.lcq)}}">
      {{clazz.coveragePercentage}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et + ': ' + clazz.currentHistoricCoverage.coverageRatioText">{{clazz.currentHistoricCoverage.lcq}}%</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.coveragePercentage}}
  </ng-container>
</td>
<td class="right" *ngIf="lineCoverageAvailable"><coverage-bar [percentage]="clazz.coverage"></coverage-bar></td>
<td class="right" *ngIf="branchCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.coveredBranches, clazz.currentHistoricCoverage.cb)}}">
      {{clazz.coveredBranches}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et">
      {{clazz.currentHistoricCoverage.cb}}
    </div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.coveredBranches}}
  </ng-container>
</td>

<td class="right" *ngIf="branchCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory">{{clazz.totalBranches}}</div>
    <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.tb}}</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.totalBranches}}
  </ng-container>
</td>
<td class="right" *ngIf="branchCoverageAvailable" [title]="clazz.branchCoverageRatioText">
  <div coverage-history-chart [historicCoverages]="clazz.branchCoverageHistory"
    *ngIf="clazz.branchCoverageHistory.length > 1"
    [ngClass]="{'historiccoverageoffset': clazz.currentHistoricCoverage !== null}"
    class="tinybranchcoveragechart ct-chart" title="{{translations.history + ': ' + translations.branchCoverage}}">
  </div>
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.branchCoverage, clazz.currentHistoricCoverage.bcq)}}">
      {{clazz.branchCoveragePercentage}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et + ': ' + clazz.currentHistoricCoverage.branchCoverageRatioText">{{clazz.currentHistoricCoverage.bcq}}%</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.branchCoveragePercentage}}
  </ng-container>
</td>
<td class="right" *ngIf="branchCoverageAvailable"><coverage-bar [percentage]="clazz.branchCoverage"></coverage-bar></td>

<td class="right" *ngIf="methodCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.coveredMethods, clazz.currentHistoricCoverage.cm)}}">
      {{clazz.coveredMethods}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et">
      {{clazz.currentHistoricCoverage.cm}}
    </div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.coveredMethods}}
  </ng-container>
</td>
<td class="right" *ngIf="methodCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory">{{clazz.totalMethods}}</div>
    <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.tm}}</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.totalMethods}}
  </ng-container>
</td>
<td class="right" *ngIf="methodCoverageAvailable" [title]="clazz.methodCoverageRatioText">
  <div coverage-history-chart [historicCoverages]="clazz.methodCoverageHistory"
    *ngIf="clazz.methodCoverageHistory.length > 1"
    [ngClass]="{'historiccoverageoffset': clazz.currentHistoricCoverage !== null}"
    class="tinymethodcoveragechart ct-chart" title="{{translations.history + ': ' + translations.methodCoverage}}">
  </div>
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.methodCoverage, clazz.currentHistoricCoverage.mcq)}}">
      {{clazz.methodCoveragePercentage}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et + ': ' + clazz.currentHistoricCoverage.methodCoverageRatioText">{{clazz.currentHistoricCoverage.mcq}}%</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.methodCoveragePercentage}}
  </ng-container>
</td>
<td class="right" *ngIf="methodCoverageAvailable"><coverage-bar [percentage]="clazz.methodCoverage"></coverage-bar></td>

<td class="right" *ngIf="methodFullCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.fullyCoveredMethods, clazz.currentHistoricCoverage.fcm)}}">
      {{clazz.fullyCoveredMethods}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et">
      {{clazz.currentHistoricCoverage.fcm}}
    </div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.fullyCoveredMethods}}
  </ng-container>
</td>
<td class="right" *ngIf="methodFullCoverageAvailable">
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory">{{clazz.totalMethods}}</div>
    <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.tm}}</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.totalMethods}}
  </ng-container>
</td>
<td class="right" *ngIf="methodFullCoverageAvailable" [title]="clazz.methodFullCoverageRatioText">
  <div coverage-history-chart [historicCoverages]="clazz.methodFullCoverageHistory"
    *ngIf="clazz.methodFullCoverageHistory.length > 1"
    [ngClass]="{'historiccoverageoffset': clazz.currentHistoricCoverage !== null}"
    class="tinyfullmethodcoveragechart ct-chart" title="{{translations.history + ': ' + translations.fullMethodCoverage}}">
  </div>
  <ng-container *ngIf="clazz.currentHistoricCoverage !== null">
    <div class="currenthistory {{getClassName(clazz.methodFullCoverage, clazz.currentHistoricCoverage.mfcq)}}">
      {{clazz.methodFullCoveragePercentage}}
    </div>
    <div [title]="clazz.currentHistoricCoverage.et + ': ' + clazz.currentHistoricCoverage.methodFullCoverageRatioText">{{clazz.currentHistoricCoverage.mfcq}}%</div>
  </ng-container>
  <ng-container *ngIf="clazz.currentHistoricCoverage === null">
    {{clazz.methodFullCoveragePercentage}}
  </ng-container>
</td>
<td class="right" *ngIf="methodFullCoverageAvailable"><coverage-bar [percentage]="clazz.methodFullCoverage"></coverage-bar></td>
<td class="right" *ngFor="let metric of visibleMetrics">{{ clazz.metrics[metric.abbreviation] }}</td>`,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class ClassRow {
    @Input() clazz!: ClassViewModel;

    @Input() translations: any = { };

    @Input() lineCoverageAvailable: boolean = false;
    
    @Input() branchCoverageAvailable: boolean = false;

    @Input() methodCoverageAvailable: boolean = false;

    @Input() methodFullCoverageAvailable: boolean = false;

    @Input() visibleMetrics: Metric[] = [];

    @Input() historyComparisionDate: string = "";

    getClassName(current: number, history: number): string {
      if (current > history) {
          return "lightgreen";
      } else if (current < history) {
          return "lightred";
      } else {
          return "lightgraybg";
      }
    }
}