import { Injectable } from '@angular/core';
import { Meta } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root'
})
export class AppConfigService {
  private appConfig: any;
  private hubConfig: any;
  private registerUrlConfig: any;
  private mainUrl: any;
  private redirectTime: any;
  private shouldSaveOtpLocalStorageConfig: string;
  private otpExpirationInHoursConfig: string;
  private heAAAUrlConfig: string;
  private enAAAUrlConfig: string;
  private signerUrlConfig: string;
  private checkVersionInMinutesConfig: string;

  private idpURLConfig: string;
  private idpClientIdConfig: string;
  private otpTimeConfig: any;

  private NAME: string = 'name='
  constructor(private meta: Meta) { }

  // loadAppConfig2() {
  //   this.appConfig = this.meta.getTag(`${this.NAME}data-api-endpoint`)
  //   console.log(this.appConfig)
  //   this.hubConfig = this.meta.getTag(`${this.NAME}data-hub-endpoint`)
  //   console.log(this.hubConfig)
  //   this.registerUrlConfig = this.meta.getTag(`${this.NAME}register-endpoint`)
  //   console.log(this.registerUrlConfig)
  //   this.redirectTime = this.meta.getTag(`${this.NAME}redirect-time`)
  //   console.log(this.redirectTime)
  //   this.otpExpirationInHoursConfig = this.meta.getTag(`${this.NAME}otp-expiration-in-hours`).content
  //   this.shouldSaveOtpLocalStorageConfig = this.meta.getTag(`${this.NAME}should-save-otp-local-storage`).content
  //   this.heAAAUrlConfig = this.meta.getTag(`${this.NAME}he-aaa-endpoint`).content
  //   this.enAAAUrlConfig = this.meta.getTag(`${this.NAME}en-aaa-endpoint`).content
  //   this.signerUrlConfig = this.meta.getTag(`${this.NAME}signer-endpoint`).content
  //   this.checkVersionInMinutesConfig = this.meta.getTag(`${this.NAME}check-version-in-minutes`).content
  //   this.idpURLConfig = this.meta.getTag(`${this.NAME}idp-url`).content
  //   this.idpClientIdConfig = this.meta.getTag(`${this.NAME}idp-client-id`).content

  //   this.mainUrl = document.getElementsByTagName('base')[0].href;
  // }

  loadAppConfig() {
    let metaTages = document.getElementsByTagName('meta');
    for (let i = 0; i < metaTages.length; i++) {
      let url = metaTages[i].getAttribute('data-api-endpoint');
      if (url != null) {
        this.appConfig = url;
      }

      let hubUrl = metaTages[i].getAttribute('data-hub-endpoint');
      if (hubUrl != null) {
        this.hubConfig = hubUrl;
      }

      let registerationUrl = metaTages[i].getAttribute('register-endpoint');
      if (registerationUrl != null) {
        this.registerUrlConfig = registerationUrl;
      }

      let redirectTimeout = metaTages[i].getAttribute('redirect-time');
      if (redirectTimeout != null) {
        this.redirectTime = redirectTimeout;
      }

      let otpExpirationInHours = metaTages[i].getAttribute('otp-expiration-in-hours');
      if (otpExpirationInHours != null) {
        this.otpExpirationInHoursConfig = otpExpirationInHours;
      }

      let shouldSaveOtpLocalStorage = metaTages[i].getAttribute('should-save-otp-local-storage');
      if (shouldSaveOtpLocalStorage != null) {
        this.shouldSaveOtpLocalStorageConfig = shouldSaveOtpLocalStorage;
      }

      let heAAAUrl = metaTages[i].getAttribute('he-aaa-endpoint');
      if (heAAAUrl != null) {
        this.heAAAUrlConfig = heAAAUrl;
      }

      let enAAAUrl = metaTages[i].getAttribute('en-aaa-endpoint');
      if (enAAAUrl != null) {
        this.enAAAUrlConfig = enAAAUrl;
      }

      let signerUrl = metaTages[i].getAttribute('signer-endpoint');
      if (signerUrl != null) {
        this.signerUrlConfig = signerUrl;
      }

      let checkVersionInMinutes = metaTages[i].getAttribute('check-version-in-minutes');
      if (checkVersionInMinutes != null) {
        this.checkVersionInMinutesConfig = checkVersionInMinutes;
      }

      let dataidpURL = metaTages[i].getAttribute('idp-url');
      if (dataidpURL != null) {
        this.idpURLConfig = dataidpURL;
      }
      let dataidpClientId = metaTages[i].getAttribute('idp-client-id');
      if (dataidpClientId != null) {
        this.idpClientIdConfig = dataidpClientId;
      }
      let otpTimeMeta = metaTages[i].getAttribute('otp');
      if (otpTimeMeta != null) {
        this.otpTimeConfig = otpTimeMeta;
      }
      else{
        this.otpTimeConfig ="5";
      }


    }

    this.mainUrl = document.getElementsByTagName('base')[0].href;
  }

  get otpTime() {
    if (!this.otpTimeConfig) {
      throw Error('Config file not loaded!');
    }
    return parseInt(this.otpTimeConfig)
  }

  get apiUrl() {
    if (!this.appConfig) {
      throw Error('Config file not loaded!');
    }

    return this.appConfig;
  }

  get hubUrl() {
    if (!this.hubConfig) {
      throw Error('Config file not loaded!');
    }

    return this.hubConfig;
  }

  get registerUrl() {
    if (!this.registerUrlConfig) {
      throw Error('Config file not loaded!');
    }

    return this.registerUrlConfig;
  }

  get redirectTimeoutconfig() {
    if (this.redirectTime) {
      let tonumber = Number(this.redirectTime);
      if (!tonumber) {
        return tonumber;
      }
    }
    return 5000;
  }

  get baseUrl() {
    return this.mainUrl;
  }

  get otpExpirationInHours() {
    return this.otpExpirationInHoursConfig;
  }

  get shouldSaveOtpLocalStorage() {
    return this.shouldSaveOtpLocalStorageConfig;
  }


  get heAAAUrl() {
    return this.heAAAUrlConfig;
  }

  get enAAAUrl() {
    return this.enAAAUrlConfig;
  }

  get signerUrl() {
    if (!this.signerUrlConfig) {
      throw Error('Config file not loaded!');
    }
    return this.signerUrlConfig;
  }

  get checkVersionInMinutes() {
    return parseInt(this.checkVersionInMinutesConfig);
  }


  get idpURL() {
    if (!this.idpURLConfig) {
      throw Error('Config file not loaded!');
    }
    return this.idpURLConfig;
  }
  get idpClientId() {
    if (!this.idpClientIdConfig) {
      throw Error('Config file not loaded!');
    }
    return this.idpClientIdConfig;
  }



}
