import { Component, Input, OnInit, QueryList, ViewChildren } from '@angular/core';
import { Attachment } from 'src/app/models/responses/attachment.model';
import { StateService } from 'src/app/services/state.service';
import { AttachmentComponent } from '../attachment/attachment.component';

@Component({
  selector: 'app-attachments',
  templateUrl: './attachments.component.html',
  styleUrls: ['./attachments.component.scss']
})
export class AttachmentsComponent implements OnInit {

  @Input() public attachments: Attachment[] = [];
  @Input() public showAttachments: boolean;
  @ViewChildren(AttachmentComponent) childrens!: QueryList<AttachmentComponent>;

  constructor(private stateService: StateService) {

    this.stateService.state$.subscribe(
      (data) => {
        if (data.OpenAtthmentsFromRemote) {
          this.showAttachments = true;
        }
      });
  }

  ngOnInit(): void {
  }

  cancel() {
    this.showAttachments = false;
    this.stateService.openAtthmentsFromRemote(false);
  }

  submit() {

    this.childrens.forEach(element => {
      element.saveAttachment();
    });

    this.showAttachments = false;
    this.stateService.openAtthmentsFromRemote(false);
  }
}