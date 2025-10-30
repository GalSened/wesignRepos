import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { take, switchMap, tap, map, mergeAll, reduce } from 'rxjs/operators';
import { DocumentColectionMode } from 'src/app/enums/document-colection-mode.enum';
import { LanguageService } from 'src/app/language.service';
import { DocumentPage, DocumentPages } from 'src/app/models/responses/document-page.model';
import { AppState } from 'src/app/models/state/app-state.model';
import { DocumentsService } from 'src/app/services/documents.service';
import { LiveEventsService } from 'src/app/services/live-events.service';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-sender-view',
  templateUrl: './sender-view.component.html',
  styleUrls: ['./sender-view.component.scss']
})
export class SenderViewComponent implements OnInit {
  pages: DocumentPage[] = [];
  token = "";
  isOnlineMode = false;
  isDarkMode = true;
  zoomLevel = 1;
  state: AppState;
  loaded: boolean;
  currentPage = 1;
  
  constructor(private documentsService: DocumentsService, private route: ActivatedRoute,
    private stateService: StateService, private langService: LanguageService, private router: Router, private liveEventsService: LiveEventsService) { }
  
    changePageNumber(pageNumer) {
    this.currentPage = pageNumer;
  }

  ngOnInit(): void {
    this.stateService.showLoader = true;
    this.stateService.state$.subscribe(
      x => {
        this.state = x;
        if (x.documentCollectionData && x.documentCollectionData.mode == DocumentColectionMode.Online) {
          setTimeout(() => {
            this.liveEventsService.listenToOnChangeBackgroud();

            this.liveEventsService.backgroudSubject.subscribe(
              (isDarkMode: boolean) => {
                this.isDarkMode = isDarkMode;
              }
            );
            this.liveEventsService.listenToOnSignerDecline();
            this.liveEventsService.signerDeclineSubject.subscribe(
              () => {
                this.router.navigate(["/decline"]);
              });
          }, 3000);
        }
      });

    this.route.paramMap.pipe(
      take(1),
      switchMap(params => {
        this.token = params.get("id");
        this.stateService.SetDocuementToken(this.token);
        return this.documentsService.getCollectionData(this.token);
      }),
      tap(res => {
        this.stateService.setDocumentCollectionData(res.documentCollection);
        this.langService.changeLangFromNumber(res.language)
        this.isOnlineMode = res.documentCollection.mode == DocumentColectionMode.Online;
        this.stateService.showLoader = false;
        this.loaded = true;

        if (this.isOnlineMode) {
          this.liveEventsService.init();
        }
      }),
      map(res => { return res.documents }),
      mergeAll(),
      reduce((a, b) => {
        let limit = 5;
        let offset = 1;
        let arrSize = Math.ceil(b.pagesCount / limit);
        let arrtest = new Array(arrSize).fill(b.id).map(
          (id) => {
            let res = { id, offset, limit };
            offset = offset + limit;
            return res;
          });


        a.push(...arrtest.map(arra => this.documentsService.getDocumentsData(this.token, arra.id, arra.offset, arra.limit)))
        return a;
      }, []),
      switchMap(ar => forkJoin(ar))
    ).subscribe((data: DocumentPages[]) => {
      data.forEach(element => {
        this.pages = this.pages.concat(element.documentPages);
      });
    },
      error => {
        this.loaded = true;
        this.stateService.showLoader = false;
      });
  }

  public changeZoomLevel(event) {
    this.zoomLevel = event.zoomLevel;
  }

  public changeBackgroud() {
    this.isDarkMode = !this.isDarkMode;
    if (this.state.documentCollectionData && this.state.documentCollectionData.mode == DocumentColectionMode.Online) {
      this.liveEventsService.changeBackgroud(this.state.Token, this.isDarkMode);
    }
  }
}