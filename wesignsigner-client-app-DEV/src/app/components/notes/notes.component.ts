import { Component, Input, OnInit } from '@angular/core';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-notes',
  templateUrl: './notes.component.html',
  styleUrls: ['./notes.component.scss']
})
export class NotesComponent implements OnInit {

  @Input() public showNotes: boolean;
  @Input() public senderNotes: string;

  public signerNotes: string;

  constructor(private stateService: StateService) { }

  ngOnInit(): void {
    this.stateService.state$.subscribe(
      (data) => {
        this.signerNotes = data.signerNotes;

        if (data.OpenNotesFromRemote) {
          this.showNotes = true;
        }
      });
  }

  public close() {
    let value = this.signerNotes;
    this.stateService.openNotesFromRemote(false);

    this.stateService.setSignerNotes(value);
    this.showNotes = false;
  }
}