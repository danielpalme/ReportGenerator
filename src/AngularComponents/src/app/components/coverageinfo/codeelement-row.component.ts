import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { CodeElementViewModel } from "./viewmodels/codelement-viewmodel.class";

@Component({
  selector: "[codeelement-row]",
  template: `
<th><a href="#" (click)="element.toggleCollapse($event)">
  <i [ngClass]="{'icon-plus': element.collapsed, 'icon-minus': !element.collapsed}"></i>
  {{element.name}}</a>
</th>
<th class="right">{{element.coveredLines}}</th>
<th class="right">{{element.uncoveredLines}}</th>
<th class="right">{{element.coverableLines}}</th>
<th class="right">{{element.totalLines}}</th>
<th class="right" [title]="element.coverageRatioText">{{element.coveragePercentage}}</th>
<th class="right"><coverage-bar [percentage]="element.coverage"></coverage-bar></th>
<th class="right" *ngIf="branchCoverageAvailable">{{element.coveredBranches}}</th>
<th class="right" *ngIf="branchCoverageAvailable">{{element.totalBranches}}</th>
<th class="right" *ngIf="branchCoverageAvailable" [title]="element.branchCoverageRatioText">{{element.branchCoveragePercentage}}</th>
<th class="right" *ngIf="branchCoverageAvailable">
  <coverage-bar [percentage]="element.branchCoverage"></coverage-bar>
</th>`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CodeElementRow {
    @Input() element: CodeElementViewModel = null;

    @Input() collapsed: boolean = false;

    @Input() branchCoverageAvailable: boolean = false;
}