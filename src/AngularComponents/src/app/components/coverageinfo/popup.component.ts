import { Component, EventEmitter, Input, Output } from "@angular/core";
import { Metric } from "./data/metric.class";

@Component({
    selector: "popup",
    template: `
<div class="popup-container" (click)="close()">
  <div class="popup" (click)="cancelEvent($event)">
    <div class="close" (click)="close()">X</div>
    <b>{{translations.coverageTypes}}</b>
    <div class="mt-1">
      <label><input type="checkbox" [(ngModel)]="showLineCoverage" (change)="showLineCoverageChange.emit(this.showLineCoverage)" /> {{ translations.coverage }}</label>
    </div>
    @if (branchCoverageAvailable) {
      <div class="mt-1">
        <label><input type="checkbox" [(ngModel)]="showBranchCoverage" (change)="showBranchCoverageChange.emit(this.showBranchCoverage)" /> {{ translations.branchCoverage }}</label>
      </div>
    }
    <div class="mt-1">
      <label><input type="checkbox" [(ngModel)]="showMethodCoverage" (change)="showMethodCoverageChange.emit(this.showMethodCoverage)" [disabled]="!methodCoverageAvailable" /> {{ translations.methodCoverage }}</label>@if (!methodCoverageAvailable) {
      <pro-button [translations]="translations"></pro-button>
    }
  </div>
  <div class="mt-1">
    <label><input type="checkbox" [(ngModel)]="showMethodFullCoverage" (change)="showMethodFullCoverageChange.emit(this.showMethodFullCoverage)" [disabled]="!methodCoverageAvailable" /> {{ translations.fullMethodCoverage }}</label>@if (!methodCoverageAvailable) {
    <pro-button [translations]="translations"></pro-button>
  }
</div>
@if (metrics.length > 0) {
  <br />
  <br/>
  <b>{{translations.metrics}}</b>@if (!methodCoverageAvailable) {
  <pro-button [translations]="translations"></pro-button>
}
@for (metric of metrics; track metric) {
  <div class="mt-1">
    <label><input type="checkbox" [checked]="isMetricSelected(metric)" (change)="toggleMetric(metric)" [disabled]="!methodCoverageAvailable" /> {{ metric.name }}</label>&nbsp;@if (metric.explanationUrl) {
    <a [href]="metric.explanationUrl" target="_blank"><i class="icon-info-circled"></i></a>
  }
</div>
}
}
</div>
</div>`,
    standalone: false
})
export class PopupComponent {
    @Input() visible: boolean = false;
    @Output() visibleChange = new EventEmitter<boolean>();

    @Input() translations: any = { };

    @Input() branchCoverageAvailable: boolean = false;
    
    @Input() methodCoverageAvailable: boolean = false;

    @Input() metrics: Metric[] = [];

    @Input() showLineCoverage: boolean = false;
    @Output() showLineCoverageChange = new EventEmitter<boolean>();

    @Input() showBranchCoverage: boolean = false;
    @Output() showBranchCoverageChange = new EventEmitter<boolean>();
    
    @Input() showMethodCoverage: boolean = false;
    @Output() showMethodCoverageChange = new EventEmitter<boolean>();
    
    @Input() showMethodFullCoverage: boolean = false;
    @Output() showMethodFullCoverageChange = new EventEmitter<boolean>();
    
    @Input() visibleMetrics: Metric[] = [];
    @Output() visibleMetricsChange = new EventEmitter<Metric[]>();

    isMetricSelected(metric: Metric): boolean {
      return this.visibleMetrics.find(m => m.name === metric.name) !== undefined;
    }

    toggleMetric(metric: Metric) {
      let match = this.visibleMetrics.find(m => m.name === metric.name);

      if (match) {
        this.visibleMetrics.splice(this.visibleMetrics.indexOf(match), 1);
      } else {
        this.visibleMetrics.push(metric);
      }

      this.visibleMetrics = [...this.visibleMetrics];

      this.visibleMetricsChange.emit(this.visibleMetrics);
    }

    close() {
      this.visible = false; 
      this.visibleChange.emit(this.visible);
    }

    cancelEvent($event: Event) {
      $event.stopPropagation();
    }
}