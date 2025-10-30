import { AfterViewInit, ChangeDetectorRef, ElementRef, HostListener, Renderer2, RendererStyleFlags2 } from '@angular/core';
import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DeviceDetectorService } from 'ngx-device-detector';
import { SignatureFieldKind } from 'src/app/enums/signature-field-kind.enum';
import { SignatureType } from 'src/app/enums/signature-type.enum';
import { StoreOperationType } from 'src/app/enums/store-operation-type.enum';
import { WeSignFieldType } from 'src/app/enums/we-sign-field-type.enum';
import { signatureField } from 'src/app/models/pdffields/field.model';
import { FieldRequest } from 'src/app/models/requests/fields-request.model';
import { SmartCardService } from 'src/app/services/smart-card.service';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-signature-field',
  templateUrl: './signature-field.component.html',
  styleUrls: ['./signature-field.component.scss']
})
export class SignatureFieldComponent implements OnInit, AfterViewInit {

  @Input() signature: signatureField;
  @Input() documentId: string;
  @Input() public pageHeight: number;
  @Input() public pageWidth: number;
  @Input() public disabled: boolean;
  top: number;
  left: number;
  width: number;
  height: number;
  fontSize = 12;
  isSenderLink: boolean = false;
  token: string;
  sigKind = SignatureFieldKind.Simple;
  SignatureFieldKind = SignatureFieldKind;

  constructor(private stateService: StateService, private elRef: ElementRef,
    private router: Router, protected renderer: Renderer2, private cdr: ChangeDetectorRef,
    private route: ActivatedRoute, private deviceService: DeviceDetectorService, private smartCardServiceAPI: SmartCardService) { }

  ngAfterViewInit(): void {
    let x = (parseInt(this.elRef.nativeElement.firstElementChild.offsetHeight) * 10 + parseInt(this.elRef.nativeElement.firstElementChild.offsetWidth)) / 100;
    this.fontSize = x;
    this.cdr.detectChanges();
  }

  ngOnInit(): void {
    // console.log("In signature-field ngOnInit");

    this.route.paramMap.subscribe(params => {
      // console.log("In signature-field ngOnInit route.paramMap.subscribe");
      this.token = params.get("id");
    });
    this.height = this.signature.height * this.pageHeight;
    this.width = this.signature.width * this.pageWidth;
    this.top = this.signature.y * this.pageHeight;
    this.left = this.signature.x * this.pageWidth;
    this.updateSignatureImage(this.signature);
    this.sigKind = this.signature.signatureKind;
    this.stateService.singature$.subscribe(sig => {
      this.updateSignatureImage(sig);
    });
    let mainUrl = this.router.url;
    this.isSenderLink = mainUrl.includes("sender");

    this.stateService.state$.subscribe((data) => {
      let element = <HTMLInputElement>document.getElementById(this.signature.name);

      if (data.storeOperationType == StoreOperationType.SetCurrentJumpedField && data.currentJumpedField.name == this.signature.name && data.currentJumpedField.page == this.signature.page && data.currentJumpedField.yLocation == this.signature.y) {
        let button = element.getElementsByTagName("button")[0];
        element.scrollIntoView(false);
        element.classList.add("is-mandatory");
        button.focus();
      }
      else {
        if (element) {
          if (element.classList.contains("is-mandatory")) {
            element.classList.remove("is-mandatory");
          }
        }
      }
    });
  }

  private updateSignatureImage(sig) {
    if (sig && sig.image && this.signature.name == sig.name) { // multiple signature name bug possibility? 
      this.signature.image = sig.image;
      if (this.renderer != undefined) {
        this.renderer.setStyle(this.elRef.nativeElement.firstElementChild, "background-image", `url('${this.signature.image}')`);
        const flags = RendererStyleFlags2.DashCase | RendererStyleFlags2.Important;
        this.renderer.setStyle(this.elRef.nativeElement.firstElementChild, "background-position", `center`, flags);
        this.renderer.setStyle(this.elRef.nativeElement.firstElementChild, "background-size", `contain`);
        this.renderer.setStyle(this.elRef.nativeElement.firstElementChild, "background-repeat", `no-repeat`);
        if (this.elRef.nativeElement.firstElementChild.children[1]) {
          this.renderer.setStyle(this.elRef.nativeElement.firstElementChild.children[1], "display", `none`);
        }
        if (this.elRef.nativeElement.firstElementChild.children[2]) {
          this.renderer.setStyle(this.elRef.nativeElement.firstElementChild.children[2], "display", `none`);
        }
        if (this.elRef.nativeElement.firstElementChild.children[3]) {
          this.renderer.setStyle(this.elRef.nativeElement.firstElementChild.children[3], "display", `none`);
        }
      }
    }
  }

  onClearSign(event: MouseEvent) {
    event.stopPropagation();
    var el = document.getElementById(this.signature.name);
    el.style.backgroundImage = "none";
    let fieldData = new FieldRequest();
    fieldData.fieldName = this.signature.name;
    fieldData.fieldType = WeSignFieldType.SignatureField;
    fieldData.fieldValue = null;
    this.signature.image = null;
    this.stateService.setFieldData(this.documentId, fieldData);
    if (this.renderer != undefined) {
      this.renderer.removeStyle(this.elRef.nativeElement.firstElementChild, "background-image");
      this.renderer.removeStyle(this.elRef.nativeElement.firstElementChild, "background-position");
      this.renderer.removeStyle(this.elRef.nativeElement.firstElementChild, "background-size");
      if (this.elRef.nativeElement.firstElementChild.children[1]) {
        this.renderer.removeStyle(this.elRef.nativeElement.firstElementChild.children[1], "display");
      }
      if (this.elRef.nativeElement.firstElementChild.children[2]) {
        this.renderer.removeStyle(this.elRef.nativeElement.firstElementChild.children[2], "display");
      }
      if (this.elRef.nativeElement.firstElementChild.children[3]) {
        this.renderer.removeStyle(this.elRef.nativeElement.firstElementChild.children[3], "display");
      }
    }
  }

  @HostListener("click")
  public onSelected() {
    if (this.disabled) {
      return;
    }
    if (!this.isSenderLink) {
      if (this.signature.signingType != SignatureType.Graphic) {
        this.stateService.setSigningType(this.signature.signingType);
      }
      if (this.signature.signingType == SignatureType.Server) {
        this.stateService.setIsContainServerSignature(true);
      }
      if (this.signature.signingType == SignatureType.SmartCard) {
        this.smartCardServiceAPI.Init(this.token);
      }

      this.stateService.setselectedSignField({
        name: this.signature.name,
        documentId: this.documentId,
        type: this.signature.signingType,
        kind: this.signature.signatureKind,
      });
    }
  }
}