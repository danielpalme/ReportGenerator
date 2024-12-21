import { ChangeDetectionStrategy, Component, Input } from "@angular/core";

@Component({
    selector: "coverage-bar",
    template: `
  <table class="coverage">
    <td class="gray covered100" *ngIf="grayVisible"> </td>
    <td class="green {{greenClass}}" *ngIf="greenVisible"> </td>
    <td class="red {{redClass}}" *ngIf="redVisible"> </td>
  </table>`,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class CoverageBarComponent {
    grayVisible: boolean = true;
    greenVisible: boolean = false;
    redVisible: boolean = false;

    greenClass: string = "";
    redClass: string = "";

    _percentage: number = NaN;
    get percentage(): number {
        return this._percentage;
    }

    @Input("percentage")
    set percentage(value: number) {
        this._percentage = value;

        this.grayVisible = isNaN(value);
        this.greenVisible = !isNaN(value) && Math.round(value) > 0;
        this.redVisible = !isNaN(value) && (100 - Math.round(value)) > 0;

        this.greenClass = "covered" + Math.round(value);
        this.redClass = "covered" + (100 - Math.round(value));
    }
}