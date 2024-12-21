import { Component, HostListener, OnInit } from "@angular/core";
import { WindowRefService } from "../../infrastructure/windowref.service";
import { GlobalHistoryState } from "../globalhistorystate";
import { RiskHotspotMetric } from "./data/riskhotspot-metric.class";
import { RiskHotspot } from "./data/riskhotspot.class";
import { RiskHotspotsSettings } from "./data/riskhotspots-settings.class";

@Component({
    selector: "risk-hotspots",
    template: `
    <div *ngIf="totalNumberOfRiskHotspots > 0">
      <div class="customizebox">
        <div>
          <select [(ngModel)]="settings.assembly" name="assembly" (ngModelChange)="updateRiskHotpots()">
            <option value="">{{translations.assembly}}</option>
            <option *ngFor="let assembly of assemblies" [value]="assembly">{{assembly}}</option>
          </select>
        </div>
        <div class="col-center">
          <span *ngIf="totalNumberOfRiskHotspots > 10">{{translations.top}}</span>
          <select [(ngModel)]="settings.numberOfRiskHotspots" *ngIf="totalNumberOfRiskHotspots > 10">
            <option value="10">10</option>
            <option value="20" *ngIf="totalNumberOfRiskHotspots > 10">20</option>
            <option value="50" *ngIf="totalNumberOfRiskHotspots > 20">50</option>
            <option value="100" *ngIf="totalNumberOfRiskHotspots > 50">100</option>
            <option [value]="totalNumberOfRiskHotspots" *ngIf="totalNumberOfRiskHotspots > 100">{{translations.all}}</option>
          </select>
        </div>
        <div class="col-center"></div>
        <div class="col-right">
          <span>{{translations.filter}} </span>
          <input type="text" [(ngModel)]="settings.filter" (ngModelChange)="updateRiskHotpots()">
        </div>
      </div>

      <div class="table-responsive">
        <table class="overview table-fixed stripped">
          <colgroup>
            <col class="column-min-200">
            <col class="column-min-200">
            <col class="column-min-200">
            <col class="column105" *ngFor="let riskHotspotMetric of riskHotspotMetrics">
          </colgroup>
          <thead>
            <tr>
              <th><a href="#" (click)="updateSorting('assembly', $event)"><i
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'assembly' && settings.sortOrder === 'asc',
                'icon-down-dir_active': settings.sortBy === 'assembly' && settings.sortOrder === 'desc',
                'icon-up-down-dir': settings.sortBy !== 'assembly'}"></i>{{translations.assembly}}</a></th>
              <th><a href="#" (click)="updateSorting('class', $event)"><i
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'class' && settings.sortOrder === 'asc',
              'icon-down-dir_active': settings.sortBy === 'class' && settings.sortOrder === 'desc',
              'icon-up-down-dir': settings.sortBy !== 'class'}"></i>{{translations.class}}</a></th>
              <th><a href="#" (click)="updateSorting('method', $event)"><i
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'method' && settings.sortOrder === 'asc',
              'icon-down-dir_active': settings.sortBy === 'method' && settings.sortOrder === 'desc',
              'icon-up-down-dir': settings.sortBy !== 'method'}"></i>{{translations.method}}</a></th>
              <th *ngFor="let riskHotspotMetric of riskHotspotMetrics; index as i">
                <a href="#" (click)="updateSorting('' + i, $event)"><i
                [ngClass]="{'icon-up-dir_active': settings.sortBy === '' + i && settings.sortOrder === 'asc',
                'icon-down-dir_active': settings.sortBy === '' + i && settings.sortOrder === 'desc',
                'icon-up-down-dir': settings.sortBy !== '' + i}"></i>{{riskHotspotMetric.name}}</a>
                <a href="{{riskHotspotMetric.explanationUrl}}" target="_blank"><i class="icon-info-circled"></i></a>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let riskHotspot of riskHotspots | slice:0:settings.numberOfRiskHotspots">
              <td>{{riskHotspot.assembly}}</td>
              <td><a [href]="riskHotspot.reportPath + queryString">{{riskHotspot.class}}</a></td>
              <td [title]="riskHotspot.methodName">
                <a [href]="riskHotspot.reportPath + queryString + '#file' + riskHotspot.fileIndex + '_line' + riskHotspot.line">
                  {{riskHotspot.methodShortName}}
                </a>
              </td>
              <td class="right" *ngFor="let metric of riskHotspot.metrics"
                [ngClass]="{'lightred': metric.exceeded, 'lightgreen': !metric.exceeded}">{{metric.value}}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
    styles: [],
    standalone: false
})
export class RiskHotspotsComponent implements OnInit {
  window: Window;
  queryString: string = "";

  riskHotspotMetrics: RiskHotspotMetric[] = [];
  riskHotspots: RiskHotspot[] = [];
  totalNumberOfRiskHotspots: number = 0;
  assemblies: string[] = [];

  translations: any = { };

  settings: RiskHotspotsSettings = new RiskHotspotsSettings();

  constructor(
    windowRef: WindowRefService) {
        this.window = windowRef.nativeWindow;
  }

  ngOnInit(): void {
    this.riskHotspotMetrics = (<any>this.window).riskHotspotMetrics;
    this.translations = (<any>this.window).translations;

    if (this.window.history !== undefined
      && this.window.history.replaceState !== undefined
      && this.window.history.state !== null
      && this.window.history.state.riskHotspotsSettings !== undefined
      && this.window.history.state.riskHotspotsSettings !== null) {
        console.log("Risk hotspots: Restoring from history", this.window.history.state.riskHotspotsSettings);
        this.settings = JSON.parse(JSON.stringify(this.window.history.state.riskHotspotsSettings));
    }

    const startOfQueryString: number = window.location.href.indexOf("?");

    if (startOfQueryString > -1) {
      this.queryString = window.location.href.substring(startOfQueryString);
    }

    this.updateRiskHotpots();
  }

  @HostListener("window:beforeunload")
  onDonBeforeUnlodad(): void {
    if (this.window.history !== undefined && this.window.history.replaceState !== undefined) {
      console.log("Risk hotspots: Updating history", this.settings);

      let globalHistoryState: GlobalHistoryState = new GlobalHistoryState();

      if (window.history.state !== null) {
        globalHistoryState = JSON.parse(JSON.stringify(this.window.history.state));
      }

      globalHistoryState.riskHotspotsSettings = JSON.parse(JSON.stringify(this.settings));
      window.history.replaceState(globalHistoryState, "");
    }
  }

  updateRiskHotpots(): void {
    const allRiskHotspots: RiskHotspot[] = (<any>this.window).riskHotspots;
    this.totalNumberOfRiskHotspots = allRiskHotspots.length;

    if (this.assemblies.length === 0) {
      let assemblies: string[] = [];

      for (let i: number = 0; i < allRiskHotspots.length; i++) {
        if (assemblies.indexOf(allRiskHotspots[i].assembly) === -1) {
          assemblies.push(allRiskHotspots[i].assembly);
        }
      }

      this.assemblies = assemblies.sort();
    }

    let riskHotspots: RiskHotspot[] = [];

    for (let i: number = 0; i < allRiskHotspots.length; i++) {
        if (this.settings.filter !== "" && allRiskHotspots[i].class.toLowerCase().indexOf(this.settings.filter.toLowerCase()) === -1) {
            continue;
        }

        if (this.settings.assembly !== "" && allRiskHotspots[i].assembly !== this.settings.assembly) {
            continue;
        }

        riskHotspots.push(allRiskHotspots[i]);
    }

    let smaller: number = this.settings.sortOrder === "asc" ? -1 : 1;
    let bigger: number = this.settings.sortOrder === "asc" ? 1 : -1;
    if (this.settings.sortBy === "assembly") {
      riskHotspots.sort(function (left: RiskHotspot, right: RiskHotspot): number {
            return left.assembly === right.assembly ?
                0
                : (left.assembly < right.assembly ? smaller : bigger);
        });
    } else if (this.settings.sortBy === "class") {
      riskHotspots.sort(function (left: RiskHotspot, right: RiskHotspot): number {
            return left.class === right.class ?
                0
                : (left.class < right.class ? smaller : bigger);
        });
    } else if (this.settings.sortBy === "method") {
      riskHotspots.sort(function (left: RiskHotspot, right: RiskHotspot): number {
            return left.methodShortName === right.methodShortName ?
                0
                : (left.methodShortName < right.methodShortName ? smaller : bigger);
        });
    } else if (this.settings.sortBy !== "") {
        let metricIndex: number = parseInt(this.settings.sortBy, 10);
        riskHotspots.sort(function (left: RiskHotspot, right: RiskHotspot): number {
            return left.metrics[metricIndex].value === right.metrics[metricIndex].value ?
                0
                : (left.metrics[metricIndex].value < right.metrics[metricIndex].value ? smaller : bigger);
        });
    }

    this.riskHotspots = riskHotspots;
  }

  updateSorting(sortBy: string, $event: Event): void {
    $event.preventDefault();

    if (sortBy === this.settings.sortBy) {
      this.settings.sortOrder = this.settings.sortOrder === "asc" ? "desc" : "asc";
    } else {
      this.settings.sortOrder = "asc";
    }

    this.settings.sortBy = sortBy;

    console.log(`Updating sort column: '${this.settings.sortBy}' (${this.settings.sortOrder})`);
    this.updateRiskHotpots();
  }
}
