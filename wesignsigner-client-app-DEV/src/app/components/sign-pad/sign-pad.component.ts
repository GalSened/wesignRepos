import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnInit, Output, QueryList, Renderer2, ViewChild, ViewChildren } from '@angular/core';
import { AppState } from 'src/app/models/state/app-state.model';
import { StateService } from 'src/app/services/state.service';
import { NgxSignaturePadComponent } from '@o.krucheniuk/ngx-signature-pad';
import { FieldRequest } from 'src/app/models/requests/fields-request.model';
import { WeSignFieldType } from 'src/app/enums/we-sign-field-type.enum';
import { SignatureType } from 'src/app/enums/signature-type.enum';
import { DocumentsService } from 'src/app/services/documents.service';
import { ContactsApiService } from 'src/app/services/contacts-api.service';
import { TranslateService } from '@ngx-translate/core';
import { StoreOperationType } from 'src/app/enums/store-operation-type.enum';
import { SignatureFieldKind } from 'src/app/enums/signature-field-kind.enum';
import { SignaturesImagesModel } from 'src/app/models/responses/signatures-images.model';
import { SignPadService } from 'src/app/services/sign-pad.service';

@Component({

  selector: 'app-sign-pad',
  templateUrl: './sign-pad.component.html',
  styleUrls: ['./sign-pad.component.scss']
})
export class SignPadComponent implements OnInit, AfterViewInit {

  public state: AppState;
  @Input() public color: string = "";
  @Input() public token: string = "";
  @Input() public displaySignerNameInSignature: boolean;
  @Input() public shouldDisplayMeaningOfSignature: boolean;
  @Output() public shouldSetImageToAllSignaturesChange = new EventEmitter<boolean>();
  @Output() public closeSpinner = new EventEmitter<any>();
  @Output() public showSpinner = new EventEmitter<any>();

  @ViewChildren("area") public areaElement !: QueryList<any>;
  isFirstClick: boolean = true;
  public signField = { name: "", documentId: "", kind: SignatureFieldKind.Simple };
  public isBusy: boolean = false;
  public showSaveSignaturePopup: boolean = false;
  public textSignatures: string[];
  public showMeaningOfSignature: boolean;
  public file: any = null;
  public selectedCanvasId: string;
  public SignatureFieldKind = SignatureFieldKind;
  public selectedFont: string = "Kalam, cursive";
  public fonts: string[] = ["Kalam, cursive", "Times New Roman", "Arial", "Arial Narrow",
    "Courier", "Calibri"];

  @ViewChild(NgxSignaturePadComponent) signaturePad: NgxSignaturePadComponent;
  @ViewChild("fileInput", { static: false }) public el: ElementRef;
  @ViewChild("canvas", { static: false }) public canvasEl: ElementRef;
  @ViewChildren("contactSignatureImageCanvas") public contactSignatureImageCanvasEl: QueryList<ElementRef>;

  selectedMeaningOfSignature = "";
  imageBeforeChange;
  showError: boolean;
  showSuccessMessage: boolean;
  alertMessage: any;
  signaturesImages: string[];
  loadedImage: string;
  shouldSetImageToAllSigntures: boolean;
  APIloaded = false;
  shouldSaveThisSignature = false;
  signerName: string = "";

  public signaturePadOptions = {
    canvasHeight: "100",
    canvasWidth: "100",
    minWidth: 2,
    penColor: 'black',
  };

  constructor(private documentsService: DocumentsService, private contactsApiService: ContactsApiService, private cdref: ChangeDetectorRef,
    private stateService: StateService, private signPadService: SignPadService, private translate: TranslateService, private renderer: Renderer2) { }

  ngAfterViewInit(): void {
    this.changeSection('Draw');

    if (document.getElementById("drawButton")) {
      document.getElementById("drawButton").classList.add("is-active");
    }

    if (this.signaturePad) {
      this.signaturePad.clear();
    }
  }

  ngAfterContentChecked() {
    this.cdref.detectChanges();
  }

  meaningOfSignatureCancelClick() {
    this.cancel(null);
  }

  meaningOfSignatureOkClick(selectedMeaning: string) {
    if (selectedMeaning != "") {
      this.selectedMeaningOfSignature = selectedMeaning;
      this.showMeaningOfSignature = false;
      this.ngAfterViewInit();
      setTimeout(_ => this.onSelected());
    }
  }

  GetShuoldShowMeaningOfSignature() {
    return this.showMeaningOfSignature;
  }

  ngOnInit(): void {
    this.stateService.state$.subscribe(
      x => {
        this.state = x
        this.signerName = this.signerName;
        this.signField.name = x.selectedSignField.name;
        this.signField.documentId = x.selectedSignField.documentId;
        this.signField.kind = x.selectedSignField.kind;
        setTimeout(_ => this.onSelected());
        if (!this.textSignatures) {
          setTimeout(_ => this.loadTextSignatures(this.selectedFont), 3000);
        }
        setTimeout(_ => this.loadContactSignatures(), 3000);

        if (this.signaturePadOptions.penColor != this.color) {
          this.setSignaturePadOptions();
        }

        if (x.storeOperationType == StoreOperationType.CloseSaveSignatureForFutureUse) {
          this.closeSaveSignaturePopup();
          this.stateService.clearStoreOperationType();
        }

        if (this.signField.kind == SignatureFieldKind.Initials) {
          this.changeSection('Initials');
        }
        else {
          this.changeSection('Draw');
        }

        if (!this.state.OauthNeeded && !this.APIloaded) {
          this.APIloaded = true;
          this.contactsApiService.readSignaturesImages(this.token).subscribe(
            x => {

              this.signaturesImages = x.signaturesImages;
            });
        }
      });
  }

  private setSignaturePadOptions() {
    this.signaturePadOptions = {
      canvasHeight: "100",
      canvasWidth: "100",
      minWidth: 2,
      penColor: this.color,
    };
  }

  loadContactSignatures(): void {
  }

  changeFont() {
    this.clearTextSignatures();
    this.loadTextSignatures(this.selectedFont);
  }

  doClosepinner() {
    this.closeSpinner.emit();
  }

  doShowSpinner() {
    this.showSpinner.emit();
  }

  clearTextSignatures() {
    let elements = document.getElementsByClassName("ct-c-initials");

    for (let i = 0; i < elements.length; i++) {
      let canvas = elements[i].children[0];

      if (canvas instanceof HTMLCanvasElement) {
        var ctx = (<HTMLCanvasElement>canvas).getContext("2d");
        ctx.clearRect(0, 0, canvas.height * 2, canvas.width * 2);
      }
    }
  }

  isSignAvilable() {
    if ((!this.signaturePad) || (this.signaturePad && this.signaturePad.isEmpty())) {
      return true;
    }

    let data = this.signaturePad.toData();
    let pointcount = 0;
    data.forEach(element => {
      pointcount += element.points.length;
    });

    return pointcount < 15;
  }

  loadTextSignatures(font: string): void {

    if (this.state.signerName) {
      let fullName = this.state.signerName.substr(0, 1).toUpperCase() + this.state.signerName.substr(1);
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
  }

  changeSection(sectionName: string) {
    if (this.signField.kind == SignatureFieldKind.Initials && sectionName != 'Initials') {
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
    if (document.getElementById(sectionName))
      document.getElementById(sectionName).style.display = "block";
  }

  onSelected() {

    if (this.isFirstClick && this.areaElement && this.areaElement.first && this.areaElement.first.nativeElement.offsetHeight > 0) {
      let el = this.areaElement.first.nativeElement.querySelector('canvas').parentElement;
      let offsetHeight = this.areaElement.first.nativeElement.offsetHeight;
      let offsetWidth = this.areaElement.first.nativeElement.offsetWidth;
      this.renderer.setStyle(el, "height", offsetHeight + 'px');
      this.renderer.setStyle(el, "width", offsetWidth + 'px');
      this.signaturePad.forceUpdate();
      this.isFirstClick = false;
    }

    if (this.state.seal && this.areaElement && this.areaElement.first && this.areaElement.first.nativeElement.offsetHeight > 0) {
      let offsetHeight = this.areaElement.first.nativeElement.offsetHeight;
      let offsetWidth = this.areaElement.first.nativeElement.offsetWidth;
      let options = {
        width: offsetWidth,
        height: offsetHeight,
      };

      this.signaturePad.fromDataURL(this.state.seal, options);
      this.signaturePad.forceUpdate();
    }
  }

  clear($event) {
    if ($event) {
      $event.preventDefault();
    }

    if (this.signaturePad)
      this.signaturePad.clear();
  }

  cancel($event) {
    this.shouldSaveThisSignature = false;
    this.clear($event);
    this.stateService.setselectedSignField({ name: "", documentId: "", type: SignatureType.Graphic, kind: SignatureFieldKind.Simple });
  }

  done($event) {
    if (this.signaturePad && !this.signaturePad.isEmpty()) {

      let base64Str = this.cropSignatureCanvas(this.signaturePad.signaturePadElement.nativeElement);
      let image = new Image();
      image.src = base64Str;
      let documentId = this.signField.documentId;
      image.onload = () => {

        if (this.shouldSetImageToAllSigntures) {
          this.doSetImageToAllSigntures(documentId, $event, base64Str, image);
        }

        else {
          let fixedCanvas = this.fixGraphicImageSizing(image, this.state.selectedSignField.name);
          let base64WithInfoStr = this.addAdditinalInformationToSignature(fixedCanvas, this.state.signerName, this.selectedMeaningOfSignature);
          this.setSignatureImageAndReduceImageFileSize(documentId, this.state.selectedSignField.name, $event, base64Str,
            !this.isSignerInTabletMode(), base64WithInfoStr);
          this.setSignatureProcessDone();
        }
      }
    }
  }

  doSetImageToAllSigntures(documentId, $event, base64Str, image) {
    this.state.documentsData.forEach(documentData => {
      documentData.pdfFields.signatureFields.forEach(
        signatureField => {
          if (signatureField.signatureKind == this.signField.kind) {
            let fixedCanvas = this.fixGraphicImageSizing(image, signatureField.name);
            let base64WithInfoStr = this.addAdditinalInformationToSignature(fixedCanvas,
              this.state.signerName, this.selectedMeaningOfSignature);
            this.setSignatureImageAndReduceImageFileSize(documentData.documentId, signatureField.name,
              $event, base64Str, !this.isSignerInTabletMode()
            && signatureField.name == this.state.selectedSignField.name, base64WithInfoStr);
          }
        });
    });
    this.shouldSetImageToAllSignaturesChange.emit(this.shouldSetImageToAllSigntures);
    this.setSignatureProcessDone()
  }

  isSignerInTabletMode() {
    return this.state.signerMeans == "";
  }

  setSignatureImageAndReduceImageFileSize(documentId, fieldName, $event, base64Str, showSaveSignForm,
    base64WithInfoStr = null,
    isGraphic = false, MAX_WIDTH = 450, MAX_HEIGHT = 450) {
    let base64StrToPresent = base64WithInfoStr ? base64WithInfoStr : base64Str;
    if (this.signPadService.calc_image_size(base64Str) > 150) {
      let img = new Image();
      img.src = base64StrToPresent;
      img.onload = () => {
        let canvas = document.createElement('canvas');
        let width = img.width;
        let height = img.height;

        if (width > height && width > MAX_WIDTH) {
          height *= MAX_WIDTH / width;
          width = MAX_WIDTH;
        }

        else if (height > MAX_HEIGHT) {
          width *= MAX_HEIGHT / height;
          height = MAX_HEIGHT;
        }

        canvas.width = width;
        canvas.height = height;

        let ctx = canvas.getContext('2d');
        ctx.drawImage(img, 0, 0, width, height);

        let image = canvas.toDataURL();
        this.setSignatureImage($event, image, fieldName, documentId);

        if (base64WithInfoStr) {
          ctx.clearRect(0, 0, width, height);
          img.src = base64Str;
          image = canvas.toDataURL();
        }
        this.loadedImage = image;

        if (showSaveSignForm && this.shouldSaveThisSignature &&
          this.state.selectedSignField &&
          this.state.selectedSignField.name == fieldName) {
          this.saveSignature(base64Str);
          this.shouldSaveThisSignature = false;
        }

        img.onload = null;
      }
    }

    else {
      this.setSignatureImage($event, base64StrToPresent, fieldName, documentId);
      this.loadedImage = base64Str;
      if (showSaveSignForm && this.shouldSaveThisSignature &&
        this.state.selectedSignField &&
        this.state.selectedSignField.name == fieldName) {
        let image = new Image();
        image.src = base64Str;
        image.onload = () => {
          if (isGraphic)
            this.loadedImage = this.fixGraphicImageSizing(image, this.state.selectedSignField.name).toDataURL();
          this.saveSignature(base64Str);
          this.shouldSaveThisSignature = false;

        }
      }

      this.file = null;
    }
  }

  saveSignature(image: string) {
    let signaturesImages = new SignaturesImagesModel();
    signaturesImages.signaturesImages = [image];

    this.contactsApiService.updateSignaturesImages(this.token, signaturesImages)
      .subscribe(
        (res) => {
          this.closeSpinner.emit();
          this.stateService.closeSaveSignatureForFutureUse();
        },
        (err) => {
        });
  }

  setSignatureImage($event, image: string, fieldName: string, documentId: string) {
    var el = document.getElementById(fieldName);
    if (el) {
      el.classList.add("is-mandatory");
      el.classList.remove("is-error");
    }

    let fieldData = new FieldRequest();
    fieldData.fieldName = fieldName;
    fieldData.fieldType = WeSignFieldType.SignatureField;
    fieldData.fieldValue = image;
    this.stateService.setFieldData(documentId, fieldData);
  }

  setSignatureProcessDone() {

    this.stateService.setselectedSignField({ name: "", documentId: "", type: SignatureType.Graphic, kind: SignatureFieldKind.Simple });
    this.clear(null);

  }

  addAdditinalInformationToSignature(canvas, signerName, selectedMeaningOfSignature) {
    if (this.shouldDisplayMeaningOfSignature) {
      let canvasWithInfo: HTMLCanvasElement = this.signPadService.addMeaningOfSignature(signerName,
        selectedMeaningOfSignature, canvas);
      return canvasWithInfo.toDataURL();
    }
    else if (this.displaySignerNameInSignature) {
      return this.signPadService.addSignatureExtraInformation(signerName, canvas).toDataURL()
    }
    return canvas.toDataURL();
  }

  public fileDropped($event) {
    //this.busy = true;
    if (this.el.nativeElement.files.length > 0) {
      this.file = this.el.nativeElement.files[0];
      const reader = new FileReader();
      reader.readAsDataURL(this.file);
      let documentId = this.signField.documentId;
      reader.onload = () => {
        let base64Str = reader.result.toString();
        let image = new Image();
        image.src = base64Str;
        image.onload = () => {

          if (this.shouldSetImageToAllSigntures) {
            this.doSetImageToAllSigntures(documentId, $event, base64Str, image);
          }
          else {

            let fixedCanvas = this.fixGraphicImageSizing(image, this.state.selectedSignField.name);
            let base64WithInfoStr = this.addAdditinalInformationToSignature(fixedCanvas, this.state.signerName, this.selectedMeaningOfSignature);
            if (this.displaySignerNameInSignature) {
              this.setSignatureImageAndReduceImageFileSize(documentId, this.state.selectedSignField.name, $event,
                base64Str, !this.isSignerInTabletMode(), base64WithInfoStr);
            }
            else {
              this.setSignatureImageAndReduceImageFileSize(documentId, this.state.selectedSignField.name, $event,
                base64Str, !this.isSignerInTabletMode(), false);
            }
            setTimeout(() => this.setSignatureProcessDone(), 500);
          }
        }
      };
    }
  }

  fixGraphicImageSizing(image: HTMLImageElement, fieldName: string): HTMLCanvasElement {

    let CANVAS_DEFAULT_WIDTH = 600;
    let CANVAS_DEFAULT_HEIGHT = 252;
    let canvas = document.createElement('canvas');
    let ctx = canvas.getContext('2d');
    if (fieldName) {
      let div = document.getElementById(fieldName);
      if (div) {

        CANVAS_DEFAULT_WIDTH = div.clientWidth;//div.getBoundingClientRect().width;
        CANVAS_DEFAULT_HEIGHT = div.clientHeight;//div.getBoundingClientRect().height;
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

    this.imageBeforeChange = canvas;
    return canvas;
  }

  closeSaveSignaturePopup() {
    this.showSaveSignaturePopup = false;
    this.contactsApiService.readSignaturesImages(this.token).subscribe(
      x => {
        this.signaturesImages = x.signaturesImages;
      });
  }

  downloadSmartCardInstaller() {
    this.isBusy = true;
    this.documentsService.downloadSmartCardDesktopClientInstaller().subscribe(
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
        this.isBusy = false;
      });
  }

  selcetCanvasName($event) {
    this.selectedCanvasId = $event.currentTarget.firstChild.id;
    this.showError = false;
    if (this.selectedCanvasId && this.selectedCanvasId != "") {
      var canvas = document.getElementById(this.selectedCanvasId);
      let imageBase64 = this.cropSignatureCanvas(canvas);

      let img = new Image();
      img.src = imageBase64;

      let documentId = this.signField.documentId;
      img.onload = () => {

        if (this.shouldSetImageToAllSigntures) {
          this.doSetImageToAllSigntures(documentId, $event, imageBase64, img);
        }
        else {

          let fixedCanvas = this.fixGraphicImageSizing(img, this.state.selectedSignField.name);
          let base64WithInfoStr = this.addAdditinalInformationToSignature(fixedCanvas, this.state.signerName, this.selectedMeaningOfSignature);
          if (this.displaySignerNameInSignature) {
            this.setSignatureImageAndReduceImageFileSize(documentId, this.state.selectedSignField.name, $event,
              imageBase64, false, base64WithInfoStr);
          }
          else {
            this.setSignatureImageAndReduceImageFileSize(documentId, this.state.selectedSignField.name,
              $event, imageBase64, false);
          }
        }
        this.setSignatureProcessDone();
      };
    }

    else {
      this.showError = true;
    }
    this.selectedCanvasId = "";
  }

  selectContactSignature(index) {
    let base64Str = this.signaturesImages[index];
    let image = new Image();
    image.src = base64Str;
    let documentId = this.signField.documentId;
    image.onload = () => {

      if (this.shouldSetImageToAllSigntures) {
        this.doSetImageToAllSigntures(documentId, null, base64Str, image);
      }
      else {

        let fixedCanvas = this.fixGraphicImageSizing(image, this.state.selectedSignField.name);
        let base64WithInfoStr = this.addAdditinalInformationToSignature(fixedCanvas, this.state.signerName, this.selectedMeaningOfSignature);
        this.setSignatureImage(null, base64WithInfoStr, this.state.selectedSignField.name, documentId);
      }
      this.setSignatureProcessDone();
    }
  }

  cropSignatureCanvas(canvas) {
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