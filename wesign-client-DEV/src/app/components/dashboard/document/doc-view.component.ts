import { Component, OnInit, ChangeDetectorRef, ViewChild } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { DocumentPageDataResult, DocumentPagesRangeResponse } from '@models/template-api/page-data-result.model';
import { Actions, ofType } from "@ngrx/effects";
import { Store } from '@ngrx/store';
import { DocumentApiService } from "@services/document-api.service";
import { SharedService } from "@services/shared.service";
import * as alertActions from "@state/actions/alert.actions";
import { AppState, IAppState } from '@state/app-state.interface';
import { forkJoin, Observable } from "rxjs";
import { bufferCount, combineAll, concatMap, first, map, mergeAll, switchMap, take, toArray, zip, zipAll } from "rxjs/operators";
import { Location } from '@angular/common';
import { Errors } from '@models/error/errors.model';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
    selector: "sgn-doc-view",
    templateUrl: "doc-view.component.html",
})

export class DocViewComponent implements OnInit {

    public collectionId: string;

    public zmodel: { ZoomLevel: number, Bright: boolean } = { ZoomLevel: 1, Bright: false };
    public pages$: Observable<number[]>;
    public documentName: string;
    isDark: boolean = true;
    isBusy: boolean;
    pagesCount: number = 0;
    public documentPages: DocumentPageDataResult[] = [];
    public currentPage : number = 1;
public changePageNumber(pageNumer)
{
    this.currentPage = pageNumer
   
}
    constructor(
        private route: ActivatedRoute,
        private documentApi: DocumentApiService,
        private sharedService: SharedService,
        private actions: Actions,
        public router: Router,
        private documentApiService: DocumentApiService,
        private changeDetectorRef: ChangeDetectorRef,
        private location: Location,
        private store: Store<IAppState>,
        private sanitizer: DomSanitizer
    ) { }

    public ngOnInit() {


        this.store.select<any>('appstate').pipe(
            take(1),
            map((s: AppState) => {
                this.collectionId = s.documentsCollectionId
                return s.documentsStatus.filter(x => x.documentCollectionId == this.collectionId)[0].documentsIds
            }
            ),
            mergeAll(),
            concatMap((doumentId) => {
                this.sharedService.setBusy(true,  "GLOBAL.LOADING");
                return this.documentApi.pageCount(this.collectionId, doumentId)
            }),
            switchMap((res) => {
                this.sharedService.setBusy(false);
                this.pagesCount += res.pagesCount;
                let limit = this.pagesCount < 100 ? 10 : Math.ceil(this.pagesCount / 10)
                let arrSize = Math.ceil(res.pagesCount / limit);
                this.sharedService.setLoadingBanner(Math.ceil(Number(arrSize / 2)), "DOCUMENT.LOADING");
                let offset = 1;
                let arrtest = new Array(arrSize).fill(res.documentId).map(
                    (id) => {
                        let res = { id, offset, limit };
                        offset = offset + limit;
                        return res;
                    });
                let pagesRequests = arrtest.map(arra => this.documentApi.getPages(this.collectionId, arra.id, arra.offset, arra.limit, true))

                return pagesRequests;

            }),
            combineAll(),

        ).subscribe(async (data: DocumentPagesRangeResponse[]) => {
            data.forEach(element => {
                this.documentPages = this.documentPages.concat(element.documentPages);
                this.documentPages.forEach(page => {
                    if (page.ocrString) {
                        page.ocrHtml = this.sanitizer.bypassSecurityTrustHtml(page.ocrString || '');
                    }
                });
            });
            this.pages$ = new Observable(x => x.next(Array.from(Array(this.pagesCount).keys()).map((v) => v + 1)));
        },
            err => {
                this.sharedService.setBusy(false);
                this.sharedService.setErrorAlert(new Errors(err.error));
            }
        );
    }

    public next() {
        this.router.navigate(["/dashboard", "main", "documents"]);
    }

    public download() {
        this.isBusy = true;
        this.documentApiService.downloadDocument(this.collectionId);
        this.isBusy = false;
    }

    private setLoadingBanner(pageCount: number) {
        this.sharedService.setBusy(true, "DOCUMENT.LOADING");
        this.actions.pipe(
            ofType(alertActions.PAGE_LOADED),
            bufferCount(pageCount),
            first(),
        ).subscribe((_) => {
            this.sharedService.setBusy(false);
        }, _err => {
            this.sharedService.setBusy(false);

        });
    }

    changeBackgroud() {
        this.isDark = !this.isDark;
    }

    back() {
        this.location.back();
    }
}
