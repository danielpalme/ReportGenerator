import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { Metric } from "./data/metric.class";
import { CodeElementViewModel } from "./viewmodels/codelement-viewmodel.class";

@Component({
    selector: "[codeelement-row]",
    template: `
<th><a href="#" (click)="element.toggleCollapse($event)">
  <i [ngClass]="{'icon-plus': element.collapsed, 'icon-minus': !element.collapsed}"></i>
  {{element.name}}</a>
</th>
<th class="right" *ngIf="lineCoverageAvailable">{{element.coveredLines}}</th>
<th class="right" *ngIf="lineCoverageAvailable">{{element.uncoveredLines}}</th>
<th class="right" *ngIf="lineCoverageAvailable">{{element.coverableLines}}</th>
<th class="right" *ngIf="lineCoverageAvailable">{{element.totalLines}}</th>
<th class="right" [title]="element.coverageRatioText" *ngIf="lineCoverageAvailable">{{element.coveragePercentage}}</th>
<th class="right" *ngIf="lineCoverageAvailable"><coverage-bar [percentage]="element.coverage"></coverage-bar></th>
<th class="right" *ngIf="branchCoverageAvailable">{{element.coveredBranches}}</th>
<th class="right" *ngIf="branchCoverageAvailable">{{element.totalBranches}}</th>
<th class="right" *ngIf="branchCoverageAvailable" [title]="element.branchCoverageRatioText">{{element.branchCoveragePercentage}}</th>
<th class="right" *ngIf="branchCoverageAvailable">
  <coverage-bar [percentage]="element.branchCoverage"></coverage-bar>
</th>
<th class="right" *ngIf="methodCoverageAvailable">{{element.coveredMethods}}</th>
<th class="right" *ngIf="methodCoverageAvailable">{{element.totalMethods}}</th>
<th class="right" *ngIf="methodCoverageAvailable" [title]="element.methodCoverageRatioText">{{element.methodCoveragePercentage}}</th>
<th class="right" *ngIf="methodCoverageAvailable">
  <coverage-bar [percentage]="element.methodCoverage"></coverage-bar>
</th>
<th class="right" *ngIf="methodFullCoverageAvailable">{{element.fullyCoveredMethods}}</th>
<th class="right" *ngIf="methodFullCoverageAvailable">{{element.totalMethods}}</th>
<th class="right" *ngIf="methodFullCoverageAvailable" [title]="element.methodFullCoverageRatioText">{{element.methodFullCoveragePercentage}}</th>
<th class="right" *ngIf="methodFullCoverageAvailable">
  <coverage-bar [percentage]="element.methodFullCoverage"></coverage-bar>
</th>
<th class="right" *ngFor="let metric of visibleMetrics"></th>`,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class CodeElementRow {
    @Input() element!: CodeElementViewModel;

    @Input() collapsed: boolean = false;

    @Input() lineCoverageAvailable: boolean = false;

    @Input() branchCoverageAvailable: boolean = false;

    @Input() methodCoverageAvailable: boolean = false;

    @Input() methodFullCoverageAvailable: boolean = false;
    
    @Input() visibleMetrics: Metric[] = [];
}