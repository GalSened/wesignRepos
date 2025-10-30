import { Component, Input, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { DocumentsService } from 'src/app/services/documents.service';

@Component({
  selector: 'app-success-page',
  templateUrl: './success-page.component.html',
  styleUrls: ['./success-page.component.scss']
})
export class SuccessPageComponent implements OnInit {
  @Input() public token: string = "";
  @Input() public showDownloadButton: boolean;

  constructor(private translate: TranslateService, private documentsService: DocumentsService) { }

  ngOnInit(): void {
  }

  public download() {
    this.documentsService.downloadDocument(this.token);
  }

  public changeLang(lang) {
    let code = lang == "English" || lang == "אנגלית" ? "en" : "he";
    let direction = code == "en" ? "ltr" : "rtl";
    this.translate.use(code);

    document.getElementsByTagName("html")[0].setAttribute("dir", direction);
    document.getElementsByTagName("html")[0].setAttribute("lang", code);
  }
}