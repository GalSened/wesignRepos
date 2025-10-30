import { NgModule } from "@angular/core";
import { TogglePasswordDirective } from "../directives/toggle-password.directive";

@NgModule({
    exports: [
        TogglePasswordDirective
    ],
    declarations: [
        TogglePasswordDirective
    ]
})
export class HelperModule {
    constructor() { }
}