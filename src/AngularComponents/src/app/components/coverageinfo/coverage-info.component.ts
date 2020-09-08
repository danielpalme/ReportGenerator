import { Component, HostListener } from "@angular/core";
import { WindowRefService } from "../../infrastructure/windowref.service";
import { GlobalHistoryState } from "../globalhistorystate";
import { Assembly } from "./data/assembly.class";
import { CoverageInfoSettings } from "./data/coverageinfo-settings.class";
import { ClassViewModel } from "./viewmodels/class-viewmodel.class";
import { CodeElementViewModel } from "./viewmodels/codelement-viewmodel.class";

@Component({
  selector: "coverage-info",
  template: `
  <div *ngIf="codeElements.length > 0">
    <div class="customizebox">
      <div>
        <a href="#" (click)="collapseAll($event)">{{translations.collapseAll}}</a>
        |
        <a href="#" (click)="expandAll($event)">{{translations.expandAll}}</a>
      </div>
      <div class="center">
        <ng-container *ngIf="settings.grouping === -1">{{translations.noGrouping}}</ng-container>
        <ng-container *ngIf="settings.grouping === 0">{{translations.byAssembly}}</ng-container>
        <ng-container *ngIf="settings.grouping > 0">{{translations.byNamespace + ' ' + this.settings.grouping}}</ng-container>
        <br />
        {{translations.grouping}}
        <input type="range" step="1" min="-1" [max]="settings.groupingMaximum"
          [(ngModel)]="settings.grouping" (ngModelChange)="updateCoverageInfo()"/>
      </div>
      <div class="center">
        <ng-container *ngIf="historicCoverageExecutionTimes.length > 0">
          <div>
            {{translations.compareHistory}}
            <select [(ngModel)]="settings.historyComparisionDate" (ngModelChange)="updateCurrentHistoricCoverage()">
              <option value="">{{translations.date}}</option>
              <option *ngFor="let time of historicCoverageExecutionTimes" [value]="time">{{time}}</option>
            </select>
          </div>
          <br *ngIf="settings.historyComparisionDate !== ''" />
          <div *ngIf="settings.historyComparisionDate !== ''">
            <select [(ngModel)]="settings.historyComparisionType">
              <option value="">{{translations.filter}}</option>
              <option value="allChanges">{{translations.allChanges}}</option>
              <option value="lineCoverageIncreaseOnly">{{translations.lineCoverageIncreaseOnly}}</option>
              <option value="lineCoverageDecreaseOnly">{{translations.lineCoverageDecreaseOnly}}</option>
              <option value="branchCoverageIncreaseOnly" *ngIf="branchCoverageAvailable">
                {{translations.branchCoverageIncreaseOnly}}
              </option>
              <option value="branchCoverageDecreaseOnly" *ngIf="branchCoverageAvailable">
                {{translations.branchCoverageDecreaseOnly}}
              </option>
            </select>
          </div>
        </ng-container>
      </div>
      <div class="right">
        <span>{{translations.filter}} </span>
        <input type="text" [(ngModel)]="settings.filter">
      </div>
    </div>

    <table class="overview table-fixed stripped">
        <colgroup>
          <col>
          <col class="column90">
          <col class="column105">
          <col class="column100">
          <col class="column70">
          <col class="column98">
          <col class="column112">
          <col class="column90" *ngIf="branchCoverageAvailable">
          <col class="column70" *ngIf="branchCoverageAvailable">
          <col class="column98" *ngIf="branchCoverageAvailable">
          <col class="column112" *ngIf="branchCoverageAvailable">
        </colgroup>
        <thead>
          <tr>
            <th><a href="#" (click)="updateSorting('name', $event)"><i class="icon-down-dir"
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'name' && settings.sortOrder === 'desc',
              'icon-down-dir_active': settings.sortBy === 'name' && settings.sortOrder === 'asc',
              'icon-down-dir': settings.sortBy !== 'name'}"></i>{{translations.name}}</a></th>
            <th class="right"><a href="#" (click)="updateSorting('covered', $event)"><i class="icon-down-dir"
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'covered' && settings.sortOrder === 'desc',
              'icon-down-dir_active': settings.sortBy === 'covered' && settings.sortOrder === 'asc',
              'icon-down-dir': settings.sortBy !== 'covered'}"></i>{{translations.covered}}</a></th>
            <th class="right"><a href="#" (click)="updateSorting('uncovered', $event)"><i class="icon-down-dir"
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'uncovered' && settings.sortOrder === 'desc',
              'icon-down-dir_active': settings.sortBy === 'uncovered' && settings.sortOrder === 'asc',
              'icon-down-dir': settings.sortBy !== 'uncovered'}"></i>{{translations.uncovered}}</a></th>
            <th class="right"><a href="#" (click)="updateSorting('coverable', $event)"><i class="icon-down-dir"
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'coverable' && settings.sortOrder === 'desc',
                'icon-down-dir_active': settings.sortBy === 'coverable' && settings.sortOrder === 'asc',
                'icon-down-dir': settings.sortBy !== 'coverable'}"></i>{{translations.coverable}}</a></th>
            <th class="right"><a href="#" (click)="updateSorting('total', $event)"><i class="icon-down-dir"
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'total' && settings.sortOrder === 'desc',
                'icon-down-dir_active': settings.sortBy === 'total' && settings.sortOrder === 'asc',
                'icon-down-dir': settings.sortBy !== 'total'}"></i>{{translations.total}}</a></th>
            <th class="center" colspan="2">
                <a href="#" (click)="updateSorting('coverage', $event)"><i class="icon-down-dir"
                  [ngClass]="{'icon-up-dir_active': settings.sortBy === 'coverage' && settings.sortOrder === 'desc',
                  'icon-down-dir_active': settings.sortBy === 'coverage' && settings.sortOrder === 'asc',
                  'icon-down-dir': settings.sortBy !== 'coverage'}"></i>{{translations.coverage}}</a></th>
            <th class="right" *ngIf="branchCoverageAvailable"><a href="#" (click)="updateSorting('covered_branches', $event)"><i class="icon-down-dir"
              [ngClass]="{'icon-up-dir_active': settings.sortBy === 'covered_branches' && settings.sortOrder === 'desc',
              'icon-down-dir_active': settings.sortBy === 'covered_branches' && settings.sortOrder === 'asc',
              'icon-down-dir': settings.sortBy !== 'covered_branches'}"></i>{{translations.covered}}</a></th>
            <th class="right" *ngIf="branchCoverageAvailable"><a href="#" (click)="updateSorting('total_branches', $event)"><i class="icon-down-dir"
                [ngClass]="{'icon-up-dir_active': settings.sortBy === 'total_branches' && settings.sortOrder === 'desc',
                'icon-down-dir_active': settings.sortBy === 'total_branches' && settings.sortOrder === 'asc',
                'icon-down-dir': settings.sortBy !== 'total_branches'}"></i>{{translations.total}}</a></th>
            <th class="center" colspan="2" *ngIf="branchCoverageAvailable">
                <a href="#" (click)="updateSorting('branchcoverage', $event)"><i class="icon-down-dir"
                  [ngClass]="{'icon-up-dir_active': settings.sortBy === 'branchcoverage' && settings.sortOrder === 'desc',
                  'icon-down-dir_active': settings.sortBy === 'branchcoverage' && settings.sortOrder === 'asc',
                  'icon-down-dir': settings.sortBy !== 'branchcoverage'}"></i>{{translations.branchCoverage}}</a></th>
          </tr>
        </thead>
        <tbody>
          <ng-container *ngFor="let element of codeElements">
            <tr *ngIf="element.visible(settings.filter, settings.historyComparisionType)"
              codeelement-row
              [element]="element"
              [collapsed]="element.collapsed"
              [branchCoverageAvailable]="branchCoverageAvailable">
            </tr>
            <ng-container *ngFor="let clazz of element.classes">
              <tr *ngIf="!element.collapsed
                && clazz.visible(settings.filter, settings.historyComparisionType)"
                class-row [clazz]="clazz"
                  [translations]="translations"
                  [branchCoverageAvailable]="branchCoverageAvailable"
                  [historyComparisionDate]="settings.historyComparisionDate">
              </tr>
            </ng-container>
            <ng-container *ngFor="let subElement of element.subElements">
              <ng-container *ngIf="!element.collapsed
                && subElement.visible(settings.filter, settings.historyComparisionType)">
               <tr class="namespace"
                 codeelement-row
                 [element]="subElement"
                 [collapsed]="subElement.collapsed"
                 [branchCoverageAvailable]="branchCoverageAvailable">
                </tr>
                <ng-container *ngFor="let clazz of subElement.classes">
                  <tr class="namespace" *ngIf="!subElement.collapsed
                   && clazz.visible(settings.filter, settings.historyComparisionType)"
                   class-row [clazz]="clazz"
                    [translations]="translations"
                    [branchCoverageAvailable]="branchCoverageAvailable"
                    [historyComparisionDate]="settings.historyComparisionDate">
                  </tr>
                </ng-container>
              </ng-container>
            </ng-container>
          </ng-container>
        </tbody>
      </table>
  </div>`
})
export class CoverageInfoComponent {
  window: Window;
  queryString: string = "";

  historicCoverageExecutionTimes: string[] = [];
  branchCoverageAvailable: boolean = false;
  codeElements: CodeElementViewModel[] = [];

  translations: any = { };

  settings: CoverageInfoSettings = new CoverageInfoSettings();

  constructor(
    windowRef: WindowRefService) {
        this.window = windowRef.nativeWindow;
  }

  ngOnInit(): void {
    this.historicCoverageExecutionTimes = (<any>this.window).historicCoverageExecutionTimes;
    this.branchCoverageAvailable = (<any>this.window).branchCoverageAvailable;

    this.translations = (<any>this.window).translations;

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
            (assemblies[i].classes[j].name.match(/\./g) || []).length);
        }
      }

      this.settings.groupingMaximum = groupingMaximum;
      console.log("Grouping maximum: " + groupingMaximum);
    }

    const startOfQueryString: number = window.location.href.indexOf("?");

    if (startOfQueryString > -1) {
      this.queryString = window.location.href.substr(startOfQueryString);
    }

    this.updateCoverageInfo();

    if (restoredFromHistory) {
      this.restoreCollapseState();
    }
  }

  @HostListener("window:beforeunload")
  onDonBeforeUnlodad(): void {
    this.saveCollapseState();

    if (this.window.history !== undefined && this.window.history.replaceState !== undefined) {
      console.log("Coverage info: Updating history", this.settings);

      let globalHistoryState: GlobalHistoryState = null;

      if (window.history.state !== null) {
        globalHistoryState = JSON.parse(JSON.stringify(this.window.history.state));
      } else {
        globalHistoryState = new GlobalHistoryState();
      }

      globalHistoryState.coverageInfoSettings = JSON.parse(JSON.stringify(this.settings));
      window.history.replaceState(globalHistoryState, null);
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
