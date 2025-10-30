import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnInit, Output, Renderer2, ViewChild } from '@angular/core';
import { DeviceDetectorService } from 'ngx-device-detector';
import { DocumentColectionMode } from 'src/app/enums/document-colection-mode.enum';
import { AppState } from 'src/app/models/state/app-state.model';
import { LiveEventsService } from 'src/app/services/live-events.service';
import { StateService } from 'src/app/services/state.service';
import { Debounce } from 'angular-debounce-throttle';


@Component({
  selector: 'app-documents-group',
  templateUrl: './documents-group.component.html',
  styleUrls: ['./documents-group.component.scss']
})
export class DocumentsGroupComponent implements OnInit, AfterViewInit {

  @ViewChild("document", { static: false }) public el: ElementRef;
  @Input() zoomLevel: number = 1;
  @Input() public pages: any;
  @Input() public isDark: boolean = true;
  @Input() public isSender: boolean = false;
  public state: AppState;
  private currentPage: number = 1;
  @Output() public changePageScroll = new EventEmitter<number>();

  constructor(private liveEventsService: LiveEventsService,
    private stateService: StateService,
    private cdr: ChangeDetectorRef,
    protected renderer: Renderer2,
    private deviceService: DeviceDetectorService) {
  }

  ngOnInit(): void {
    this.stateService.state$.subscribe((data) => {
      this.state = data;
      if (data.documentCollectionData) {
        this.liveEventsService.init();

        setTimeout(() => {
          this.liveEventsService.connect(this.state.Token);
          if (data.documentCollectionData.mode == DocumentColectionMode.Online) {
            this.liveEventsService.zoomSubject.subscribe(
              (zoomLevel: number) => {
                this.zoomLevel = zoomLevel;
              }
            );

            this.liveEventsService.listenToOnScroll(this.el);
            this.liveEventsService.listenToOnFieldDataChanged();
            this.liveEventsService.listenToOnZoom();
          }

          this.liveEventsService.listenToOnFinishSigning();

        }, 2000); // Will exe once, after a 3 second.
      }
    });

  }
  public height: number;

  ngAfterViewInit() {
    if (!this.deviceService.isDesktop()) {
      this.zoomLevel = 0.6;
      let secondHeader = (<HTMLElement><any>document.getElementsByClassName("ct-c-titlebar is-user-sign"))[0].offsetHeight;
      let firstHeader = (<HTMLElement><any>document.getElementById("firstHeader")).offsetHeight;
      let viewSettingNav = (<HTMLElement><any>document.getElementById("viewSettingNav")).offsetHeight;
      let innerScrollHeigth = screen.height - secondHeader - firstHeader - viewSettingNav;
      this.height = innerScrollHeigth + 350;
      this.renderer.setStyle(this.el.nativeElement, "height", this.height + "px");
      this.cdr.detectChanges();
    }


  }


  public changePageNumber(pageNumer) {
    this.currentPage = pageNumer;
    this.changePageScroll.emit(pageNumer);

  }

  @Debounce(30)
  onScroll(event) {
    if (this.state.documentCollectionData && this.state.documentCollectionData.mode == DocumentColectionMode.Online) {
      if (this.liveEventsService.isScrollFromrRemote && this.currentPage != this.liveEventsService.lastScrollPage) {
        this.changePageNumber(this.liveEventsService.lastScrollPage)
      }
      this.liveEventsService.scroll(this.state.Token, event, this.currentPage);
      // this.liveEventsService.setScrollValues(this.state.Token, event); ---> future use
    }
  }
}
