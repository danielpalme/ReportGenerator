import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { Metric } from "./data/metric.class";
import { ClassViewModel } from "./viewmodels/class-viewmodel.class";

@Component({
    selector: "[class-row]",
    template: `
<td>
  @if (clazz.reportPath !== '') {
    <a [href]="clazz.reportPath">{{clazz.name}}</a>
  }
  @if (clazz.reportPath === '') {
    {{clazz.name}}
  }
</td>
@if (lineCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.coveredLines, clazz.currentHistoricCoverage.cl)}}">
        {{clazz.coveredLines}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et">
        {{clazz.currentHistoricCoverage.cl}}
      </div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.coveredLines}}
    }
  </td>
}
@if (lineCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.currentHistoricCoverage.ucl, clazz.uncoveredLines)}}">
        {{clazz.uncoveredLines}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et">
        {{clazz.currentHistoricCoverage.ucl}}
      </div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.uncoveredLines}}
    }
  </td>
}
@if (lineCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory">{{clazz.coverableLines}}</div>
      <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.cal}}</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.coverableLines}}
    }
  </td>
}
@if (lineCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory">{{clazz.totalLines}}</div>
      <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.tl}}</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.totalLines}}
    }
  </td>
}
@if (lineCoverageAvailable) {
  <td class="right" [title]="clazz.coverageRatioText">
    @if (clazz.lineCoverageHistory.length > 1) {
      <div coverage-history-chart [historicCoverages]="clazz.lineCoverageHistory"
        [ngClass]="{'historiccoverageoffset': clazz.currentHistoricCoverage !== null}"
        class="tinylinecoveragechart ct-chart" title="{{translations.history + ': ' + translations.coverage}}">
      </div>
    }
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.coverage, clazz.currentHistoricCoverage.lcq)}}">
        {{clazz.coveragePercentage}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et + ': ' + clazz.currentHistoricCoverage.coverageRatioText">{{clazz.currentHistoricCoverage.lcq}}%</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.coveragePercentage}}
    }
  </td>
}
@if (lineCoverageAvailable) {
  <td class="right"><coverage-bar [percentage]="clazz.coverage"></coverage-bar></td>
}
@if (branchCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.coveredBranches, clazz.currentHistoricCoverage.cb)}}">
        {{clazz.coveredBranches}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et">
        {{clazz.currentHistoricCoverage.cb}}
      </div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.coveredBranches}}
    }
  </td>
}

@if (branchCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory">{{clazz.totalBranches}}</div>
      <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.tb}}</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.totalBranches}}
    }
  </td>
}
@if (branchCoverageAvailable) {
  <td class="right" [title]="clazz.branchCoverageRatioText">
    @if (clazz.branchCoverageHistory.length > 1) {
      <div coverage-history-chart [historicCoverages]="clazz.branchCoverageHistory"
        [ngClass]="{'historiccoverageoffset': clazz.currentHistoricCoverage !== null}"
        class="tinybranchcoveragechart ct-chart" title="{{translations.history + ': ' + translations.branchCoverage}}">
      </div>
    }
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.branchCoverage, clazz.currentHistoricCoverage.bcq)}}">
        {{clazz.branchCoveragePercentage}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et + ': ' + clazz.currentHistoricCoverage.branchCoverageRatioText">{{clazz.currentHistoricCoverage.bcq}}%</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.branchCoveragePercentage}}
    }
  </td>
}
@if (branchCoverageAvailable) {
  <td class="right"><coverage-bar [percentage]="clazz.branchCoverage"></coverage-bar></td>
}

@if (methodCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.coveredMethods, clazz.currentHistoricCoverage.cm)}}">
        {{clazz.coveredMethods}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et">
        {{clazz.currentHistoricCoverage.cm}}
      </div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.coveredMethods}}
    }
  </td>
}
@if (methodCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory">{{clazz.totalMethods}}</div>
      <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.tm}}</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.totalMethods}}
    }
  </td>
}
@if (methodCoverageAvailable) {
  <td class="right" [title]="clazz.methodCoverageRatioText">
    @if (clazz.methodCoverageHistory.length > 1) {
      <div coverage-history-chart [historicCoverages]="clazz.methodCoverageHistory"
        [ngClass]="{'historiccoverageoffset': clazz.currentHistoricCoverage !== null}"
        class="tinymethodcoveragechart ct-chart" title="{{translations.history + ': ' + translations.methodCoverage}}">
      </div>
    }
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.methodCoverage, clazz.currentHistoricCoverage.mcq)}}">
        {{clazz.methodCoveragePercentage}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et + ': ' + clazz.currentHistoricCoverage.methodCoverageRatioText">{{clazz.currentHistoricCoverage.mcq}}%</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.methodCoveragePercentage}}
    }
  </td>
}
@if (methodCoverageAvailable) {
  <td class="right"><coverage-bar [percentage]="clazz.methodCoverage"></coverage-bar></td>
}

@if (methodFullCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.fullyCoveredMethods, clazz.currentHistoricCoverage.fcm)}}">
        {{clazz.fullyCoveredMethods}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et">
        {{clazz.currentHistoricCoverage.fcm}}
      </div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.fullyCoveredMethods}}
    }
  </td>
}
@if (methodFullCoverageAvailable) {
  <td class="right">
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory">{{clazz.totalMethods}}</div>
      <div [title]="clazz.currentHistoricCoverage.et">{{clazz.currentHistoricCoverage.tm}}</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.totalMethods}}
    }
  </td>
}
@if (methodFullCoverageAvailable) {
  <td class="right" [title]="clazz.methodFullCoverageRatioText">
    @if (clazz.methodFullCoverageHistory.length > 1) {
      <div coverage-history-chart [historicCoverages]="clazz.methodFullCoverageHistory"
        [ngClass]="{'historiccoverageoffset': clazz.currentHistoricCoverage !== null}"
        class="tinyfullmethodcoveragechart ct-chart" title="{{translations.history + ': ' + translations.fullMethodCoverage}}">
      </div>
    }
    @if (clazz.currentHistoricCoverage !== null) {
      <div class="currenthistory {{getClassName(clazz.methodFullCoverage, clazz.currentHistoricCoverage.mfcq)}}">
        {{clazz.methodFullCoveragePercentage}}
      </div>
      <div [title]="clazz.currentHistoricCoverage.et + ': ' + clazz.currentHistoricCoverage.methodFullCoverageRatioText">{{clazz.currentHistoricCoverage.mfcq}}%</div>
    }
    @if (clazz.currentHistoricCoverage === null) {
      {{clazz.methodFullCoveragePercentage}}
    }
  </td>
}
@if (methodFullCoverageAvailable) {
  <td class="right"><coverage-bar [percentage]="clazz.methodFullCoverage"></coverage-bar></td>
}
@for (metric of visibleMetrics; track metric) {
  <td class="right">{{ clazz.metrics[metric.abbreviation] }}</td>
}`,
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