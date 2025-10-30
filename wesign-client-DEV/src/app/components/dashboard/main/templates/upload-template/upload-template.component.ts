import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { FLOW_STEP } from '@models/enums/flow-step.enum';
import { Errors } from '@models/error/errors.model';
import { UploadRequest } from '@models/template-api/upload-request.model';
import { Store } from '@ngrx/store';
import { SharedService } from '@services/shared.service';
import { TemplateApiService } from '@services/template-api.service';
import { IAppState } from '@state/app-state.interface';
import * as fieldsActions from "@state/actions/fields.actions";
import * as selectActions from "@state/actions/selection.actions";
import { TemplateInfo } from '@models/template-api/template-info.model';
@Component({
  selector: 'sgn-upload-template',
  templateUrl: './upload-template.component.html',
  styles: []
})
export class UploadTemplateComponent implements OnInit , AfterViewInit{

  @Output() public hide = new EventEmitter<any>();  
  @Input() public templateMode : boolean; 
  public file: any;
  public base64defaultTemplate: string="";
  public name: any;
  public name2: any;
  public name3: any;
  public base64placeHolderTemplate: string="";
  public base64placeHolderMataData: string="";
  public mode: any;
  public isBusy: boolean = true;
  public isInProcess: boolean = false;
  public isFileLoded: boolean = false;
  // public acceptTypes : string = "image/*,.pdf,.docx";
  public acceptTypes : string = ".jpg,.jpeg,.pdf,.docx,.png";
  public xmlAcceptTypes : string = ".xml";
  public fileUpladed : boolean = false;
  

  constructor(private templateApiService: TemplateApiService,
    private store: Store<IAppState>,
    private sharedService: SharedService,
    private router: Router) { }
  ngAfterViewInit(): void {
    this.isBusy = false;
    if(this.templateMode)
    {
      document.getElementById("defaultOpen").click();
    }
    else
    {
      document.getElementById("placeHolderOpen").click();
    }
  }

  ngOnInit() {
    console.log("Inside upload-template");
  }

  openCity(evt, cityName) {
    this.mode = cityName;
    if(this.mode=="Default" && this.base64defaultTemplate ==""){
      this.isBusy = true;
    }else if(this.mode=="Default" && this.base64defaultTemplate !=""){
      this.isBusy = false;
    }else if(this.mode == "Placeholders" &&( this.base64placeHolderTemplate =="" || this.base64placeHolderMataData=="")){
      this.isBusy = true;
    }
    else if(this.mode == "Placeholders" &&( this.base64placeHolderTemplate !="" && this.base64placeHolderMataData!="")){
      this.isBusy = false;
    }
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tabcontent");
    for (i = 0; i < tabcontent.length; i++) {
      tabcontent[i].style.display = "none";
    }
    tablinks = document.getElementsByClassName("tablinks");
    for (i = 0; i < tablinks.length; i++) {
      tablinks[i].className = tablinks[i].className.replace(" is-active", "");
    }
    document.getElementById(cityName).style.display = "block";
    if(evt){
      evt.currentTarget.className += " is-active";
    }
  }

  public close() {
    this.sharedService.setBusy(false);
    this.hide.emit();
  }

  public submit() {
    this.isInProcess = true;
    this.isBusy = true;
    this.isFileLoded = true;
    const uploadRequest = new UploadRequest();
    if(this.mode=="Default"){
      uploadRequest.Name = this.name;
      uploadRequest.Base64File = this.base64defaultTemplate;
    }else if(this.mode == "Placeholders"){
      uploadRequest.Name = this.name2;
      uploadRequest.Base64File = this.base64placeHolderTemplate;
      uploadRequest.MetaData = this.base64placeHolderMataData;
    }
    
    if(this.templateMode)
    {
      this.sharedService.setBusy(true, "TEMPLATE.UPLOADING");
    }
    else if(this.mode == "Placeholders"){
    
      this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
      uploadRequest.IsOneTimeUseTemplate = true;
    }
    this.templateApiService.upload(uploadRequest).subscribe((data) => {
      if(this.templateMode)
      {
        this.sharedService.setFlowState("template", FLOW_STEP.TEMPLATE_EDIT);
        this.router.navigate(["dashboard", "template", "edit", `${data.templateId}`]);
        this.sharedService.setBusy(false);
      }
      else
      {
        this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}))
        this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}))
        let templateInfo  = new TemplateInfo();
        templateInfo.name = data.templateName;
        templateInfo.templateId = data.templateId;        
        this.store.dispatch(new selectActions.SelectTemplateAction({ templateInfo: templateInfo }))
        this.sharedService.setFlowState("none", FLOW_STEP.NONE);
        this.router.navigate(["dashboard", "selectsigners"]);
        this.sharedService.setBusy(false);
      }
      
      this.hide.emit();
      this.isBusy = false;
      this.isInProcess = false;
      this.isFileLoded = false;
    }, (error) => {
      this.sharedService.setErrorAlert(new Errors(error.error));
      this.sharedService.setBusy(false);
      this.isBusy = false;
      this.isInProcess = false;
      this.isFileLoded = false;
    });
  }
  
  public fileDropped(file) {
    this.name = this.limitFileName(file.name);
    let reader = new FileReader();
    reader.readAsDataURL(file);
    this.sharedService.setBusy(true);
    reader.onload = () => {
      this.base64defaultTemplate = reader.result.toString();
      this.sharedService.setBusy(false);
    };
    this.isBusy = false;
  }

  public fileDropped2(file) {
    this.name2 = this.limitFileName(file.name);
    let reader = new FileReader();
    reader.readAsDataURL(file);
    this.sharedService.setBusy(true);
    reader.onload = () => {
      this.base64placeHolderTemplate = reader.result.toString();
      this.sharedService.setBusy(false);
    };
    if (this.base64placeHolderMataData != "") {
      this.isBusy = false;
    }
  }

  public fileDropped3(file) {
    this.name3 = this.limitFileName(file.name);
    let reader = new FileReader();
    reader.readAsDataURL(file);
    this.sharedService.setBusy(true);
    reader.onload = () => {
      this.base64placeHolderMataData = reader.result.toString();
      this.sharedService.setBusy(false);
    };

    if (this.base64placeHolderTemplate != "") {
      this.isBusy = false;
    }
  }

  
  private limitFileName(fileName: string, maxLength: number = 50) {
    // Extract the file extension
    const fileExtension = fileName.substring(fileName.lastIndexOf('.'));

    // Ensure there's an extension and that it's valid
    if (fileExtension && fileExtension.length > 0) {
      // Get the part of the file name before the extension
      const baseFileName = fileName.substring(0, fileName.lastIndexOf('.'));

      // If the base file name is longer than the max length minus the extension length
      if (baseFileName.length > maxLength - fileExtension.length) {
        // Truncate the base file name to fit within the maxLength
        return baseFileName.substring(0, maxLength - fileExtension.length) + fileExtension;
      }
    }

    // If no truncation needed, return the original file name
    return fileName;
  }

  public clean(){
    this.file = undefined;
    this.base64defaultTemplate ="";
    this.name ="";
    this.isBusy = true;
  }

  public clean2(){
    this.base64placeHolderTemplate ="";
    this.name2 ="";
    this.isBusy = true;
  }

  public clean3(){
    this.base64placeHolderMataData ="";
    this.name3 ="";
    this.isBusy = true;

  }
  
}
