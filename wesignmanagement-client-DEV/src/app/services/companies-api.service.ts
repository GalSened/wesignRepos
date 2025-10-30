import { environment } from 'src/environments/environment';
import { Router, RoutesRecognized } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { Injectable } from '@angular/core';
import { Company } from '../models/company.model';
import { Filter } from '../models/filter.model';
import { CompaniesResult } from '../models/results/companies-result.model';
import { ExpandedCompanyResult } from '../models/results/expanded-company-result.model';
import { AppConfigService } from './app-config.service';
import { Deletion } from '../models/deletion.model';

@Injectable({ providedIn: 'root' })
export class CompaniesApiService {

    private companiesApi: string;

    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
        this.companiesApi = this.appConfigService.apiUrl + "/companies";
    }

    public createCompany(company: Company) {
        return this.httpClient.post(`${this.companiesApi}`, company);
    }

    public updateCompany(company: Company) {
        return this.httpClient.put(`${this.companiesApi}/${company.id}`, company);
    }

    public Read(filter: Filter) {
        return this.httpClient.get<CompaniesResult>(`${this.companiesApi}?key=${filter.key}&` +
            `offset=${filter.offset}&limit=${filter.limit}`, { observe: "response" });

    }

    public ReadCompany(id: string, userId: string) {
        return this.httpClient.get<ExpandedCompanyResult>(`${this.companiesApi}/${id}/users/${userId}`, { observe: "response" });
    }

    public ReadCompanyDeletionConfiguration(id: string) {
        return this.httpClient.get<Deletion>(`${this.companiesApi}/${id}/deletionconfiguration`, { observe: "response" });
    }


    public deleteCompany(company: Company) {
        return this.httpClient.delete(`${this.companiesApi}/${company.id}`);
    }

    public resendResetPassword(userId: string) {
        return this.httpClient.get(`${this.companiesApi}/password/${userId}`);
    }
}