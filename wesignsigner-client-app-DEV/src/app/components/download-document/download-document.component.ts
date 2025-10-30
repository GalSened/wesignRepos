import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DocumentsService } from 'src/app/services/documents.service';

@Component({
  selector: 'app-download-document',
  templateUrl: './download-document.component.html',
  styleUrls: ['./download-document.component.scss']
})
export class DownloadDocumentComponent implements OnInit {

  public documentCollectionName: string = "test";
  public token: string = "";
  isBusy: boolean;

  constructor(private documentsSevice: DocumentsService,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.token = params.get("id")
    });
    this.documentsSevice.releseButton.subscribe(
      () => {
        this.isBusy = false;
      }
    );
  }

  public download() {
    this.isBusy = true;
    this.documentsSevice.downloadDocument(this.token);
  }

}
