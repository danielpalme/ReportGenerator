import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { Helper } from './viewmodels/helper.class';

@Component({
    selector: "[coverage-history-chart]",
    template: `
  <svg width="30" height="18" class="ct-chart-line">
    <g class="ct-series ct-series-a">
        <path [attr.d]="path" class="ct-line"></path>
    </g>
  </svg>`,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class CoverageHistoryChartComponent {
    path: string|null = null;

    _historicCoverages: number[] = [];
    get historicCoverages(): number[] {
        return this._historicCoverages;
    }

    @Input("historicCoverages")
    set historicCoverages(value: number[]) {
        this._historicCoverages = value;

        if (value.length > 1) {
            let path: string = "";

            for (let i: number = 0; i < value.length; i++) {
                path += `${i === 0 ? "M" : "L"}`;
                path += `${Helper.roundNumber(30 * i / (value.length - 1))}`;
                path += `,${Helper.roundNumber(18 - (18 * value[i] / 100))}`;
            }

            this.path = path;
        } else {
            this.path = null;
        }
    }
}