import { Component, ElementRef, HostListener, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Errors } from '@models/error/errors.model';
import { MergeTemplatesRequest } from '@models/template-api/merge-templates-request.model';
import { TemplateInfo } from '@models/template-api/template-info.model';
import { Store } from '@ngrx/store';
import { TranslateService } from '@ngx-translate/core';
import { SharedService } from '@services/shared.service';
import { TemplateApiService } from '@services/template-api.service';
import { IAppState } from '@state/app-state.interface';
import * as selectActions from "@state/actions/selection.actions";
import * as documentActions from "@state/actions/document.actions";

@Component({
  selector: 'sgn-merge-files',
  templateUrl: 'merge-files.component.html',
  styles: [
  ]
})
export class MergeFilesComponent implements OnInit {

  public amuntOfFiles: number = 2;
  public filesSelectorInfo: [string, string][];
  public file1Value = "";
  public file2Value = "";
  public file3Value = "";
  public file4Value = "";
  public file5Value = "";
  public collentionName = "";
  public errorMsg = "";
  mergeTemplatesRequest: MergeTemplatesRequest = new MergeTemplatesRequest();

  public clear(itemId) {
    this.setValue(itemId, "");
  }
  constructor(public router: Router,
    private translate: TranslateService,
    private templateApiService: TemplateApiService,
    private sharedService: SharedService,
    private store: Store<IAppState>,
  ) {
    this.filesSelectorInfo = [];
    this.translate.get(['MERGE_FILES.FILE_1', 'MERGE_FILES.FILE_2', 'MERGE_FILES.FILE_3', 'MERGE_FILES.FILE_4', 'MERGE_FILES.FILE_5']).subscribe((res: object) => {
      let keys = Object.keys(res);
      this.filesSelectorInfo.push(["ct-is-signer1", res[keys[0]]]);
      this.filesSelectorInfo.push(["ct-is-signer2", res[keys[1]]]);
      this.filesSelectorInfo.push(["ct-is-signer3", res[keys[2]]]);
      this.filesSelectorInfo.push(["ct-is-signer4", res[keys[3]]]);
      this.filesSelectorInfo.push(["ct-is-signer5", res[keys[4]]]);

      return { unsubscribe() { } };
    });
  }



  public dataSelected(itemId, data) {
    this.setValue(itemId, data);

  }


  setValue(itemId, data) {
    switch (itemId) {
      case 1:
        {
          this.file1Value = data;
          break;
        }
      case 2:
        {
          this.file2Value = data;
          break;
        }
      case 3:
        {
          this.file3Value = data;
          break;
        }
      case 4:
        {
          this.file4Value = data;
          break;
        }
      case 5:
        {
          this.file5Value = data;
          break;
        }
    }
  }

  ngOnInit(): void {
    this.collentionName = "";
    this.mergeTemplatesRequest.name = this.collentionName;

  }


  public uploadData() {
    this.mergeTemplatesRequest.name = this.collentionName;
    this.mergeTemplatesRequest.templates = [];
    this.mergeTemplatesRequest.templates.push(this.file1Value);
    this.mergeTemplatesRequest.templates.push(this.file2Value);
    if (this.amuntOfFiles > 2) {
      this.mergeTemplatesRequest.templates.push(this.file3Value);
    }
    if (this.amuntOfFiles > 3) {
      this.mergeTemplatesRequest.templates.push(this.file4Value);
    }
    if (this.amuntOfFiles > 4) {
      this.mergeTemplatesRequest.templates.push(this.file5Value);
    }

    this.sharedService.setBusy(true, "TEMPLATE.MERGEING_FILES");
    this.templateApiService.mergeDocuments(this.mergeTemplatesRequest).subscribe(
      (res) => {

        if (!this.mergeTemplatesRequest.isOneTimeUseTemplate) {
          this.router.navigate(["/dashboard", "templates"]);
        }
        else {
          // Clear previous selected templates before adding the new one
          this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}));
          
          let templateInfo = new TemplateInfo();
          templateInfo.templateId = res.templateId.toString();
          templateInfo.name = res.templateName;
          this.store.dispatch(new selectActions.SelectTemplateAction({ templateInfo }));

          // Set the new document name
          this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: res.templateName }));

          this.router.navigate(["dashboard", "selectsigners"])

        }
      },
      (err) => {
        if (err.status == 0) {
          this.sharedService.setErrorAlert(new Errors(err));
        }
        else {
          this.sharedService.setErrorAlert(new Errors(err.error));
        }
        this.sharedService.setBusy(false);
      },
      () => {
        this.sharedService.setBusy(false)
      }


    )


  }
  public createTemplate() {
    if (!this.validateInput()) {
      return;
    }
    this.mergeTemplatesRequest = new MergeTemplatesRequest();
    this.mergeTemplatesRequest.isOneTimeUseTemplate = false;
    this.uploadData();
  }
  public sendDocument() {
    if (!this.validateInput()) {
      return;
    }
    this.mergeTemplatesRequest = new MergeTemplatesRequest();
    this.mergeTemplatesRequest.isOneTimeUseTemplate = true;

    this.uploadData();
  }

  public validateInput() {
    this.errorMsg = "";
    if (this.collentionName.trim() == "") {
      this.errorMsg = this.translate.instant('ERROR.INPUT.MERGE_FILE_MISSING_COLLECTION_NAME');
      return false;
    }
    if (this.file1Value.trim() == "" || this.file2Value.trim() == "") {
      this.errorMsg = this.translate.instant('ERROR.INPUT.MERGE_FILE_MISSING_COLLECTION_DATA');
      return false;
    }
    if (this.amuntOfFiles > 2) {
      if (this.file3Value.trim() == "") {
        this.errorMsg = this.translate.instant('ERROR.INPUT.MERGE_FILE_MISSING_COLLECTION_DATA');
        return false;
      }
    }
    if (this.amuntOfFiles > 3) {
      if (this.file4Value.trim() == "") {
        this.errorMsg = this.translate.instant('ERROR.INPUT.MERGE_FILE_MISSING_COLLECTION_DATA');
        return false;
      }
    }
    if (this.amuntOfFiles > 4) {
      if (this.file5Value.trim() == "") {
        this.errorMsg = this.translate.instant('ERROR.INPUT.MERGE_FILE_MISSING_COLLECTION_DATA');
        return false;
      }
    }
    return true;

  }
  private isUUID(uuid) {
    let s = "" + uuid;

    let item = s.match('^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$');
    if (item === null) {
      return false;
    }
    return true;
  }
}
