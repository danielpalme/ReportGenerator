import { CoverageInfoSettings } from "../data/coverageinfo-settings.class";
import { ClassViewModel } from "./class-viewmodel.class";
import { ElementBase } from "./elementbase.class";
import { Helper } from "./helper.class";

export class CodeElementViewModel extends ElementBase {
    subElements: CodeElementViewModel[] = [];

    classes: ClassViewModel[] = [];

    collapsed: boolean = false;

    constructor(
        name: string,
        parent: CodeElementViewModel|null) {
            super();
            this.name = name;
            this.collapsed = name.indexOf("Test") > -1 && parent === null;
    }

    visible(settings: CoverageInfoSettings): boolean {
        if (settings.filter !== "" && this.name.toLowerCase().indexOf(settings.filter.toLowerCase()) > -1) {
            return true;
        }

        for (let i: number = 0; i < this.subElements.length; i++) {
            if (this.subElements[i].visible(settings)) {
                return true;
            }
        }

        for (let i: number = 0; i < this.classes.length; i++) {
            if (this.classes[i].visible(settings)) {
                return true;
            }
        }

        return false;
    }

    insertClass(clazz: ClassViewModel, grouping: number|null): void {
        this.coveredLines += clazz.coveredLines;
        this.uncoveredLines += clazz.uncoveredLines;
        this.coverableLines += clazz.coverableLines;
        this.totalLines += clazz.totalLines;

        this.coveredBranches += clazz.coveredBranches;
        this.totalBranches += clazz.totalBranches;

        this.coveredMethods += clazz.coveredMethods;
        this.fullyCoveredMethods += clazz.fullyCoveredMethods;
        this.totalMethods += clazz.totalMethods;

        if (grouping === null) {
            this.classes.push(clazz);
            return;
        }

        let groupingDotIndex: number = Helper.getNthOrLastIndexOf(clazz.name, ".", grouping);

        if (groupingDotIndex === -1) {
            groupingDotIndex = Helper.getNthOrLastIndexOf(clazz.name, "\\", grouping);
        }

        let groupedNamespace: string = groupingDotIndex === -1 ? "-" : clazz.name.substring(0, groupingDotIndex);

        for (let i: number = 0; i < this.subElements.length; i++) {
            if (this.subElements[i].name === groupedNamespace) {
                this.subElements[i].insertClass(clazz, null);
                return;
            }
        }

        let subNamespace: CodeElementViewModel = new CodeElementViewModel(groupedNamespace, this);
        this.subElements.push(subNamespace);
        subNamespace.insertClass(clazz, null);
    }

    collapse(): void {
        this.collapsed = true;

        for (let i: number = 0; i < this.subElements.length; i++) {
            this.subElements[i].collapse();
        }
    }

    expand(): void {
        this.collapsed = false;

        for (let i: number = 0; i < this.subElements.length; i++) {
            this.subElements[i].expand();
        }
    }

    toggleCollapse($event: Event): void {
        $event.preventDefault();

        this.collapsed = !this.collapsed;
    }

    updateCurrentHistoricCoverage(historyComparisionDate: string): void {
        for (let i: number = 0; i < this.subElements.length; i++) {
            this.subElements[i].updateCurrentHistoricCoverage(historyComparisionDate);
        }

        for (let i: number = 0; i < this.classes.length; i++) {
            this.classes[i].updateCurrentHistoricCoverage(historyComparisionDate);
        }
    }

    static sortCodeElementViewModels(elements: CodeElementViewModel[], sortBy: string, ascending: boolean): void {
        let smaller: number = ascending ? -1 : 1;
        let bigger: number = ascending ? 1 : -1;

        if (sortBy === "name") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                return left.name === right.name ? 0 : (left.name < right.name ? smaller : bigger);
            });
        } else if (sortBy === "covered") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                return left.coveredLines === right.coveredLines ?
                    0 :
                    (left.coveredLines < right.coveredLines ? smaller : bigger);
            });
        } else if (sortBy === "uncovered") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                return left.uncoveredLines === right.uncoveredLines ?
                    0
                    : (left.uncoveredLines < right.uncoveredLines ? smaller : bigger);
            });
        } else if (sortBy === "coverable") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                return left.coverableLines === right.coverableLines ?
                    0
                    : (left.coverableLines < right.coverableLines ? smaller : bigger);
            });
        } else if (sortBy === "total") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                return left.totalLines === right.totalLines ?
                    0 
                    : (left.totalLines < right.totalLines ? smaller : bigger);
            });
        } else if (sortBy === "coverage") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.coverage === right.coverage) {
                    return 0;
                } else if (isNaN(left.coverage)) {
                    return smaller;
                } else if (isNaN(right.coverage)) {
                    return bigger;
                } else {
                    return left.coverage < right.coverage ? smaller : bigger;
                }
            });
        } else if (sortBy === "covered_branches") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.coveredBranches === right.coveredBranches) {
                    return 0;
                } else if (isNaN(left.coveredBranches)) {
                    return smaller;
                } else if (isNaN(right.coveredBranches)) {
                    return bigger;
                } else {
                    return left.coveredBranches < right.coveredBranches ? smaller : bigger;
                }
            });
        } else if (sortBy === "total_branches") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.totalBranches === right.totalBranches) {
                    return 0;
                } else if (isNaN(left.totalBranches)) {
                    return smaller;
                } else if (isNaN(right.totalBranches)) {
                    return bigger;
                } else {
                    return left.totalBranches < right.totalBranches ? smaller : bigger;
                }
            });
        } else if (sortBy === "branchcoverage") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.branchCoverage === right.branchCoverage) {
                    return 0;
                } else if (isNaN(left.branchCoverage)) {
                    return smaller;
                } else if (isNaN(right.branchCoverage)) {
                    return bigger;
                } else {
                    return left.branchCoverage < right.branchCoverage ? smaller : bigger;
                }
            });
        } else if (sortBy === "covered_methods") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.coveredMethods === right.coveredMethods) {
                    return 0;
                } else if (isNaN(left.coveredMethods)) {
                    return smaller;
                } else if (isNaN(right.coveredMethods)) {
                    return bigger;
                } else {
                    return left.coveredMethods < right.coveredMethods ? smaller : bigger;
                }
            });
        } else if (sortBy === "fullycovered_methods") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.fullyCoveredMethods === right.fullyCoveredMethods) {
                    return 0;
                } else if (isNaN(left.fullyCoveredMethods)) {
                    return smaller;
                } else if (isNaN(right.fullyCoveredMethods)) {
                    return bigger;
                } else {
                    return left.fullyCoveredMethods < right.fullyCoveredMethods ? smaller : bigger;
                }
            });
        } else if (sortBy === "total_methods") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.totalMethods === right.totalMethods) {
                    return 0;
                } else if (isNaN(left.totalMethods)) {
                    return smaller;
                } else if (isNaN(right.totalMethods)) {
                    return bigger;
                } else {
                    return left.totalMethods < right.totalMethods ? smaller : bigger;
                }
            });
        } else if (sortBy === "methodcoverage") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.methodCoverage === right.methodCoverage) {
                    return 0;
                } else if (isNaN(left.methodCoverage)) {
                    return smaller;
                } else if (isNaN(right.methodCoverage)) {
                    return bigger;
                } else {
                    return left.methodCoverage < right.methodCoverage ? smaller : bigger;
                }
            });
        } else if (sortBy === "methodfullcoverage") {
            elements.sort(function (left: CodeElementViewModel, right: CodeElementViewModel): number {
                if (left.methodFullCoverage === right.methodFullCoverage) {
                    return 0;
                } else if (isNaN(left.methodFullCoverage)) {
                    return smaller;
                } else if (isNaN(right.methodFullCoverage)) {
                    return bigger;
                } else {
                    return left.methodFullCoverage < right.methodFullCoverage ? smaller : bigger;
                }
            });
        }
    }

    changeSorting(sortBy: string, ascending: boolean): void {
       CodeElementViewModel.sortCodeElementViewModels(this.subElements, sortBy, ascending);

        let smaller: number = ascending ? -1 : 1;
        let bigger: number = ascending ? 1 : -1;

        if (sortBy === "name") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.name === right.name ? 0 : (left.name < right.name ? smaller : bigger);
            });
        } else if (sortBy === "covered") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.coveredLines === right.coveredLines ?
                        0
                        : (left.coveredLines < right.coveredLines ? smaller : bigger);
            });
        } else if (sortBy === "uncovered") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.uncoveredLines === right.uncoveredLines ?
                        0
                        : (left.uncoveredLines < right.uncoveredLines ? smaller : bigger);
            });
        } else if (sortBy === "coverable") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.coverableLines === right.coverableLines ?
                        0
                        : (left.coverableLines < right.coverableLines ? smaller : bigger);
            });
        } else if (sortBy === "total") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.totalLines === right.totalLines ?
                        0
                        : (left.totalLines < right.totalLines ? smaller : bigger);
            });
        } else if (sortBy === "coverage") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                if (left.coverage === right.coverage) {
                    return 0;
                } else if (isNaN(left.coverage)) {
                    return smaller;
                } else if (isNaN(right.coverage)) {
                    return bigger;
                } else {
                    return left.coverage < right.coverage ? smaller : bigger;
                }
            });
        } else if (sortBy === "covered_branches") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.coveredBranches === right.coveredBranches ?
                        0
                        : (left.coveredBranches < right.coveredBranches ? smaller : bigger);
            });
        } else if (sortBy === "total_branches") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.totalBranches === right.totalBranches ?
                        0
                        : (left.totalBranches < right.totalBranches ? smaller : bigger);
            });
        } else if (sortBy === "branchcoverage") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                if (left.branchCoverage === right.branchCoverage) {
                    return 0;
                } else if (isNaN(left.branchCoverage)) {
                    return smaller;
                } else if (isNaN(right.branchCoverage)) {
                    return bigger;
                } else {
                    return left.branchCoverage < right.branchCoverage ? smaller : bigger;
                }
            });
        } else if (sortBy === "covered_methods") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.coveredMethods === right.coveredMethods ?
                        0
                        : (left.coveredMethods < right.coveredMethods ? smaller : bigger);
            });
        } else if (sortBy === "fullycovered_methods") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.fullyCoveredMethods === right.fullyCoveredMethods ?
                        0
                        : (left.fullyCoveredMethods < right.fullyCoveredMethods ? smaller : bigger);
            });
        } else if (sortBy === "total_methods") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                return left.totalMethods === right.totalMethods ?
                        0
                        : (left.totalMethods < right.totalMethods ? smaller : bigger);
            });
        } else if (sortBy === "methodcoverage") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                if (left.methodCoverage === right.methodCoverage) {
                    return 0;
                } else if (isNaN(left.methodCoverage)) {
                    return smaller;
                } else if (isNaN(right.methodCoverage)) {
                    return bigger;
                } else {
                    return left.methodCoverage < right.methodCoverage ? smaller : bigger;
                }
            });
        } else if (sortBy === "methodfullcoverage") {
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                if (left.methodFullCoverage === right.methodFullCoverage) {
                    return 0;
                } else if (isNaN(left.methodFullCoverage)) {
                    return smaller;
                } else if (isNaN(right.methodFullCoverage)) {
                    return bigger;
                } else {
                    return left.methodFullCoverage < right.methodFullCoverage ? smaller : bigger;
                }
            });
        } else { 
            this.classes.sort(function (left: ClassViewModel, right: ClassViewModel): number {
                const leftMetric = left.metrics[sortBy];
                const rightMetric = right.metrics[sortBy];
                if (leftMetric === rightMetric) {
                    return 0;
                } else if (isNaN(leftMetric)) {
                    return smaller;
                } else if (isNaN(rightMetric)) {
                    return bigger;
                } else {
                    return leftMetric < rightMetric ? smaller : bigger;
                }
            });
        }

        for (let i: number = 0; i < this.subElements.length; i++) {
            this.subElements[i].changeSorting(sortBy, ascending);
        }
    }
}