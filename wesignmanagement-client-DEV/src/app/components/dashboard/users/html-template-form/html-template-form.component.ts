import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import * as saveAs from 'file-saver';
import { BannerAlertComponent } from 'src/app/components/shared/banner-alert/banner-alert.component';
import { CreateHtmlTemplate } from 'src/app/models/create-html-template.model';
import { Errors } from 'src/app/models/error/errors.model';
import { ToolTip } from 'src/app/models/tool-tip.model';
import { User } from 'src/app/models/user.model';
import { SharedService } from 'src/app/services/shared.service';
import { UsersApiService } from 'src/app/services/users-api.service';

@Component({
  selector: 'app-html-template-form',
  templateUrl: './html-template-form.component.html',
  styleUrls: ['./html-template-form.component.css']
})
export class HtmlTemplateFormComponent implements OnInit {
  
  private SUCCESS = 1;
  public templateId : string;
  public htmlFileBase64String: string="";
  public javascriptFileBase64String: string ="";

  public submitText : string;
  public errorMessage : string;
  
  public isError: boolean = false;
  public isBusy: boolean = false;
  public toolTipInfo: ToolTip = new ToolTip();
  public userName : string = "";
  dataModel  = [];
  @Input() selectedTemplate:string = "";
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;
  @Output() public hideUserForm = new EventEmitter<number>();
  @ViewChild("htmlFile") htmlFileEl: ElementRef;
  @ViewChild("javascriptFile") javascriptFileEl: ElementRef;

  file: any;
  user: User;
  
  constructor(private userApiService: UsersApiService,
    private sharedService: SharedService) { }

  ngOnInit(): void {
    this.sharedService.getToolTipsInfo().subscribe(
      (data) => {
        this.toolTipInfo.jsTemplateFile = (data as ToolTip).jsTemplateFile;
        this.toolTipInfo.htmlTemplateFile = (data as ToolTip).htmlTemplateFile;
      });
     this.bannerAlert.hideBannerAlert();
  }

  public LoadUser(user: User){
    this.user = user;
    this.userName = user.name;
    this.userApiService.readTemplates(this.user.id).subscribe(
      (dict : { [id: string] : string; })=>{
        var arr = [];

        for (var key in dict) {
            if (dict.hasOwnProperty(key)) {
                arr.push(  `${key}_${ dict[key]}`  );
            }
        }

        this.dataModel = arr;
      },
      error=>{

      }
    )
  }

  public cancel() {
    this.templateId = "";
    this.hideUserForm.emit(0);

  }

  public submit(){
    this.isError = false;
    let templateId = this.selectedTemplate.split('_')[0];
    let request = new CreateHtmlTemplate();
    request.templateId = templateId;
    request.htmlBase64File = this.htmlFileBase64String;
    request.jsBase64File = this.javascriptFileBase64String;
    request.userId = this.user.id;
    this.isBusy = true;   
    this.userApiService.createHtmlTemplate(request).subscribe(
      x=>{
        this.isError = false;
        this.isBusy = false;
        this.bannerAlert.showBannerAlert("HTML ans JS files successfully created for template", this.SUCCESS);
      },
      err=>{
        this.isBusy = false;
        let result = new Errors(err.error);
        this.isError = true;
        this.errorMessage = this.sharedService.getErrorMessage(result);        
      })

  }

  public removeHtmlFile(){
    this.htmlFileEl.nativeElement.value = "";
    this.htmlFileBase64String = "";
  }

  public downloadHtmlFile(){
    if (this.htmlFileBase64String != null && this.htmlFileBase64String != '') {
      fetch(this.htmlFileBase64String)
        .then(res => res.blob())
        .then((blob) => {
          const fileName = 'doc.html';
          saveAs(blob, fileName);
        }
        );
    }
  }

  public htmlFileDropped(){
    if (this.htmlFileEl.nativeElement.files.length > 0) {
      this.file = this.htmlFileEl.nativeElement.files[0];
      if (String(this.file.type).endsWith("html")) {
        const reader = new FileReader();
        reader.readAsDataURL(this.file);
        reader.onload = () => {
          this.htmlFileBase64String = reader.result.toString();
        };
      }
    } else {
      this.file = null;
    }
  }


  public javascriptFileDropped(){
    if (this.javascriptFileEl.nativeElement.files.length > 0) {
      this.file = this.javascriptFileEl.nativeElement.files[0];
      if (String(this.file.type).endsWith("javascript")) {
        const reader = new FileReader();
        reader.readAsDataURL(this.file);
        reader.onload = () => {
          this.javascriptFileBase64String = reader.result.toString();
        };
      }
    } else {
      this.file = null;
    }
  }

  public removeJavaScriptFile(){
    this.javascriptFileEl.nativeElement.value = "";
    this.javascriptFileBase64String = "";
  }

  public downloadJavaScriptFile(){
    if (this.javascriptFileBase64String != null && this.javascriptFileBase64String != '') {
      fetch(this.javascriptFileBase64String)
        .then(res => res.blob())
        .then((blob) => {
          const fileName = 'doc.js';
          saveAs(blob, fileName);
        }
        );
    }
  }

}
