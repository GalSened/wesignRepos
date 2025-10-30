import { Component, ElementRef, OnDestroy, OnInit, ViewChild, Input, ViewChildren, QueryList, Renderer2, AfterViewInit,Output,EventEmitter } from "@angular/core";
import { Store } from "@ngrx/store";
import { UserApiService } from "@services/user-api.service";
import * as fieldActions from "@state/actions/fields.actions";
import { AppState, IAppState } from "@state/app-state.interface";
import { Subscription } from "rxjs";
import { NgxSignaturePadComponent } from '@o.krucheniuk/ngx-signature-pad';
import { SignatureField } from '@models/template-api/page-data-result.model';
import { SignatureType } from '@models/enums/signature-type.enum';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { SmartCardSigningService } from '@services/smart-card-signing.service';
import { UserProgram } from '@models/program/user-program.model';
import { PageField } from "@models/template-api/page-data-result.model";
import { SharedService } from '@services/shared.service';
import { SignatureFieldKind } from '@models/enums/signature-field-kind.enum';
import { elementAt } from 'rxjs-compat/operator/elementAt';
import { ContactApiService } from '@services/contact-api.service';
import { SignaturesImagesModel } from '@models/contacts/signature-images-result.model';
import { Modal } from '@models/modal/modal.model';
import { TranslateService } from '@ngx-translate/core';
import { userConfiguration } from '@models/account/user-configuration.model';
import { SignPadService } from '@services/sign-pad.service';
@Component({
  selector: "sgn-sign-pad",
  templateUrl: "sign-pad.component.html",
})

export class SignPadComponent implements OnInit, OnDestroy, AfterViewInit {
  public fieldName: string;
  public templateId: string;
  public field: SignatureField;
  public showError: boolean = true;
  public isSignaturePad: boolean = true;
  public isInitialsSignaturePad: boolean = false;
  isFirstClick: boolean = true;
  @Input() public color: string = null;
  @Input() public collectionId: string = null;
  public signaturesImages: string[] = [];
  public signaturesImagesToSave: string = null;
  public enableDisplaySignerNameInSignature: boolean = false;
  public show: boolean;
  public file: any = null;
  public userProgram: UserProgram;
  public userConfiguration: userConfiguration;
  public shouldSetImageToAllSigntures: boolean = false;
  @Output() shouldSetImageToAllSignturesChange = new EventEmitter<boolean>();
  shouldSaveThisSignature: boolean = false;
  public currentSelectedSection = "Draw";
  @ViewChild(NgxSignaturePadComponent, { static: true }) signaturePad: NgxSignaturePadComponent;
  @ViewChildren("area") public areaElement !: QueryList<any>;
  @ViewChildren("contactSignatureImageCanvas") public contactSignatureImageCanvasEl: QueryList<ElementRef>;
  @ViewChild("fileInput") public el: ElementRef;
  public selectedFont: string = "Kalam, cursive";
  public fonts: string[] = ["Kalam, cursive", "Times New Roman", "Arial", "Arial Narrow",
    "Courier", "Calibri"];

  public signaturePadOptions = {
    canvasHeight: "100",
    canvasWidth: "100",
    minWidth: 2,
    penColor: this.color || 'black',
  };

  private storeSelectSub: Subscription;
  currentUserName: string;
  public textSignatures: string[];
  selectedCanvasId: any;
  isBusy: boolean;
  allFields: PageField[];


  public saveContactImage: Modal = new Modal();

  constructor(private store: Store<IAppState>,
    private authService: UserApiService,
    private sharedService: SharedService,
    private selfSignApiService: SelfSignApiService,
    private smartCardServiceAPI: SmartCardSigningService,
    private contactApiService: ContactApiService,
    private signPadService: SignPadService,
    private translate: TranslateService,
    private renderer: Renderer2) {
    this.storeSelectSub = this.store.select<any>('appstate').subscribe((state: AppState) => {
      this.currentUserName = state.CurrentUserName;
      this.userProgram = state.program;
      this.userConfiguration = state.userConfiguration;
      this.enableDisplaySignerNameInSignature = state.enableDisplaySignerNameInSignature;
      if (state.SelectedSignField != undefined) {
        this.fieldName = state.SelectedSignField.FieldName;
        this.templateId = state.SelectedSignField.TemplateId;
        this.field = (state.PageFields.find(x => x.name == this.fieldName && x.templateId == this.templateId) as SignatureField);
        this.allFields = state.PageFields;

        if (this.field != undefined) {
          if (this.field.signingType == SignatureType.Server) {
            var serverNumber: number = SignatureType['Server'];
            let el = document.getElementById(serverNumber.toString());
            el.click();
          }
          if (this.field.signingType == SignatureType.SmartCard) {
            var smartCardNumber: number = SignatureType['SmartCard'];
            let el = document.getElementById(smartCardNumber.toString());
            el.click();
            this.smartCardServiceAPI.Init(this.authService.accessToken);
          }
          if (this.field.signatureKind == SignatureFieldKind.Initials) {
            this.isInitialsSignaturePad = true;
            this.changeSection('Initials');
          }
          else {
            this.changeSection('Draw');
            this.isInitialsSignaturePad = false;
          }
        }
      }
      else {
        this.fieldName = "";
        this.templateId = "";
      }
      this.show = this.fieldName != "" && this.templateId != "";
      setTimeout(_ => this.onSelected());
    });
  }


  public doSaveContactImageEvent() {
    if (this.signaturesImagesToSave && this.signaturesImagesToSave != "") {
      let signImage = new SignaturesImagesModel();
      signImage.signaturesImages = [];
      signImage.signaturesImages.push(this.signaturesImagesToSave);
      signImage.documentCollectionId = this.collectionId;
      this.contactApiService.updateSignaturesImages(signImage).subscribe(
        () => {
          this.readContactImages();
          this.signaturesImagesToSave = "";

        }
      )
    }
    this.shouldSaveThisSignature = false;
  }

  public ngOnInit() {
    this.shouldSaveThisSignature = false;
    if (this.areaElement) {
      this.signaturePadOptions.canvasHeight = this.areaElement.first.nativeElement.offsetHeight;
      this.signaturePadOptions.canvasWidth = this.areaElement.first.nativeElement.offsetWidth;
    }
    this.signaturePadOptions.penColor = this.color || 'black';
    //setTimeout(_ => this.changeFont(), 3000);
  }

  ngAfterViewInit(): void {
    this.changeSection('Draw');
    this.shouldSaveThisSignature = false;
    document.getElementById("drawButton").classList.add("is-active");

    this.readContactImages();
    //setTimeout(_ => this.changeFont(), 3000);
  }

onShouldSetImageToAllSignturesChange(value: boolean) {
  this.shouldSetImageToAllSigntures = value;
  this.shouldSetImageToAllSignturesChange.emit(value);
}

// onShouldSetImageToAllSignturesChange(value: boolean) {
//   this.shouldSetImageToAllSigntures = value;
//   this.shouldSetImageToAllSignturesValue.emit(value);
// }

  readContactImages() {
    this.contactApiService.getAllSaveSignatureForSelfSignContact(this.collectionId).subscribe((images: SignaturesImagesModel) => {
      this.signaturesImages = images.signaturesImages;
    });
  }

  selectContactSignature(index) {
    let base64Str = this.signaturesImages[index];
    let type = this.field ? this.field.signingType : SignatureType.Graphic;
    let image = new Image();
    
    image.src = base64Str;
    image.onload = () => {
      this.shouldSaveThisSignature = false;
      if (this.shouldSetImageToAllSigntures) {
        this.doSetSignatureImageToAllSignaturesField(image, base64Str, this.field.signatureKind);
      }
      else {
        let canvas = this.fixGraphicImageSizing(image, this.fieldName)
        if (this.shouldAddSignatureExtraInfo()) {
          base64Str = this.addAdditionalInformationToSignature(canvas, this.currentUserName);
        }        
        this.setSignatureImage(base64Str, type, this.field.signatureKind);
      }
   
    }
  }
  public ngOnDestroy() {
    if (this.storeSelectSub) {
      this.storeSelectSub.unsubscribe();
    }
  }

  private shouldAddSignatureExtraInfo(): boolean {
    return this.enableDisplaySignerNameInSignature && this.userConfiguration.shouldDisplayNameInSignature;
  }

  public changeFont() {
    this.clearTextSignatures();
    this.loadTextSignatures(this.selectedFont);
  }

  public clearTextSignatures() {
    let elements = document.getElementsByClassName("ct-c-initials");
    for (let i = 0; i < elements.length; i++) {
      let canvas = elements[i].children[0];
      if (canvas instanceof HTMLCanvasElement) {
        var ctx = (<HTMLCanvasElement>canvas).getContext("2d");
        ctx.clearRect(0, 0, canvas.height * 2, canvas.width * 2);
      }
    }
  }

  loadTextSignatures(font: string): void {
    let fullName = this.currentUserName.substr(0, 1).toUpperCase() + this.currentUserName.substr(1);
    this.textSignatures = [fullName];
    if (this.textSignatures[0] && this.textSignatures[0].split(" ").length < 4) {
      let firstLastName = this.textSignatures[0].split(" ");
      let firstNameWithAcronymsLastName = this.signPadService.generateAcronymFirstName(firstLastName);
      let acronyms = this.signPadService.generateAcronyms(firstLastName);
      this.textSignatures = [fullName, firstNameWithAcronymsLastName, acronyms];
    }

    for (let index = 0; index < this.textSignatures.length; index++) {
      const name = this.textSignatures[index];
      var canvas = document.getElementById(name);
      if (canvas) {

        let fontsize = 55;
        if (name.length > 8 && name.length < 12) {
          fontsize = 50
        }
        if (name.length > 12 && name.length < 18) {
          fontsize = 35
        }
        if (name.length > 18) {
          fontsize = 30
        }

        var ctx = (<HTMLCanvasElement>canvas).getContext("2d");
        ctx.font = fontsize + `px ${font}`;
        ctx.textAlign = "center";
        ctx.fillText(name, (<HTMLCanvasElement>canvas).width / 2, (<HTMLCanvasElement>canvas).height / 2);
      }
    }
  }

  public ChangedSigningType(itemIndex) {
    if (this.field) {
      this.field.signingType = itemIndex;

      //TODO change signature type in store
      this.storeSelectSub = this.store.select<any>('appstate').subscribe((state: AppState) => {
        let sig = <SignatureField>state.PageFields.find(x => x.name == this.fieldName);
        if (this.field) {
          sig.signingType = this.field.signingType;
        }
      });

      if (this.field.signingType == SignatureType.SmartCard) {
        this.smartCardServiceAPI.Init(this.authService.accessToken);
      }
    }
  }

  public clear($event) {
    $event.preventDefault();
    this.signaturePad.clear();
    this.signaturesImagesToSave = "";
  }

  public cancel($event) {

    this.clear($event);
    this.store.dispatch(new fieldActions.CancelSignFieldAction({}));
    this.signaturesImagesToSave = "";
    this.shouldSaveThisSignature = false;
  }

  public done() {
    if (!this.signaturePad) {
      return;
    }
    let type = SignatureType.Graphic;
    Array.from(document.getElementsByName("sigType")).forEach(
      (x: HTMLInputElement) => {
        if (x.checked) {
          type = Number(x.id);
        }
      }
    );
    this.removeErrorBorderFromSignatureField();

    let base64Str = this.signaturePad.isEmpty() ? "" :
      this.cropSignatureCanvas(this.signaturePad.signaturePadElement.nativeElement);
    if (base64Str == "") {
      // edit the image ---
      return;

    }
    let image = new Image();
    image.src = base64Str;
    image.onload = () => {
      this.signaturesImagesToSave = base64Str;
      if (this.shouldSaveThisSignature) {
        this.doSaveContactImageEvent();
      }
      this.shouldSaveThisSignature = false;
      if (this.shouldSetImageToAllSigntures) {
        this.doSetSignatureImageToAllSignaturesField(image, base64Str, this.field.signatureKind);
      }
      else {
        let canvas = this.fixGraphicImageSizing(image, this.fieldName)
        if (this.shouldAddSignatureExtraInfo()) {
          base64Str = this.addAdditionalInformationToSignature(canvas, this.currentUserName);
        }
        else {
          base64Str = canvas.toDataURL();
        }
        this.setSignatureImage(base64Str, type, this.field.signatureKind);
      }
    }
  }

  private removeErrorBorderFromSignatureField() {
    var el = document.querySelector(`[data-fieldname="${this.fieldName}"]`);
    el.children[0].classList.remove("is-error");
  }



  public changeSection(sectionName: string) {
    this.currentSelectedSection = sectionName;


    if (this.isInitialsSignaturePad && sectionName != 'Initials') {
      return;
    }
    var tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tabcontent");
    for (let i = 0; i < tabcontent.length; i++) {
      tabcontent[i].style.display = "none";
    }
    tablinks = document.getElementsByClassName("tablinks");
    for (let i = 0; i < tablinks.length; i++) {
      tablinks[i].className = tablinks[i].className.replace(" is-active", "");
    }
    document.getElementById(sectionName).style.display = "block";
    if (sectionName == 'Initials') {
      this.changeFont();
      setTimeout(() => {
        this.loadTextSignatures(this.selectedFont);
      }, 850);
    }


    setTimeout(() => {
      if (this.field != undefined) {
        if (this.field.signingType == SignatureType.Server) {
          var serverNumber: number = SignatureType['Server'];
          let el = document.getElementById(serverNumber.toString());

          el.click();
        }
        if (this.field.signingType == SignatureType.SmartCard) {
          var smartCardNumber: number = SignatureType['SmartCard'];
          let el = document.getElementById(smartCardNumber.toString());

          el.click();
        }
      }
    }, 200);


  }

  public onSelected() {
    if (this.isFirstClick && this.areaElement && this.areaElement.first && this.areaElement.first.nativeElement.offsetHeight > 0) {
      let el = this.areaElement.first.nativeElement.querySelector('canvas').parentElement;
      let offsetHeight = this.areaElement.first.nativeElement.offsetHeight;
      let offsetWidth = this.areaElement.first.nativeElement.offsetWidth;
      this.renderer.setStyle(el, "height", offsetHeight + 'px');
      this.renderer.setStyle(el, "width", offsetWidth + 'px');
      this.signaturePad.forceUpdate();
      this.isFirstClick = false;
    }
  }


  selcetCanvasName($event) {
    this.selectedCanvasId = $event.currentTarget.firstChild.id;
    this.showError = false;
    if (this.selectedCanvasId && this.selectedCanvasId != "") {
      var canvas = document.getElementById(this.selectedCanvasId);
      
      let base64Str = this.cropSignatureCanvas(canvas);
      let image = new Image();
      image.src = base64Str;
      image.onload = () => {
        let type = this.field ? this.field.signingType : SignatureType.Graphic;
        this.shouldSaveThisSignature = false;
        if (this.shouldSetImageToAllSigntures) {
          this.doSetSignatureImageToAllSignaturesField(image, base64Str, this.field.signatureKind);
        }
        else {
          let canvas = this.fixGraphicImageSizing(image, this.fieldName)
          if (this.shouldAddSignatureExtraInfo()) {
            base64Str = this.addAdditionalInformationToSignature(canvas, this.currentUserName);
          }
          else {
            base64Str = canvas.toDataURL();
          }
          this.setSignatureImage(base64Str, type, this.field.signatureKind);
        }
      }

    }
    else {
      this.showError = true;
    }
    this.selectedCanvasId = "";
  }



  public fileDropped($event) {
    //this.busy = true;
    if (this.el.nativeElement.files.length > 0) {
      this.file = this.el.nativeElement.files[0];
      const reader = new FileReader();
      reader.readAsDataURL(this.file);
      reader.onload = () => {
        let base64Str = reader.result.toString();
        let image = new Image();
        image.src = base64Str;
        image.onload = () => {
          if (this.shouldSaveThisSignature) {
            this.signaturesImagesToSave = base64Str;
            this.doSaveContactImageEvent();
          }
          if (this.shouldSetImageToAllSigntures) {
            this.doSetSignatureImageToAllSignaturesField(image, base64Str, this.field.signatureKind);
            this.file = null;
          }
          else {
            base64Str = this.fixGraphicImageSizing(image, this.fieldName).toDataURL();

            let type = this.field ? this.field.signingType : SignatureType.Graphic;
            if (this.shouldAddSignatureExtraInfo()) {
              this.drawExtraInfoWithGraphic(base64Str, type);
            }
            else {
              this.setSignatureImage(base64Str, type, this.field.signatureKind);
              this.file = null;
            }
          }


        }
      };
    }
  }

  private fixGraphicImageSizing(image: HTMLImageElement, fieldName: string): HTMLCanvasElement {
    let CANVAS_DEFAULT_WIDTH = 600;
    let CANVAS_DEFAULT_HEIGHT = 252;
    let canvas = document.createElement('canvas');
    let ctx = canvas.getContext('2d');
    if (fieldName) {
      let div = document.querySelector(`[data-fieldName=${fieldName}]`);
      if (div) {
        CANVAS_DEFAULT_WIDTH = div.getBoundingClientRect().width;
        CANVAS_DEFAULT_HEIGHT = div.getBoundingClientRect().height;
      }
    }
    const imageWidth = image.width;
    const imageHeight = image.height;
    canvas.width = CANVAS_DEFAULT_WIDTH;
    canvas.height = CANVAS_DEFAULT_HEIGHT;
    const scale = Math.min(CANVAS_DEFAULT_WIDTH / imageWidth, CANVAS_DEFAULT_HEIGHT / imageHeight);
    //  ctx.drawImage(img, 0, 0, CANVAS_DEFAULT_WIDTH, CANVAS_DEFAULT_HEIGHT);
    const scaledWidth = imageWidth * scale;
    const scaledHeight = imageHeight * scale;
    const x = (CANVAS_DEFAULT_WIDTH - scaledWidth) / 2;
    const y = (CANVAS_DEFAULT_HEIGHT - scaledHeight) / 2;
    ctx.drawImage(image, x, y, scaledWidth, scaledHeight);
    return canvas;
  }

  private drawExtraInfoWithGraphic(base64Str: string, type: SignatureType) {
    let CANVAS_DEFAULT_WIDTH = 600;
    let CANVAS_DEFAULT_HEIGHT = 252;
    let image = new Image();
    image.src = base64Str;
    image.onload = () => {
      let canvas = document.createElement('canvas');
      let ctx = canvas.getContext('2d');
      if (this.fieldName) {
        let div = document.querySelector(`[data-fieldName=${this.fieldName}]`);
        if (div) {
          CANVAS_DEFAULT_WIDTH = div.getBoundingClientRect().width;
          CANVAS_DEFAULT_HEIGHT = div.getBoundingClientRect().height;
        }
      }
      canvas.width = CANVAS_DEFAULT_WIDTH;
      canvas.height = CANVAS_DEFAULT_HEIGHT;
      ctx.drawImage(image, 0, 0, canvas.width, canvas.height);
      let base64WithInfoStr = this.addAdditionalInformationToSignature(canvas, this.currentUserName);
      this.setSignatureImage(base64WithInfoStr, type, this.field.signatureKind);
      this.file = null;
    }
  }

  setSignatureImage(image, type: SignatureType, kind: SignatureFieldKind) {
    this.removeErrorBorderFromSignatureField();
    // need to reduce here

    if (this.signPadService.calc_image_size(image) > 150) {

      this.reduce_image_file_size(image, type, kind)


    }
    else {
      this.doSetSignatureImage(image, type, kind);
    }
  }

  private doSetSignatureImageToAllSignaturesField(image, base64Str, kind: SignatureFieldKind) {
    this.allFields.forEach(
      field => {
        if (field instanceof SignatureField) {
          if (field.signatureKind == kind) {
            let fixedCanvas = this.fixGraphicImageSizing(image, field.name);
            let fixedCanvasStr = fixedCanvas.toDataURL();
            if (this.shouldAddSignatureExtraInfo()) {
              fixedCanvasStr = this.addAdditionalInformationToSignature(fixedCanvas, this.currentUserName);
            }


            this.store.dispatch(new fieldActions.SetSignFieldImageAction(
              {
                SelectedSignField: {
                  FieldName: field.name,
                  SignFieldImage: fixedCanvasStr,
                  TemplateId: field.templateId,
                  Type: field.signingType
                }
              }));
          }
        }
      });
      //this.shouldSetImageToAllSignturesValue.emit(this.shouldSetImageToAllSigntures);
    this.changeSection('Draw');
    this.signaturePad.clear();
  }


  private doSetSignatureImage(image, type: SignatureType, kind: SignatureFieldKind) {

    this.store.dispatch(new fieldActions.SetSignFieldImageAction(
      {
        SelectedSignField: {
          FieldName: this.fieldName,
          SignFieldImage: image,
          TemplateId: this.templateId,
          Type: type
        }
      }));


    this.changeSection('Draw');
    this.signaturePad.clear();
  }
  public fixImageSizeAndDispatch(field: SignatureField, image: string) {

    // read the html elemnet with the SELECTOR[data-.....]
    // if not exist - send the data as we sended before... with the original image
    //if exist load the image - at the on load- need to resize the image - (canvas) and send the new image to SignFieldImage 
    let img = new Image();
    img.src = image;
    img.onload = () => {
      let canvas = document.createElement('canvas');
      let ctx = canvas.getContext('2d');
      if (field.name) {
        let div = document.querySelector(`[data-fieldName=${field.name}]`);
        if (div) {
          canvas.width = div.getBoundingClientRect().width;
          canvas.height = div.getBoundingClientRect().height;
        }
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
      }
      //read the canvas size of  the given name -
      this.store.dispatch(new fieldActions.SetSignFieldImageAction(
        {
          SelectedSignField: {
            FieldName: field.name,
            SignFieldImage: canvas.toDataURL(),
            TemplateId: field.templateId,
            Type: field.signingType
          }
        }));
    }

  }

  public downloadSmartCardInstaller() {
    this.isBusy = true;
    this.selfSignApiService.downloadSmartCardDesktopClientInstaller().subscribe(
      (data) => {
        let fn = data.headers.get("x-file-name");
        const filename = decodeURIComponent(fn) ? decodeURIComponent(fn) : "setup";
        const blob = new Blob([data.body], { type: "application/octet-stream" });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.target = "_blank";
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        this.isBusy = false;
      },
      (err) => {
        //TODO if failed to download, redirect to another page 
        this.isBusy = false;
        let ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
        this.sharedService.setErrorAlert(ex);
        this.sharedService.setBusy(false);
      });;
  }

  public reduce_image_file_size(base64Str: string, type: SignatureType, kind: SignatureFieldKind,
    MAX_WIDTH = 450, MAX_HEIGHT = 450) {
    let img = new Image();
    img.src = base64Str;
    img.onload = () => {
      let canvas = document.createElement('canvas');
      let width = img.width;
      let height = img.height;

      if (width > height && width > MAX_WIDTH) {
        height *= MAX_WIDTH / width;
        width = MAX_WIDTH;
      } else if (height > MAX_HEIGHT) {
        width *= MAX_HEIGHT / height;
        height = MAX_HEIGHT;
      }
      canvas.width = width;
      canvas.height = height;
      let ctx = canvas.getContext('2d');
      ctx.drawImage(img, 0, 0, width, height);
      this.doSetSignatureImage(canvas.toDataURL(), type, kind);
    }
  }

  private addAdditionalInformationToSignature(canvas: HTMLCanvasElement, signerName: string) {
    let canvasWithInfo: HTMLCanvasElement = this.signPadService.addSignatureExtraInformation(signerName, canvas);
    return canvasWithInfo.toDataURL();
  }


  private cropSignatureCanvas(canvas) {
    var croppedCanvas = document.createElement('canvas'),
      croppedCtx = croppedCanvas.getContext('2d');

    croppedCanvas.width = canvas.width;
    croppedCanvas.height = canvas.height;
    croppedCtx.drawImage(canvas, 0, 0);

    // Next do the actual cropping
    var w = croppedCanvas.width,
      h = croppedCanvas.height,
      pix = { x: [], y: [] },
      imageData = croppedCtx.getImageData(0, 0, croppedCanvas.width, croppedCanvas.height),
      x, y, index;

    for (y = 0; y < h; y++) {
      for (x = 0; x < w; x++) {
        index = (y * w + x) * 4;
        if (imageData.data[index + 3] > 0) {
          pix.x.push(x);
          pix.y.push(y);

        }
      }
    }
    pix.x.sort(function (a, b) { return a - b });
    pix.y.sort(function (a, b) { return a - b });
    var n = pix.x.length - 1;

    w = pix.x[n] - pix.x[0];
    h = pix.y[n] - pix.y[0];
    var cut = croppedCtx.getImageData(pix.x[0], pix.y[0], w, h);

    croppedCanvas.width = w;
    croppedCanvas.height = h;
    croppedCtx.putImageData(cut, 0, 0);

    return croppedCanvas.toDataURL();

  }

}
