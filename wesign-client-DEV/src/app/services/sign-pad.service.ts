import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SignPadService {

  constructor() { }
  public addSignatureExtraInformation(signerName: string, signatureCanvasToDraw: HTMLCanvasElement) {
    var canvas = document.createElement('canvas');
    canvas.width = signatureCanvasToDraw.width;
    canvas.height = 60;

    let context = canvas.getContext('2d');
    context.font = '25px Arial';
    context.fillStyle = "#333";


    signerName = "Name: " + signerName;
    let signingDate = new Date().toLocaleString();


    context.fillText(signerName, 0, 21, canvas.width * 0.9);

    context.fillText(signingDate, 0, 50, canvas.width * 0.9);


    var combaindCanvas = document.createElement('canvas');
    combaindCanvas.width = signatureCanvasToDraw.width;
    combaindCanvas.height = signatureCanvasToDraw.height;

    const clonedCtx = combaindCanvas.getContext('2d');
    let sigheight = combaindCanvas.height * 0.6
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

  public containsHebrewLetters(str: string): boolean {

    for (var i = 0; i < str.length; i++) {
      if (/^[[\u0590-\u05FF]*$/.test(str.charAt(i))) {
        return true;
      }
    }
    return false;
  }
}
