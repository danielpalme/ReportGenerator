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
@if (lineCoverageAvailable) {
  <th class="right">{{element.coveredLines}}</th>
}
@if (lineCoverageAvailable) {
  <th class="right">{{element.uncoveredLines}}</th>
}
@if (lineCoverageAvailable) {
  <th class="right">{{element.coverableLines}}</th>
}
@if (lineCoverageAvailable) {
  <th class="right">{{element.totalLines}}</th>
}
@if (lineCoverageAvailable) {
  <th class="right" [title]="element.coverageRatioText">{{element.coveragePercentage}}</th>
}
@if (lineCoverageAvailable) {
  <th class="right"><coverage-bar [percentage]="element.coverage"></coverage-bar></th>
}
@if (branchCoverageAvailable) {
  <th class="right">{{element.coveredBranches}}</th>
}
@if (branchCoverageAvailable) {
  <th class="right">{{element.totalBranches}}</th>
}
@if (branchCoverageAvailable) {
  <th class="right" [title]="element.branchCoverageRatioText">{{element.branchCoveragePercentage}}</th>
}
@if (branchCoverageAvailable) {
  <th class="right">
    <coverage-bar [percentage]="element.branchCoverage"></coverage-bar>
  </th>
}
@if (methodCoverageAvailable) {
  <th class="right">{{element.coveredMethods}}</th>
}
@if (methodCoverageAvailable) {
  <th class="right">{{element.totalMethods}}</th>
}
@if (methodCoverageAvailable) {
  <th class="right" [title]="element.methodCoverageRatioText">{{element.methodCoveragePercentage}}</th>
}
@if (methodCoverageAvailable) {
  <th class="right">
    <coverage-bar [percentage]="element.methodCoverage"></coverage-bar>
  </th>
}
@if (methodFullCoverageAvailable) {
  <th class="right">{{element.fullyCoveredMethods}}</th>
}
@if (methodFullCoverageAvailable) {
  <th class="right">{{element.totalMethods}}</th>
}
@if (methodFullCoverageAvailable) {
  <th class="right" [title]="element.methodFullCoverageRatioText">{{element.methodFullCoveragePercentage}}</th>
}
@if (methodFullCoverageAvailable) {
  <th class="right">
    <coverage-bar [percentage]="element.methodFullCoverage"></coverage-bar>
  </th>
}
@for (metric of visibleMetrics; track metric) {
  <th class="right"></th>
}`,
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