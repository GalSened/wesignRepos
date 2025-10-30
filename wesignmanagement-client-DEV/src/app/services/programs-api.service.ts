import { environment } from 'src/environments/environment';
import { Router, RoutesRecognized } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { Injectable } from '@angular/core';
import { Program } from '../models/program.model';
import { ProgramFilter } from '../models/program-filter.model';
import { ProgramsResult } from '../models/results/programs-result.model';
import { AppConfigService } from './app-config.service';

@Injectable({ providedIn: 'root' })
export class ProgramsApiService {

    private programsApi: string;

    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
        this.programsApi = this.appConfigService.apiUrl + "/programs";
    }

    public createProgram(program: Program) {
        return this.httpClient.post(`${this.programsApi}`, program);
    }

    public Read(programFilter: ProgramFilter) {
        return this.httpClient.get<ProgramsResult>(`${this.programsApi}?key=${programFilter.key}&` +
            `offset=${programFilter.offset}&limit=${programFilter.limit}`, { observe: "response" });

    }
    public ReadProgram(programId: string) {
        return this.httpClient.get<Program>(`${this.programsApi}/${programId}`, { observe: "response" });
    }
    
    public updateProgram(program: Program) {
        return this.httpClient.put(`${this.programsApi}/${program.id}`, program);
    }

    public deleteProgram(program: Program) {
        return this.httpClient.delete(`${this.programsApi}/${program.id}`);
    }

}