import { Options } from "@angular-slider/ngx-slider";
import { Component, HostListener } from "@angular/core";
import { WindowRefService } from "../../infrastructure/windowref.service";
import { GlobalHistoryState } from "../globalhistorystate";
import { Assembly } from "./data/assembly.class";
import { CoverageInfoSettings } from "./data/coverageinfo-settings.class";
import { Metric } from "./data/metric.class";
import { ClassViewModel } from "./viewmodels/class-viewmodel.class";
import { CodeElementViewModel } from "./viewmodels/codelement-viewmodel.class";
import { Helper } from "./viewmodels/helper.class";

@Component({
    selector: "coverage-info",
    template: `
  @if (codeElements.length > 0) {
    <div>
      @if (popupVisible) {
        <popup
          [(visible)]="popupVisible"
          [translations]="translations"
          [branchCoverageAvailable]="branchCoverageAvailable"
          [methodCoverageAvailable]="methodCoverageAvailable"
          [metrics]="metrics"
          [(showLineCoverage)]="settings.showLineCoverage"
          [(showBranchCoverage)]="settings.showBranchCoverage"
          [(showMethodCoverage)]="settings.showMethodCoverage"
          [(showMethodFullCoverage)]="settings.showFullMethodCoverage"
          [(visibleMetrics)]="settings.visibleMetrics">
        </popup>
      }
      <div class="customizebox">
        <div>
          <a href="#" (click)="collapseAll($event)">{{translations.collapseAll}}</a>
          |
          <a href="#" (click)="expandAll($event)">{{translations.expandAll}}</a>
        </div>
        <div class="col-center">
          <span class="slider-label">
            @if (settings.grouping === -1) {
              {{translations.noGrouping}}
            }
            @if (settings.grouping === 0) {
              {{translations.byAssembly}}
            }
            @if (settings.grouping > 0) {
              {{translations.byNamespace + ' ' + this.settings.grouping}}
            }
          </span>
          <br />
          {{translations.grouping}}
          <input type="range" step="1" min="-1" [max]="settings.groupingMaximum"
            [(ngModel)]="settings.grouping" (ngModelChange)="updateCoverageInfo()"/>
          </div>
          <div class="col-center">
            @if (historicCoverageExecutionTimes.length > 0) {
              <div>
                {{translations.compareHistory}}
                <select [(ngModel)]="settings.historyComparisionDate" (ngModelChange)="updateCurrentHistoricCoverage()">
                  <option value="">{{translations.date}}</option>
                  @for (time of historicCoverageExecutionTimes; track time) {
                    <option [value]="time">{{time}}</option>
                  }
                </select>
              </div>
              @if (settings.historyComparisionDate !== '') {
                <br />
              }
              @if (settings.historyComparisionDate !== '') {
                <div>
                  <select [(ngModel)]="settings.historyComparisionType">
                    <option value="">{{translations.filter}}</option>
                    <option value="allChanges">{{translations.allChanges}}</option>
                    <option value="lineCoverageIncreaseOnly">{{translations.lineCoverageIncreaseOnly}}</option>
                    <option value="lineCoverageDecreaseOnly">{{translations.lineCoverageDecreaseOnly}}</option>
                    @if (branchCoverageAvailable) {
                      <option value="branchCoverageIncreaseOnly">
                        {{translations.branchCoverageIncreaseOnly}}
                      </option>
                    }
                    @if (branchCoverageAvailable) {
                      <option value="branchCoverageDecreaseOnly">
                        {{translations.branchCoverageDecreaseOnly}}
                      </option>
                    }
                    @if (methodCoverageAvailable) {
                      <option value="methodCoverageIncreaseOnly">
                        {{translations.methodCoverageIncreaseOnly}}
                      </option>
                    }
                    @if (methodCoverageAvailable) {
                      <option value="methodCoverageDecreaseOnly">
                        {{translations.methodCoverageDecreaseOnly}}
                      </option>
                    }
                    @if (methodCoverageAvailable) {
                      <option value="fullMethodCoverageIncreaseOnly">
                        {{translations.fullMethodCoverageIncreaseOnly}}
                      </option>
                    }
                    @if (methodCoverageAvailable) {
                      <option value="fullMethodCoverageDecreaseOnly">
                        {{translations.fullMethodCoverageDecreaseOnly}}
                      </option>
                    }
                  </select>
                </div>
              }
            }
          </div>
          <div class="col-right right">
            <button type="button" (click)="popupVisible=true;"><i class="icon-cog"></i>{{ metrics.length > 0 ? translations.selectCoverageTypesAndMetrics :  translations.selectCoverageTypes}}</button>
          </div>
        </div>
        <div class="table-responsive">
          <table class="overview table-fixed stripped">
            <colgroup>
            <col class="column-min-200">
            @if (settings.showLineCoverage) {
              <col class="column90">
            }
            @if (settings.showLineCoverage) {
              <col class="column105">
            }
            @if (settings.showLineCoverage) {
              <col class="column100">
            }
            @if (settings.showLineCoverage) {
              <col class="column70">
            }
            @if (settings.showLineCoverage) {
              <col class="column98">
            }
            @if (settings.showLineCoverage) {
              <col class="column112">
            }
            @if (branchCoverageAvailable && settings.showBranchCoverage) {
              <col class="column90">
            }
            @if (branchCoverageAvailable && settings.showBranchCoverage) {
              <col class="column70">
            }
            @if (branchCoverageAvailable && settings.showBranchCoverage) {
              <col class="column98">
            }
            @if (branchCoverageAvailable && settings.showBranchCoverage) {
              <col class="column112">
            }
            @if (methodCoverageAvailable && settings.showMethodCoverage) {
              <col class="column90">
            }
            @if (methodCoverageAvailable && settings.showMethodCoverage) {
              <col class="column70">
            }
            @if (methodCoverageAvailable && settings.showMethodCoverage) {
              <col class="column98">
            }
            @if (methodCoverageAvailable && settings.showMethodCoverage) {
              <col class="column112">
            }
            @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
              <col class="column90">
            }
            @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
              <col class="column70">
            }
            @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
              <col class="column98">
            }
            @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
              <col class="column112">
            }
            @for (metric of settings.visibleMetrics; track metric) {
              <col class="column112">
            }
          </colgroup>
          <thead>
            <tr class="header">
              <th></th>
              @if (settings.showLineCoverage) {
                <th class="center" colspan="6">{{translations.coverage}}</th>
              }
              @if (branchCoverageAvailable && settings.showBranchCoverage) {
                <th class="center" colspan="4">{{translations.branchCoverage}}</th>
              }
              @if (methodCoverageAvailable && settings.showMethodCoverage) {
                <th class="center" colspan="4">{{translations.methodCoverage}}</th>
              }
              @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
                <th class="center" colspan="4">{{translations.fullMethodCoverage}}</th>
              }
              @if (settings.visibleMetrics.length > 0) {
                <th class="center" [attr.colspan]="settings.visibleMetrics.length">{{translations.metrics}}</th>
              }
            </tr>
            <tr class="filterbar">
              <td>
                <input type="search" [(ngModel)]="settings.filter" placeholder="{{translations.filter}}" />
              </td>
              @if (settings.showLineCoverage) {
                <td class="center" colspan="6">
                  <ngx-slider [(value)]="settings.lineCoverageMin" [(highValue)]="settings.lineCoverageMax" [options]="sliderOptions"></ngx-slider>
                </td>
              }
              @if (branchCoverageAvailable && settings.showBranchCoverage) {
                <td class="center" colspan="4">
                  <ngx-slider [(value)]="settings.branchCoverageMin" [(highValue)]="settings.branchCoverageMax" [options]="sliderOptions"></ngx-slider>
                </td>
              }
              @if (methodCoverageAvailable && settings.showMethodCoverage) {
                <td class="center" colspan="4">
                  <ngx-slider [(value)]="settings.methodCoverageMin" [(highValue)]="settings.methodCoverageMax" [options]="sliderOptions"></ngx-slider>
                </td>
              }
              @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
                <td class="center" colspan="4">
                  <ngx-slider [(value)]="settings.methodFullCoverageMin" [(highValue)]="settings.methodFullCoverageMax" [options]="sliderOptions"></ngx-slider>
                </td>
              }
              @if (settings.visibleMetrics.length > 0) {
                <td class="center" [attr.colspan]="settings.visibleMetrics.length"></td>
              }
            </tr>
            <tr>
              <th><a href="#" (click)="updateSorting('name', $event)"><i
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'name' && settings.sortOrder === 'asc',
              'icon-down-dir_active': settings.sortBy === 'name' && settings.sortOrder === 'desc',
              'icon-up-down-dir': settings.sortBy !== 'name'}"></i>{{translations.name}}</a></th>
              @if (settings.showLineCoverage) {
                <th class="right"><a href="#" (click)="updateSorting('covered', $event)"><i
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'covered' && settings.sortOrder === 'asc',
              'icon-down-dir_active': settings.sortBy === 'covered' && settings.sortOrder === 'desc',
              'icon-up-down-dir': settings.sortBy !== 'covered'}"></i>{{translations.covered}}</a></th>
              }
              @if (settings.showLineCoverage) {
                <th class="right"><a href="#" (click)="updateSorting('uncovered', $event)"><i
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'uncovered' && settings.sortOrder === 'asc',
              'icon-down-dir_active': settings.sortBy === 'uncovered' && settings.sortOrder === 'desc',
              'icon-up-down-dir': settings.sortBy !== 'uncovered'}"></i>{{translations.uncovered}}</a></th>
              }
              @if (settings.showLineCoverage) {
                <th class="right"><a href="#" (click)="updateSorting('coverable', $event)"><i
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'coverable' && settings.sortOrder === 'asc',
                'icon-down-dir_active': settings.sortBy === 'coverable' && settings.sortOrder === 'desc',
                'icon-up-down-dir': settings.sortBy !== 'coverable'}"></i>{{translations.coverable}}</a></th>
              }
              @if (settings.showLineCoverage) {
                <th class="right"><a href="#" (click)="updateSorting('total', $event)"><i
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'total' && settings.sortOrder === 'asc',
                'icon-down-dir_active': settings.sortBy === 'total' && settings.sortOrder === 'desc',
                'icon-up-down-dir': settings.sortBy !== 'total'}"></i>{{translations.total}}</a></th>
              }
              @if (settings.showLineCoverage) {
                <th class="center" colspan="2">
                  <a href="#" (click)="updateSorting('coverage', $event)"><i
                  [ngClass]="{'icon-up-dir_active': settings.sortBy === 'coverage' && settings.sortOrder === 'asc',
                  'icon-down-dir_active': settings.sortBy === 'coverage' && settings.sortOrder === 'desc',
                  'icon-up-down-dir': settings.sortBy !== 'coverage'}"></i>{{translations.percentage}}</a></th>
                }
                @if (branchCoverageAvailable && settings.showBranchCoverage) {
                  <th class="right"><a href="#" (click)="updateSorting('covered_branches', $event)"><i
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'covered_branches' && settings.sortOrder === 'asc',
              'icon-down-dir_active': settings.sortBy === 'covered_branches' && settings.sortOrder === 'desc',
              'icon-up-down-dir': settings.sortBy !== 'covered_branches'}"></i>{{translations.covered}}</a></th>
                }
                @if (branchCoverageAvailable && settings.showBranchCoverage) {
                  <th class="right"><a href="#" (click)="updateSorting('total_branches', $event)"><i
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'total_branches' && settings.sortOrder === 'asc',
                'icon-down-dir_active': settings.sortBy === 'total_branches' && settings.sortOrder === 'desc',
                'icon-up-down-dir': settings.sortBy !== 'total_branches'}"></i>{{translations.total}}</a></th>
                }
                @if (branchCoverageAvailable && settings.showBranchCoverage) {
                  <th class="center" colspan="2">
                    <a href="#" (click)="updateSorting('branchcoverage', $event)"><i
                  [ngClass]="{'icon-up-dir_active': settings.sortBy === 'branchcoverage' && settings.sortOrder === 'asc',
                  'icon-down-dir_active': settings.sortBy === 'branchcoverage' && settings.sortOrder === 'desc',
                  'icon-up-down-dir': settings.sortBy !== 'branchcoverage'}"></i>{{translations.percentage}}</a></th>
                  }
                  @if (methodCoverageAvailable && settings.showMethodCoverage) {
                    <th class="right"><a href="#" (click)="updateSorting('covered_methods', $event)"><i
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'covered_methods' && settings.sortOrder === 'asc',
              'icon-down-dir_active': settings.sortBy === 'covered_methods' && settings.sortOrder === 'desc',
              'icon-up-down-dir': settings.sortBy !== 'covered_methods'}"></i>{{translations.covered}}</a></th>
                  }
                  @if (methodCoverageAvailable && settings.showMethodCoverage) {
                    <th class="right"><a href="#" (click)="updateSorting('total_methods', $event)"><i
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'total_methods' && settings.sortOrder === 'asc',
                'icon-down-dir_active': settings.sortBy === 'total_methods' && settings.sortOrder === 'desc',
                'icon-up-down-dir': settings.sortBy !== 'total_methods'}"></i>{{translations.total}}</a></th>
                  }
                  @if (methodCoverageAvailable && settings.showMethodCoverage) {
                    <th class="center" colspan="2">
                      <a href="#" (click)="updateSorting('methodcoverage', $event)"><i
                  [ngClass]="{'icon-up-dir_active': settings.sortBy === 'methodcoverage' && settings.sortOrder === 'asc',
                  'icon-down-dir_active': settings.sortBy === 'methodcoverage' && settings.sortOrder === 'desc',
                  'icon-up-down-dir': settings.sortBy !== 'methodcoverage'}"></i>{{translations.percentage}}</a></th>
                    }
                    @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
                      <th class="right"><a href="#" (click)="updateSorting('fullycovered_methods', $event)"><i
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'fullycovered_methods' && settings.sortOrder === 'asc',
              'icon-down-dir_active': settings.sortBy === 'fullycovered_methods' && settings.sortOrder === 'desc',
              'icon-up-down-dir': settings.sortBy !== 'fullycovered_methods'}"></i>{{translations.covered}}</a></th>
                    }
                    @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
                      <th class="right"><a href="#" (click)="updateSorting('total_methods', $event)"><i
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'total_methods' && settings.sortOrder === 'asc',
                'icon-down-dir_active': settings.sortBy === 'total_methods' && settings.sortOrder === 'desc',
                'icon-up-down-dir': settings.sortBy !== 'total_methods'}"></i>{{translations.total}}</a></th>
                    }
                    @if (methodCoverageAvailable && settings.showFullMethodCoverage) {
                      <th class="center" colspan="2">
                        <a href="#" (click)="updateSorting('methodfullcoverage', $event)"><i
                  [ngClass]="{'icon-up-dir_active': settings.sortBy === 'methodfullcoverage' && settings.sortOrder === 'asc',
                  'icon-down-dir_active': settings.sortBy === 'methodfullcoverage' && settings.sortOrder === 'desc',
                  'icon-up-down-dir': settings.sortBy !== 'methodfullcoverage'}"></i>{{translations.percentage}}</a></th>
                      }
                      @for (metric of settings.visibleMetrics; track metric) {
                        <th>
                          <a href="#" (click)="updateSorting(metric.abbreviation, $event)"><i
                  [ngClass]="{'icon-up-dir_active': settings.sortBy === metric.abbreviation && settings.sortOrder === 'asc',
                  'icon-down-dir_active': settings.sortBy === metric.abbreviation && settings.sortOrder === 'desc',
                  'icon-up-down-dir': settings.sortBy !== metric.abbreviation}"></i>{{metric.name}}</a><a href="{{metric.explanationUrl}}" target="_blank"><i class="icon-info-circled"></i></a>
                          </th>
                        }
                      </tr>
                    </thead>
                    <tbody>
                      @for (element of codeElements; track element) {
                        @if (element.visible(settings)) {
                          <tr
                            codeelement-row
                            [element]="element"
                            [collapsed]="element.collapsed"
                            [lineCoverageAvailable]="settings.showLineCoverage"
                            [branchCoverageAvailable]="branchCoverageAvailable && settings.showBranchCoverage"
                            [methodCoverageAvailable]="methodCoverageAvailable && settings.showMethodCoverage"
                            [methodFullCoverageAvailable]="methodCoverageAvailable && settings.showFullMethodCoverage"
                            [visibleMetrics]="settings.visibleMetrics">
                          </tr>
                        }
                        @for (clazz of element.classes; track clazz) {
                          @if (!element.collapsed
                            && clazz.visible(settings)) {
                            <tr
                              class-row [clazz]="clazz"
                              [translations]="translations"
                              [lineCoverageAvailable]="settings.showLineCoverage"
                              [branchCoverageAvailable]="branchCoverageAvailable && settings.showBranchCoverage"
                              [methodCoverageAvailable]="methodCoverageAvailable && settings.showMethodCoverage"
                              [methodFullCoverageAvailable]="methodCoverageAvailable && settings.showFullMethodCoverage"
                              [visibleMetrics]="settings.visibleMetrics"
                              [historyComparisionDate]="settings.historyComparisionDate">
                            </tr>
                          }
                        }
                        @for (subElement of element.subElements; track subElement) {
                          @if (!element.collapsed
                            && subElement.visible(settings)) {
                            <tr class="namespace"
                              codeelement-row
                              [element]="subElement"
                              [collapsed]="subElement.collapsed"
                              [lineCoverageAvailable]="settings.showLineCoverage"
                              [branchCoverageAvailable]="branchCoverageAvailable && settings.showBranchCoverage"
                              [methodCoverageAvailable]="methodCoverageAvailable && settings.showMethodCoverage"
                              [methodFullCoverageAvailable]="methodCoverageAvailable && settings.showFullMethodCoverage"
                              [visibleMetrics]="settings.visibleMetrics">
                            </tr>
                            @for (clazz of subElement.classes; track clazz) {
                              @if (!subElement.collapsed
                                && clazz.visible(settings)) {
                                <tr class="namespace"
                                  class-row [clazz]="clazz"
                                  [translations]="translations"
                                  [lineCoverageAvailable]="settings.showLineCoverage"
                                  [branchCoverageAvailable]="branchCoverageAvailable && settings.showBranchCoverage"
                                  [methodCoverageAvailable]="methodCoverageAvailable && settings.showMethodCoverage"
                                  [methodFullCoverageAvailable]="methodCoverageAvailable && settings.showFullMethodCoverage"
                                  [visibleMetrics]="settings.visibleMetrics"
                                  [historyComparisionDate]="settings.historyComparisionDate">
                                </tr>
                              }
                            }
                          }
                        }
                      }
                    </tbody>
                  </table>
                </div>
              </div>
            }`,
    standalone: false
})
export class CoverageInfoComponent {
  window: Window;
  queryString: string = "";

  historicCoverageExecutionTimes: string[] = [];
  branchCoverageAvailable: boolean = false;
  methodCoverageAvailable: boolean = false;
  metrics: Metric[] = [];
  codeElements: CodeElementViewModel[] = [];
  translations: any = { };

  popupVisible: boolean = false;

  settings: CoverageInfoSettings = new CoverageInfoSettings();

  sliderOptions: Options = {
    floor: 0,
    ceil: 100,
    step: 1,
    ticksArray: [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100],
    showTicks: true
  };

  constructor(
    windowRef: WindowRefService) {
        this.window = windowRef.nativeWindow;
  }

  ngOnInit(): void {
    this.historicCoverageExecutionTimes = (<any>this.window).historicCoverageExecutionTimes;
    this.branchCoverageAvailable = (<any>this.window).branchCoverageAvailable;
    this.methodCoverageAvailable = (<any>this.window).methodCoverageAvailable;
    this.metrics = (<any>this.window).metrics;

    this.translations = (<any>this.window).translations;
    Helper.maximumDecimalPlacesForCoverageQuotas = (<any>this.window).maximumDecimalPlacesForCoverageQuotas;

    let restoredFromHistory: boolean = false;

    if (this.window.history !== undefined
      && this.window.history.replaceState !== undefined
      && this.window.history.state !== null
      && this.window.history.state.coverageInfoSettings !== undefined
      && this.window.history.state.coverageInfoSettings !== null) {
        console.log("Coverage info: Restoring from history", this.window.history.state.coverageInfoSettings);
        restoredFromHistory = true;
        this.settings = JSON.parse(JSON.stringify(this.window.history.state.coverageInfoSettings));
    } else {
      let groupingMaximum: number = 0;
      let assemblies: Assembly[] = (<any>this.window).assemblies;

      for (let i: number = 0; i < assemblies.length; i++) {
        for (let j: number = 0; j < assemblies[i].classes.length; j++) {
          groupingMaximum = Math.max(
            groupingMaximum,
            (assemblies[i].classes[j].name.match(/\.|\\/g) || []).length);
        }
      }

      this.settings.groupingMaximum = groupingMaximum;
      console.log("Grouping maximum: " + groupingMaximum);

      if ((<any>this.window).applyMaximumGroupingLevel) {
        this.settings.grouping = groupingMaximum;
      }

      this.settings.showBranchCoverage = this.branchCoverageAvailable;
      this.settings.showMethodCoverage = this.methodCoverageAvailable;
      this.settings.showFullMethodCoverage = this.methodCoverageAvailable;
    }

    const startOfQueryString: number = window.location.href.indexOf("?");

    if (startOfQueryString > -1) {
      this.queryString = window.location.href.substring(startOfQueryString);
    }

    this.updateCoverageInfo();

    if (restoredFromHistory) {
      this.restoreCollapseState();
    }
  }

  @HostListener("window:beforeunload")
  onBeforeUnload(): void {
    this.saveCollapseState();

    if (this.window.history !== undefined && this.window.history.replaceState !== undefined) {
      console.log("Coverage info: Updating history", this.settings);

      let globalHistoryState: GlobalHistoryState = new GlobalHistoryState();

      if (window.history.state !== null) {
        globalHistoryState = JSON.parse(JSON.stringify(this.window.history.state));
      }

      globalHistoryState.coverageInfoSettings = JSON.parse(JSON.stringify(this.settings));
      window.history.replaceState(globalHistoryState, "");
    }
  }

  updateCoverageInfo(): void {
    let start: number = new Date().getTime();

    let assemblies: Assembly[] = (<any>this.window).assemblies;

    let codeElements: CodeElementViewModel[] = [];
    let numberOfClasses: number = 0;

    if (this.settings.grouping === 0) { // group by assembly
        for (let i: number = 0; i < assemblies.length; i++) {
            let assemblyElement: CodeElementViewModel = new CodeElementViewModel(assemblies[i].name, null);
            codeElements.push(assemblyElement);

            for (let j: number = 0; j < assemblies[i].classes.length; j++) {
                assemblyElement.insertClass(new ClassViewModel(assemblies[i].classes[j], this.queryString), null);
                numberOfClasses++;
            }
        }
    } else if (this.settings.grouping === -1) { // no grouping
        let assemblyElement: CodeElementViewModel = new CodeElementViewModel(this.translations.all, null);
        codeElements.push(assemblyElement);

        for (let i: number = 0; i < assemblies.length; i++) {
            for (let j: number = 0; j < assemblies[i].classes.length; j++) {
                assemblyElement.insertClass(new ClassViewModel(assemblies[i].classes[j], this.queryString), null);
                numberOfClasses++;
            }
        }
    } else { // group by assembly and namespace
        for (let i: number = 0; i < assemblies.length; i++) {
          let assemblyElement: CodeElementViewModel = new CodeElementViewModel(assemblies[i].name, null);
          codeElements.push(assemblyElement);

            for (let j: number = 0; j < assemblies[i].classes.length; j++) {
                assemblyElement.insertClass(new ClassViewModel(assemblies[i].classes[j], this.queryString), this.settings.grouping);
                numberOfClasses++;
            }
        }
    }

    let smaller: number = -1;
    let bigger: number = 1;

    if (this.settings.sortBy === "name") {
        smaller = this.settings.sortOrder === "asc" ? -1 : 1;
        bigger = this.settings.sortOrder === "asc" ? 1 : -1;
    }

    codeElements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
        return left.name === right.name ? 0 : (left.name < right.name ? smaller : bigger);
    });

    CodeElementViewModel.sortCodeElementViewModels(codeElements, this.settings.sortBy, this.settings.sortOrder === "asc");

    for (let i: number = 0; i < codeElements.length; i++) {
      codeElements[i].changeSorting(this.settings.sortBy, this.settings.sortOrder === "asc");
    }

    this.codeElements = codeElements;

    // tslint:disable-next-line
    console.log(`Processing assemblies finished (Duration: ${(new Date().getTime() - start)}ms, Assemblies: ${codeElements.length}, Classes: ${numberOfClasses})`);

    if (this.settings.historyComparisionDate !== "") {
      this.updateCurrentHistoricCoverage();
    }
  }

  updateCurrentHistoricCoverage(): void {
    let start: number = new Date().getTime();

    for (let i: number = 0; i < this.codeElements.length; i++) {
      this.codeElements[i].updateCurrentHistoricCoverage(this.settings.historyComparisionDate);
    }

    console.log(`Updating current historic coverage finished (Duration: ${(new Date().getTime() - start)}ms)`);
  }

  collapseAll($event: Event): void {
    $event.preventDefault();

    for (let i: number = 0; i < this.codeElements.length; i++) {
      this.codeElements[i].collapse();
    }
  }

  expandAll($event: Event): void {
    $event.preventDefault();

    for (let i: number = 0; i < this.codeElements.length; i++) {
      this.codeElements[i].expand();
    }
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
    CodeElementViewModel.sortCodeElementViewModels(this.codeElements, this.settings.sortBy, this.settings.sortOrder === "asc");

    for (let i: number = 0; i < this.codeElements.length; i++) {
      this.codeElements[i].changeSorting(this.settings.sortBy, this.settings.sortOrder === "asc");
    }
  }

  saveCollapseState(): void {
    this.settings.collapseStates = [];

    let saveCollapseStateRecursive:(elements: CodeElementViewModel[]) => void = (elements: CodeElementViewModel[]) => {
      for (let i: number = 0; i < elements.length; i++) {
        this.settings.collapseStates.push(elements[i].collapsed);

        saveCollapseStateRecursive(elements[i].subElements);
      }
    };

    saveCollapseStateRecursive(this.codeElements);
  }

  restoreCollapseState(): void {
    let counter: number = 0;

    let restoreCollapseStateRecursive:(elements: CodeElementViewModel[]) => void
      = (elements: CodeElementViewModel[]) => {
      for (let i: number = 0; i < elements.length; i++) {
        if (this.settings.collapseStates.length > counter) {
          elements[i].collapsed = this.settings.collapseStates[counter];
        }

        counter++;
        restoreCollapseStateRecursive(elements[i].subElements);
      }
    };

    restoreCollapseStateRecursive(this.codeElements);
  }
}
