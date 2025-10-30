import { Component, OnInit, ViewChild, ViewEncapsulation } from "@angular/core";
import { FormGroup, FormControl, Validators, AbstractControl } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { UserApiService } from "@services/user-api.service";
import { Errors } from '@models/error/errors.model';
import { SignUp } from '@models/Users/sign-up.model';
import { LangList } from '@components/shared/languages.list';
import { Observable, of } from 'rxjs';
import { SharedService } from '@services/shared.service';
import { Plan } from '@models/account/plan.model';
import { PaymentApiService } from '@services/payment-api.service';
import { PaymentDetailsRequest } from '@models/payment/payment-details-request.model';
import { switchMap, catchError } from 'rxjs/operators';
import { environment } from "../../../environments/environment"
import { Currency } from '@models/enums/currency.enum';
import { ConfigurationApiService } from '@services/configuration-api.service';
import { AppConfigService } from '@services/app-config.service';
import { LangSelectorComponent } from '@components/shared/lang-selector.component';
import { ILanguage } from '@components/shared/languages.list';
@Component({
    selector: "sgn-register",
    templateUrl: "register.component.html",
})

export class RegisterComponent implements OnInit {
    public registerForm: FormGroup;
    public isBusy: boolean = false;
    public errorsArr: string[] = [];
    public plans: Plan[];
    public trialCheck: boolean;
    public eyeIcon: string = "eye";
    public eyeIcon2: string = "eye";
    public selectedPlan: Plan;
    public isRtl: boolean = false;
    public showIntro: boolean = false;
    public tiny = environment.tiny;
    public currency: Currency = Currency.USD;
    captchaResponse: string;
    shouldUseReCaptchaInRegistration: boolean = false;
    reCaptchaKey: string;
    
    public languages: ILanguage[] = LangList;
    @ViewChild('lang', { static: true }) lang: LangSelectorComponent;
    constructor(
        private userApiService: UserApiService,
        private configurationApiService: ConfigurationApiService,
        private router: Router,
        private appConfigService: AppConfigService,
        private sharedService: SharedService,
        private paymentApiService: PaymentApiService,
        private activeRoute: ActivatedRoute
    ) {
        this.reCaptchaKey = this.appConfigService.reCaptchaKey;
    }

    public ngOnInit() {
        let language = LangList.find(x => x.Code == 'he');
        this.lang.languageSelected(language);

        this.isRtl = document.getElementsByTagName("html")[0].getAttribute("dir") == "rtl";

        let _fragment;
        this.activeRoute.fragment.pipe(
            switchMap(fragment => {
                _fragment = fragment;
                return this.sharedService.getPlans();
            }))
            .subscribe(plans => {
                this.plans = plans;
                this.selectedPlan = this.plans[0];
                if (_fragment) {
                    let split
                    if (_fragment == 0) {
                        this.trialCheck = true;
                        //this.selectPlan(this.plans[0].programID);
                        this.selectedPlan = this.plans[0];
                    }
                    else {

                        let splitfragment = _fragment.split("-", 2);
                        const _indx = (splitfragment[0] - 1);
                        //if (this.plans[_indx]) this.selectPlan(this.plans[_indx].programID);
                        this.selectedPlan = this.plans[_indx] ? this.plans[_indx] : this.plans[0];
                        if (splitfragment[1] == "1") {
                            this.currency = Currency.ILS;
                        }
                        else if (splitfragment[1] == "3") {
                            this.currency = Currency.EUR;
                        }
                        else {
                            this.currency = Currency.USD;
                        }

                    }
                }
            });

        this.registerForm = new FormGroup({
            fullName: new FormControl('', Validators.required),
            //lastName: new FormControl('', Validators.required),
            email: new FormControl('', [Validators.required, Validators.email]),
            password: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")]),
            //confirmPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")]),
            isConfirmTerms: new FormControl('', Validators.requiredTrue),
            isTrial: new FormControl('')
        }, {
            // validators: this.MatchPassword
        });

        this.configurationApiService.readInitConfiguration().subscribe(
            (data) => {
                this.shouldUseReCaptchaInRegistration = data.shouldUseReCaptchaInRegistration
            },
            (error) => {

            }
        )
    }

    get getSelectdSign(): string {
        switch (this.currency) {

            case Currency.ILS:
                {
                    return "₪"
                }
            case Currency.USD:
                {
                    return "$"
                }
            case Currency.EUR:
                {
                    return "€"
                }

        }

    }

    get getSelectedPricePerMonth(): number {
        switch (this.currency) {

            case Currency.ILS:
                {
                    return this.selectedPlan.pricePerMonthILS;
                }
            case Currency.USD:
                {
                    return this.selectedPlan.pricePerMonthUSD;
                }
            case Currency.EUR:
                {
                    return this.selectedPlan.pricePerMonthEUR;
                }

        }
    }

    public changePlan() {
       
        if  (this.currency != Currency.ILS) {
            window.location.href = this.appConfigService.LiteChangePlanEngUrl;
        }
        else {
            window.location.href = this.appConfigService.LiteChangePlanHebUrl;
        }
    }

    public MatchPassword(AC: AbstractControl) {
        let password = AC.get('password').value; // to get value in input tag
        let confirmPassword = AC.get('confirmPassword').value; // to get value in input tag
        if (password != confirmPassword) {
            AC.get('confirmPassword').setErrors({ MatchPassword: true })
        } else {
            return null
        }
    }

    public selectPlan(programId: string) {
        this.plans.forEach(p => {
            p.programID != programId ? p.isBestPlan = false : p.isBestPlan = true;
        });
    }

    public register() {
        this.errorsArr = [];

        if (this.registerForm.valid) {
            let signUpUser = new SignUp();
            signUpUser.Name = this.registerForm.get('fullName').value;
            signUpUser.Password = this.registerForm.get('password').value;
            signUpUser.Email = this.registerForm.get('email').value;
            let language = document.getElementsByTagName("html")[0].getAttribute("lang");
            let langCode = LangList.find((l) => l.Code === language);
            signUpUser.Language = langCode ? langCode.Enum : 1;
            signUpUser.ReCAPCHA = this.captchaResponse;
            signUpUser.SendActivationLink = true;
            this.isBusy = true;
            this.userApiService.signUp(signUpUser).pipe(
                switchMap(() => {
                    if (this.trialCheck || !this.tiny) return of(null); // If trial

                    return this.paymentApiService.openSessionPaymentApi(signUpUser.Email);
                }),
                switchMap((_token) => {
                    if (this.trialCheck || !this.tiny) return of(null); // If trial

                    // let _selectedPlan = this.plans.find(p => p.isBestPlan);
                    let _selectedPlan = this.selectedPlan;
                    let _nameArr = signUpUser.Name.split(" ") || [signUpUser.Name];

                    var paymentDetailsRequest = new PaymentDetailsRequest();
                    paymentDetailsRequest.token = _token;
                    paymentDetailsRequest.product.curreny = this.currency;
                    paymentDetailsRequest.product.language = language == "ltr" ? 2:  1 ; // 1-HE, 2-EN
                    paymentDetailsRequest.product.productType = 0; // WESIGN
                    paymentDetailsRequest.product.planId = _selectedPlan.programID;
                    paymentDetailsRequest.firstName = _nameArr[0];
                    paymentDetailsRequest.lastName = _nameArr.length > 1 ? _nameArr[1] : "";
                    paymentDetailsRequest.sucessUrl = language == "ltr" ? _selectedPlan.successURLEng : _selectedPlan.successURLHeb;

                    return this.paymentApiService.sendPaymentRequest(paymentDetailsRequest, 'credit2000');
                })
            )
                .subscribe((paymentResult) => {
                    if (!this.trialCheck && paymentResult && paymentResult.approvalURL) { // If NOT trial
                        //window.open(paymentResult.approvalURL);
                        //this.router.navigateByUrl(paymentResult.approvalURL);
                        window.location.href = paymentResult.approvalURL;
                    } else {
                        if (this.trialCheck) {
                            signUpUser.Language == 1 ? window.location.href = this.appConfigService.TrailRegisterHebURL : window.location.href = this.appConfigService.TrailRegisterEngURL;
                        }
                        else {
                            this.router.navigate(["login", "registerconfirm", signUpUser.Email]);
                        }
                    }
                    this.sharedService.setBusy(false);
                }, (err) => {
                    this.isBusy = false;
                    let result = new Errors(err.error);
                    this.errorsArr.push(`SERVER_ERROR.${result.errorCode}`);

                    // TODO - Take care when payment fails;
                },
                    () => { this.isBusy = false; });
        }
    }

    resolved(captchaResponse: string) {
        this.captchaResponse = captchaResponse;
      //  console.log(`Resolved response token: ${captchaResponse}`);
    }

}
