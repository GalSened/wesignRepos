import { Injectable } from '@angular/core';
import { StateService } from './state.service';
import { AppState } from '../models/state/app-state.model';
import { SignatureFieldKind } from '../enums/signature-field-kind.enum';
import { signatureField } from '../models/pdffields/field.model';
import { FieldRequest } from '../models/requests/fields-request.model';
import { WeSignFieldType } from '../enums/we-sign-field-type.enum';

@Injectable({
  providedIn: 'root'
})
export class SignPadService {
  state: AppState
  
  constructor(private stateService: StateService) {
    this.stateService.state$.subscribe(state => {
      this.state = state;
    });
  }

  private AdjustImageSizeAndSet(documentId, signatureField: signatureField) {
    let CANVAS_DEFAULT_WIDTH = 100;
    let CANVAS_DEFAULT_HEIGHT = 100;
    let div = document.getElementById(signatureField.name);

    if (!div) {
      this.setFieldData(documentId, signatureField);
      return;
    }

    CANVAS_DEFAULT_WIDTH = div.clientWidth;
    CANVAS_DEFAULT_HEIGHT = div.clientHeight;
    let img = new Image();
    img.src = signatureField.image;
    img.onload = () => {
      let canvas = document.createElement('canvas');
      let ctx = canvas.getContext('2d');
      ctx.imageSmoothingEnabled = true;
      ctx.imageSmoothingQuality = 'high';
      const imageWidth = img.width;
      const imageHeight = img.height;
      canvas.width = CANVAS_DEFAULT_WIDTH;
      canvas.height = CANVAS_DEFAULT_HEIGHT;
      const scale = Math.min(CANVAS_DEFAULT_WIDTH / imageWidth, CANVAS_DEFAULT_HEIGHT / imageHeight);
      //  ctx.drawImage(img, 0, 0, CANVAS_DEFAULT_WIDTH, CANVAS_DEFAULT_HEIGHT);
      const scaledWidth = imageWidth * scale;
      const scaledHeight = imageHeight * scale;
      const x = (CANVAS_DEFAULT_WIDTH - scaledWidth) / 2;
      const y = (CANVAS_DEFAULT_HEIGHT - scaledHeight) / 2;
      ctx.drawImage(img, x, y, scaledWidth, scaledHeight);
      signatureField.image = canvas.toDataURL('image/png', 1.0);
      this.setFieldData(documentId, signatureField);
    }
  }

  setFieldData(documentId, signatureField: signatureField) {
    let fieldData = new FieldRequest();
    fieldData.fieldName = signatureField.name;
    fieldData.fieldType = WeSignFieldType.SignatureField;
    fieldData.fieldValue = signatureField.image;
    this.stateService.setFieldData(documentId, fieldData);
  }

  setImageToAllSignatures(canvas, kind: SignatureFieldKind, addNameToSignature: boolean,
    addMeaningOfSignature: boolean, signerName: string, meaningOfsignature: string) {
    let image = canvas.toDataURL();
    if (this.state.documentsData == null) {
      return;
    }
    this.state.documentsData.forEach(documentData => {
      documentData.pdfFields.signatureFields.forEach(
        signatureField => {

          if (signatureField.signatureKind == kind) {
            if (addNameToSignature) {
              signatureField.image = this.addSignatureExtraInformation(signerName, canvas).toDataURL();
            }

            else if (addMeaningOfSignature) {
              signatureField.image = this.addMeaningOfSignature(signerName, meaningOfsignature, canvas).toDataURL();
            }

            else {
              signatureField.image = image;
            }

            if (signatureField.image) {
              this.AdjustImageSizeAndSet(documentData.documentId, signatureField);
            }
          }
        });
    });
  }

  public addMeaningOfSignature(signerName: string, meaningOfsignature: string, signatureCanvasToDraw: HTMLCanvasElement) {
    var canvas = document.createElement('canvas');
    canvas.width = signatureCanvasToDraw.width;
    canvas.height = 60;

    let context = canvas.getContext('2d');
    context.font = '22px Arial';
    context.fillStyle = "#333";

    signerName = "Name: " + signerName;
    meaningOfsignature = "Reason: " + meaningOfsignature;
    let signingDate = new Date().toLocaleTimeString();

    context.fillText(signerName, 0, 21, canvas.width * 0.9);
    context.fillText(meaningOfsignature, 0, 50, canvas.width * 0.9);
    context.fillText(signingDate, 0, 85, canvas.width * 0.9);

    var combaindCanvas = document.createElement('canvas');
    combaindCanvas.width = signatureCanvasToDraw.width;
    combaindCanvas.height = signatureCanvasToDraw.height;

    const clonedCtx = combaindCanvas.getContext('2d');
    let sigheight = signatureCanvasToDraw.height * 0.6
    clonedCtx.drawImage(signatureCanvasToDraw, 0, 0, combaindCanvas.width, sigheight);

    clonedCtx.drawImage(canvas, 0, sigheight - 5, combaindCanvas.width, combaindCanvas.height - sigheight + 5);

    return combaindCanvas;
  }

  public addSignatureExtraInformation(signerName: string, signatureCanvasToDraw: HTMLCanvasElement) {
    var canvas = document.createElement('canvas');
    canvas.width = signatureCanvasToDraw.width;
    canvas.height = 60;

    let context = canvas.getContext('2d');
    context.font = '22px Arial';
    context.fillStyle = "#333";


    signerName = "Name: " + signerName;
    let signingDate = new Date().toLocaleString();

    context.fillText(signerName, 0, 21, canvas.width * 0.9);
    context.fillText(signingDate, 0, 50, canvas.width * 0.9);

    var combaindCanvas = document.createElement('canvas');
    combaindCanvas.width = signatureCanvasToDraw.width;
    combaindCanvas.height = signatureCanvasToDraw.height;

    const clonedCtx = combaindCanvas.getContext('2d');
    let sigheight = signatureCanvasToDraw.height * 0.6
    clonedCtx.drawImage(signatureCanvasToDraw, 0, 0, combaindCanvas.width, sigheight);



    clonedCtx.drawImage(canvas, 0, sigheight - 5, combaindCanvas.width, combaindCanvas.height - sigheight + 5);

    return combaindCanvas;
  }

  public cloneCanvas(originalCanvas: HTMLCanvasElement): HTMLCanvasElement {
    let clonedCanvas = document.createElement('canvas');
    clonedCanvas.width = originalCanvas.width;
    clonedCanvas.height = originalCanvas.height;
    const clonedCtx = clonedCanvas.getContext('2d');
    clonedCtx.drawImage(originalCanvas, 0, 0);
    return clonedCanvas;
  }

  public calc_image_size(image) {
    let y = 1;
    if (image.endsWith('==')) {
      y = 2
    }
    const x_size = (image.length * (3 / 4)) - y
    return Math.round(x_size / 1024)
  }

  public generateAcronymFirstName(firstLastName: string[]): string {

    let appendForHeb = this.containsHebrewLetters(firstLastName[0].slice(-1));
    if (appendForHeb) {
      return "." + firstLastName[0] + " " + firstLastName[firstLastName.length - 1].slice(0, 1)
    }

    return firstLastName[0] + " " + firstLastName[firstLastName.length - 1].slice(0, 1).toUpperCase() + "."
  }

  public generateAcronyms(firstLastName: string[]) {
    let appendForHeb = this.containsHebrewLetters(firstLastName[0].slice(-1));

    if (appendForHeb) {
      return "." + firstLastName[0].slice(0, 1) + ". " + firstLastName[firstLastName.length - 1].slice(0, 1)
    }

    return firstLastName[0].slice(0, 1).toUpperCase() + ". " + firstLastName[firstLastName.length - 1].slice(0, 1).toUpperCase() + "."
  }

  private containsHebrewLetters(str): boolean {
    for (var i = 0; i < str.length; i++) {
      if (/^[[\u0590-\u05FF]*$/.test(str.charAt(i))) {
        return true;
      }
    }

    return false;
  }
}