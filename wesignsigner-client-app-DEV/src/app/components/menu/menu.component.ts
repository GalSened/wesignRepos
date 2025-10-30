import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { Attachment } from 'src/app/models/responses/attachment.model';
import { AppConfigService } from 'src/app/services/app-config.service';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent implements OnInit {
  @Input() public token = "";
  public registerUrl = "";
  @Output() public showErrorEvent = new EventEmitter<string>();
  notes: string;
  attachments: Attachment[];
  public showAttachments: boolean;
  public showNotes: boolean;
  public showAppendices: boolean;
  public showAAA: boolean;
  appendices: string[];

  constructor(private stateService: StateService, private appConfigService: AppConfigService) { }

  ngOnInit(): void {
    this.registerUrl = this.appConfigService.registerUrl;
    this.stateService.state$.subscribe(
      data => {
        this.attachments = data.attachments;
        this.notes = data.senderNotes;
        this.appendices = data.senderAppendices;
      });

    this.showAAA = this.appConfigService.enAAAUrl != "" && this.appConfigService.heAAAUrl != "";
  }

  public moveToRegister() {
    window.open(this.registerUrl, "_blank");
  }

  public decline() {
    this.stateService.showDeclineForm = true;
  }

  public readNotes() {
    this.showNotes = true;
  }

  public viewAppendices() {
    this.showAppendices = true;
  }

  public addAttachments() {
    this.showAttachments = true;
    this.stateService.setAttachmentshown();
  }

  public openAAAUrl() {
    let lang = document.getElementsByTagName("html")[0].lang;
    let url = lang == "en" ? this.appConfigService.enAAAUrl : this.appConfigService.heAAAUrl;
    window.open(url, "_blank").focus();
  }
}