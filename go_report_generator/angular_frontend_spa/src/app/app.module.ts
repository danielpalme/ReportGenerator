import { NgxSliderModule } from "@angular-slider/ngx-slider";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { ClassRow } from "./components/coverageinfo/class-row.component";
import { CodeElementRow } from "./components/coverageinfo/codeelement-row.component";
import { CoverageBarComponent } from "./components/coverageinfo/coverage-bar.component";
import { CoverageHistoryChartComponent } from "./components/coverageinfo/coverage-history-chart.component";
import { CoverageInfoComponent } from "./components/coverageinfo/coverage-info.component";
import { PopupComponent } from "./components/coverageinfo/popup.component";
import { ProButton } from "./components/coverageinfo/pro-button.component";
import { RiskHotspotsComponent } from "./components/riskhotspots/riskhotspots.component";
import { WindowRefService } from "./infrastructure/windowref.service";

@NgModule({
  declarations: [
    RiskHotspotsComponent,
    CoverageInfoComponent,
    PopupComponent,
    CodeElementRow,
    ClassRow,
    CoverageHistoryChartComponent,
    CoverageBarComponent,
    ProButton
  ],
  imports: [
    BrowserModule,
    FormsModule,
    NgxSliderModule
  ],
  providers: [WindowRefService],
  bootstrap: [RiskHotspotsComponent, CoverageInfoComponent]
})
export class AppModule { }
