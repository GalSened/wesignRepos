import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'orderBy',
    pure: true
})
export class OrderByPipe implements PipeTransform {

    transform(value: any[], propertyName: string, reverse: boolean = false): any[] {
        if (propertyName && value)
            return reverse ?
                // value.sort((a: any, b: any) => b[propertyName].toString().localeCompare(a[propertyName].toString())) :
                // value.sort((a: any, b: any) => a[propertyName].toString().localeCompare(b[propertyName].toString()));
                value.sort((a: any, b: any) => this.getNestedProp(b, propertyName).localeCompare(this.getNestedProp(a, propertyName))) :
                value.sort((a: any, b: any) => this.getNestedProp(a, propertyName).localeCompare(this.getNestedProp(b, propertyName)));
        else
            return value;
    }

    // dot . convention for delimiting properties
    private getNestedProp(obj: any, props: string) {
        let arr = props.split('.'),
            result = null;
        arr.forEach(el => {
            result = result ? result[el] : obj[el];
        });
        return result != null ? result.toString() : "";
    }

}

