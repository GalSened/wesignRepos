import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { SharedService } from "@services/shared.service";
import {environment} from "../../../../../environments/environment";

@Component({
    selector: "sgn-onboard-component",
    templateUrl: "onboard.component.html",
})

export class OnboardComponent implements OnInit {
    public tiny =  environment.tiny;
    constructor(
        private router: Router,
        private sharedService: SharedService,
    ) {
        //this.sharedService.setFlowState("", FLOW_STEP.NONE);
    }

    public ngOnInit() {
        /* TODO */
    }

    // public selfsign() {
    //     this.router.navigate(["dashboard", "selfsign"]);
    // }
}
