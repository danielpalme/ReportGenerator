import { Component, Input } from "@angular/core";

@Component({
    selector: "pro-button",
    template: `&nbsp;<a href="https://reportgenerator.io/pro" class="pro-button pro-button-tiny" target="_blank" title="{{ translations.methodCoverageProVersion}}">PRO</a>`,
    standalone: false
})
export class ProButton {
  @Input() translations: any = { };
}