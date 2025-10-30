import { AfterViewInit, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { AppState } from 'src/app/models/state/app-state.model';
import { LiveEventsService } from 'src/app/services/live-events.service';
import { StateService } from 'src/app/services/state.service';
import { DeviceDetectorService } from 'ngx-device-detector';
import { DocumentColectionMode } from 'src/app/enums/document-colection-mode.enum';

@Component({
  selector: 'app-view-settings',
  templateUrl: './view-settings.component.html',
  styleUrls: ['./view-settings.component.scss']
})
export class ViewSettingsComponent implements OnInit, AfterViewInit {

  isOnlineMode = false;
  isDark = true;
  mainUrl: string = "";
  @Input() currentPage = 1;
  @Input() public totalPages = 1;
  @Output() public changeBackgroundEvent = new EventEmitter<string>();
  @Output() public changeZoomLevelEvent = new EventEmitter<{ zoomLevel: number }>();
  @Input() public showNotes: boolean = false;
  @Input() public showAppendixesIcon: boolean = false;
  @Input() public showAttchments: boolean = false;
  @Input() public token = "";
  @Input() public senderView: boolean = false;
  zoomLevel = 1;
  isDesktopDevice: boolean;
  state: AppState;
  notes: string;
  lastPingDate: Date = new Date(-8640000000000000);
  greenPing = false;
  showNotesClicked = false;
  showAppendicesClicked = false;
  appendices: string[];

  constructor(private router: Router, private liveEventsService: LiveEventsService, private stateService: StateService, private deviceService: DeviceDetectorService) { }

  ngAfterViewInit(): void {
    setTimeout(x => {
      if (!this.isDesktopDevice) {
        this.onZoomOptionChanged();
        this.zoomOut(0.375);
      }
    }, 1000);
    this.stateService.state$.subscribe(
      data => {
        this.notes = data.senderNotes;
        this.appendices = data.senderAppendices;
        if (data.documentCollectionData && data.documentCollectionData.mode == DocumentColectionMode.Online && !this.isOnlineMode) {
          setTimeout(() => {
            this.isOnlineMode = true;
          }, 1000);
        }
      });

    setTimeout(() => {
      this.liveEventsService.listenToOnPingToOthers();
      this.liveEventsService.pingFromOthersSubject.subscribe((isSendFromSenderView) => {
        if (isSendFromSenderView != this.senderView) {
          this.lastPingDate = new Date();
        }
      })
      setInterval(() => {
        if (this.state.documentCollectionData && this.state.documentCollectionData.mode == DocumentColectionMode.Online) {
          this.liveEventsService.pingToOthers(this.state.Token, this.senderView);
          if (!this.isOnlineMode) {
            this.isOnlineMode = true;
          }

        }
      }, 3000);
      setInterval(() => {
        if (this.state.documentCollectionData && this.state.documentCollectionData.mode == DocumentColectionMode.Online) {
          // check last ping time was over 5 seconds      

          let diff = Math.abs(Math.abs((new Date()).getTime()) - Math.abs(this.lastPingDate.getTime()));
          if (diff > 5000) {
            this.greenPing = false;

          }
          else {

            this.greenPing = true;
          }
        }
      }, 2000)
    }, 3000);
  }

  ngOnInit(): void {
    this.mainUrl = this.router.url;
    this.stateService.state$.subscribe((data) => {
      this.state = data;
    });
    this.isDesktopDevice = this.deviceService.isDesktop();

  }

  public changeBackground() {
    this.isDark = !this.isDark
    this.changeBackgroundEvent.emit();
  }

  public zoomIn() {
    if (this.zoomLevel < 1.8) {
      //this.zoomLevel += 0.25;
      this.zoomLevel += 0.125;
    }
    this.changeZoomLevelEvent.emit({ zoomLevel: this.zoomLevel });
    if (this.state.documentCollectionData && this.state.documentCollectionData.mode == DocumentColectionMode.Online) {
      this.liveEventsService.zoom(this.state.Token, this.zoomLevel);
    }
  }

  zoomOut(offset = 0.125) {
    if (this.zoomLevel > 0.5) {
      //this.zoomLevel -= 0.25;
      this.zoomLevel -= offset;
    }
    this.changeZoomLevelEvent.emit({ zoomLevel: this.zoomLevel });
    if (this.state.documentCollectionData && this.state.documentCollectionData.mode == DocumentColectionMode.Online) {
      this.liveEventsService.zoom(this.state.Token, this.zoomLevel);
    }
  }

  showAppendixes() {
    this.stateService.openAppendicesFromRemote(true);

  }
  readNotes() {
    this.stateService.openNotesFromRemote(true);

  }
  addAttchments() {
    this.stateService.openAtthmentsFromRemote(true);
  }

  onZoomOptionChanged() {
    let option = (<HTMLSelectElement>document.getElementById("zoomOptions"))?.value;
    if (option != undefined) {
      option = option.replace("%", "");
      this.zoomLevel = parseInt(option) / 100;
      this.changeZoomLevelEvent.emit({ zoomLevel: this.zoomLevel });

      if (this.state.documentCollectionData && this.state.documentCollectionData.mode == DocumentColectionMode.Online) {
        this.liveEventsService.zoom(this.state.Token, this.zoomLevel);
      }
    }
  }

  goToPage(pageNum: number) {
    pageNum = Math.min(Math.max(pageNum, 1), this.totalPages);
    let pageId = 'page-' + pageNum;

    this.router.navigate([this.mainUrl], { fragment: pageId }).then(res => {
      let pageElement = document.getElementById(pageId);
      if (pageElement != undefined) {
        pageElement.scrollIntoView(true);
        // var scrolly = window.scrollY
        // if (scrolly) {
        //   window.scroll(0, 0);
        // }
        this.currentPage = pageNum;
      }
    });
  }
}