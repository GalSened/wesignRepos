import { Injectable } from "@angular/core";

@Injectable()
export class AppConfigService {

  private appVersionConfig: string = "";
  private appConfig: string = "";
  private signerAppConfig: string = "";
  private paymentConfig: string = "";
  private reCaptchaKeyConfig: string = "";
  private hubConfig: string = "";
  private SSOLoginURL: string = "";
  private trailRegisterHebURL: string = "";
  private trailRegisterEngURL: string = "";
  private liteChangePlanHebUrl: string = "";
  private liteChangePlanEngUrl: string = "";
  private emailSupport: string = "";
  private heAAAUrl: string = "";
  private enAAAUrl: string = "";
  private userUrlConfig: string = "";
  private checkVersionInMinutesConfig: string = "";
  private contactUsEnglishLink: string = "";
  private contactUsHebrewLink: string = "";
  private comsignGuidesLink: string = "";

  constructor() {
    this.loadAppConfig();
  }

  loadAppConfig() {
    const baseAppVersion = document.querySelector("meta[app-version]");
    if (baseAppVersion) {
      this.appVersionConfig = baseAppVersion.getAttribute('app-version');
    }
    const baseApiUrl = document.querySelector("meta[data-api-endpoint]");
    if (baseApiUrl) {
      this.appConfig = baseApiUrl.getAttribute('data-api-endpoint');
    }
    const baseSignerApiUrl = document.querySelector("meta[data-signer-api-endpoint]");
    if (baseSignerApiUrl) {
      this.signerAppConfig = baseSignerApiUrl.getAttribute('data-signer-api-endpoint');
    }

    const reCaptchaKey = document.querySelector("meta[data-reCaptcha-site-key]");
    if (reCaptchaKey) {
      this.reCaptchaKeyConfig = reCaptchaKey.getAttribute('data-reCaptcha-site-key');
    }

    const paymentUrl = document.querySelector("meta[data-payment-api-endpoint]");
    if (paymentUrl) {
      this.paymentConfig = paymentUrl.getAttribute('data-payment-api-endpoint');
    }
    const hubUrl = document.querySelector("meta[data-hub-endpoint]");
    if (hubUrl) {
      this.hubConfig = hubUrl.getAttribute('data-hub-endpoint');
    }

    const samlUrl = document.querySelector("meta[SSO-login-endpoint]");
    if (samlUrl) {
      this.SSOLoginURL = samlUrl.getAttribute('SSO-login-endpoint');
    }

    const trailHeb = document.querySelector("meta[trail-register-thank-you-Heb]");
    if (trailHeb) {

      this.trailRegisterHebURL = trailHeb.getAttribute('trail-register-thank-you-Heb');
    }

    const trailEng = document.querySelector("meta[trail-register-thank-you-Eng]");
    if (trailEng) {
      this.trailRegisterEngURL = trailEng.getAttribute('trail-register-thank-you-Eng');
    }

    const liteChangePlanHeb = document.querySelector("meta[lite-change-plan-heb-url]");
    if (liteChangePlanHeb) {
      this.liteChangePlanHebUrl = liteChangePlanHeb.getAttribute('lite-change-plan-heb-url');
    }

    const liteChangePlanEng = document.querySelector("meta[lite-change-plan-eng-url]");
    if (liteChangePlanEng) {
      this.liteChangePlanEngUrl = liteChangePlanEng.getAttribute('lite-change-plan-eng-url');
    }

    const email_support = document.querySelector("meta[email-support]");
    if (email_support) {
      this.emailSupport = email_support.getAttribute('email-support');
    }

    const he_AAA_Url = document.querySelector("meta[he-aaa-endpoint]");
    if (he_AAA_Url) {
      this.heAAAUrl = he_AAA_Url.getAttribute('he-aaa-endpoint');
    }

    const en_AAA_Url = document.querySelector("meta[en-aaa-endpoint]");
    if (en_AAA_Url) {
      this.enAAAUrl = en_AAA_Url.getAttribute('en-aaa-endpoint');
    }

    let userUrl = document.querySelector("meta[user-endpoint]");
    if (userUrl != null) {
      this.userUrlConfig = userUrl.getAttribute('user-endpoint');
    }

    let checkVersionInMinutes = document.querySelector("meta[check-version-in-minutes]");
    if (checkVersionInMinutes != null) {
      this.checkVersionInMinutesConfig = checkVersionInMinutes.getAttribute('check-version-in-minutes');
    }

    const contact_us_english_link = document.querySelector("meta[contact-us-english-link]");
    if (contact_us_english_link) {
      this.contactUsEnglishLink = contact_us_english_link.getAttribute('contact-us-english-link');
    }
    const contact_us_hebrew_link = document.querySelector("meta[contact-us-hebrew-link]");
    if (contact_us_hebrew_link) {
      this.contactUsHebrewLink = contact_us_hebrew_link.getAttribute('contact-us-hebrew-link');
    }

    const comsign_guides_link = document.querySelector("meta[comsign-guides-link]");
    if (comsign_guides_link) {
      this.comsignGuidesLink = comsign_guides_link.getAttribute('comsign-guides-link');
    }

  }
  get HeAAAUrl() {
    // if (!this.heAAAUrl)
    // {
    //   throw Error('Config file not loaded! heAAAUrl');
    // }
    return this.heAAAUrl
  }

  get EnAAAUrl() {
    // if (!this.enAAAUrl)
    // {
    //   throw Error('Config file not loaded! enAAAUrl');
    // }
    return this.enAAAUrl
  }

  get ContactUsEnglishLink() {
    if (!this.contactUsEnglishLink) {
      throw Error('Config file not loaded! contactUsEnglishLink');
    }
    return this.contactUsEnglishLink
  }
  get ContactUsHebrewLink() {
    if (!this.contactUsHebrewLink) {
      throw Error('Config file not loaded! contactUsHebrewLink');
    }
    return this.contactUsHebrewLink
  }

  get ComsignGuidesLink() {
    if (!this.comsignGuidesLink) {
      throw Error('Config file not loaded! comsignGuidesLink');
    }
    return this.comsignGuidesLink;
  }

  get EmailSupport() {
    if (!this.emailSupport) {
      throw Error('Config file not loaded! emailSupport');
    }
    return this.emailSupport
  }

  get LiteChangePlanHebUrl() {
    if (!this.liteChangePlanHebUrl) {
      throw Error('Config file not loaded! liteChangePlanHebUrl');
    }
    return this.liteChangePlanHebUrl
  }

  get LiteChangePlanEngUrl() {
    if (!this.liteChangePlanEngUrl) {
      throw Error('Config file not loaded! liteChangePlanEngUrl');
    }
    return this.liteChangePlanEngUrl
  }

  get TrailRegisterHebURL() {
    if (!this.trailRegisterHebURL) {
      throw Error('Config file not loaded! trailRegisterHebURL');
    }

    return this.trailRegisterHebURL;
  }
  get TrailRegisterEngURL() {
    if (!this.trailRegisterEngURL) {
      throw Error('Config file not loaded! trailRegisterEngURL');
    }

    return this.trailRegisterEngURL;
  }
  get SSOLogin() {
    if (!this.SSOLoginURL) {
      throw Error('Config file not loaded!');
    }

    return this.SSOLoginURL;
  }

  get appVersion() {
    if (!this.appVersionConfig) {
      throw Error('App version not loaded!');
    }

    return this.appVersionConfig;
  }

  get apiUrl() {
    if (!this.appConfig) {
      throw Error('Config file not loaded!');
    }

    return this.appConfig;
  }

  get signerApiUrl() {
    if (!this.signerAppConfig) {
      throw Error('Config file not loaded!');
    }

    return this.signerAppConfig;
  }

  get paymentapi() {
    if (!this.paymentConfig) {
      throw Error('Config file not loaded!');
    }

    return this.paymentConfig;
  }

  get reCaptchaKey() {
    if (!this.reCaptchaKeyConfig) {
      throw Error('Config file not loaded!');
    }

    return this.reCaptchaKeyConfig;
  }

  get hubapi() {
    if (!this.hubConfig) {
      throw Error('Config file not loaded!');
    }

    return this.hubConfig;
  }

  get userUrl() {
    if (!this.userUrlConfig) {
      throw Error('Config file not loaded!');
    }
    return this.userUrlConfig;
  }

  get checkVersionInMinutes() {
    return parseInt(this.checkVersionInMinutesConfig);
  }
}