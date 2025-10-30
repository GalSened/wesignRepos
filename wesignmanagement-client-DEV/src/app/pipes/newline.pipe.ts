import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'newline'
})
export class NewlinePipe implements PipeTransform {

  transform(value: any): any {
    return  value.replace(/(?:\r\n|\r|\n)/g, '<br>');
  }

}
