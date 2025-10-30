import { Component, ElementRef, OnInit, Renderer2 } from "@angular/core";
import { PageField, RadioField } from "@models/template-api/page-data-result.model";


@Component({
    selector: "sgn-base-field-component",
    template: ``,
})

export class BaseFieldComponent {

   
    public showNavs: boolean = false;
    constructor(
        protected elRef: ElementRef,
        protected renderer: Renderer2,
    ) { }

    public updateElementPosition(field: PageField) {

        

        const pageRect = this.elRef.nativeElement.parentElement.parentElement.getBoundingClientRect();
        const fieldRect = this.elRef.nativeElement.getBoundingClientRect();

        field.x = (fieldRect.left - pageRect.left) / pageRect.width;
        field.y = (fieldRect.top - pageRect.top) / pageRect.height;
        field.width = fieldRect.width / pageRect.width;
        field.height = fieldRect.height / pageRect.height;
    }

    public filedIntercect(currentField :PageField, pageField: PageField)    
    {
        if(pageField ==  null)
        {
            return false;
        }

        if(currentField.name == pageField.name &&
            currentField.page == pageField.page &&
            currentField.templateId == pageField.templateId &&
            currentField.x == pageField.x &&
            currentField.y == pageField.y )
            {
                
              
                return true;
            }

    }

    public positionElement(field: PageField) {

       
        
        
        const pageRect = this.elRef.nativeElement.parentElement.parentElement.getBoundingClientRect();

        const hvRatio = pageRect.width / pageRect.height;

        
        
        const left = field.x * pageRect.width;
        const top = field.y * pageRect.height;
        const width = field.width * pageRect.width;
        const height = field.height * pageRect.height;

        this.renderer.setStyle(this.elRef.nativeElement, "top", `${top}px`);
        this.renderer.setStyle(this.elRef.nativeElement, "left", `${left}px`);

        this.renderer.setStyle(this.elRef.nativeElement, "width", `${width}px`);
        this.renderer.setStyle(this.elRef.nativeElement, "height", `${height}px`);

    }


}
