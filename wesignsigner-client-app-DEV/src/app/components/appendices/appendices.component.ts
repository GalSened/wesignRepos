import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { DocumentsService } from 'src/app/services/documents.service';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-appendices',
  templateUrl: './appendices.component.html',
  styleUrls: ['./appendices.component.scss']
})
export class AppendicesComponent implements OnInit {

  @Input() public showAppendices: boolean;
  @Input() public appendices: string[] = [];
  @Input() public token: string;
  @Output() public onClose = new EventEmitter<void>();
  constructor(private documentsService: DocumentsService,
    private stateService: StateService) {
    this.stateService.state$.subscribe(
      (data) => {
        this.appendices = data.senderAppendices;

        if (data.OpenAppendicesFromRemote) {
          this.showAppendices = true;
        }
      });
  }

  ngOnInit(): void {
  }

  downloadAppendix(appendix) {
    //console.log("downlod "+ appendix);
    this.documentsService.downloadAppendix(this.token, appendix)
  }

  close() {
    this.showAppendices = false;
    this.stateService.openAppendicesFromRemote(false);
    this.onClose.emit();
  }
}